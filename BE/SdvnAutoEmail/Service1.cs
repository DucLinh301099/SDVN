using System;
using System.ServiceProcess;
using System.Configuration; // Để đọc App.config
using System.Diagnostics;    // Để ghi Event Log
using System.Threading.Tasks; // Để sử dụng async/await với Quartz
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers; // Thêm dòng này cho GroupMatcher
using SdvnAutoEmail.Data;    // Chứa IEmailTaskRepository và EmailTaskRepository
using SdvnAutoEmail.Services; // Chứa EmailSender
using SdvnAutoEmail.Models;  // Chứa EmailTask
using SdvnAutoEmail.Jobs;
using System.Linq;

namespace SdvnAutoEmail
{
    public partial class Service1 : ServiceBase
    {
        private IScheduler _scheduler; // Scheduler chính của Quartz
        private readonly IEmailTaskRepository _emailTaskRepository;
        private readonly EmailSender _emailSender;
        private const string EventLogSource = "SdvnAutoEmailServiceSource"; // Nguồn Event Log

        public Service1()
        {
            InitializeComponent();

            // Cấu hình Event Log cho Service
            this.EventLog.Source = EventLogSource;
            this.EventLog.Log = "Application";
            if (!EventLog.SourceExists(this.EventLog.Source))
            {
                EventLog.CreateEventSource(this.EventLog.Source, this.EventLog.Log);
            }

            // Đọc Connection String từ App.config
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            _emailTaskRepository = new EmailTaskRepository(connectionString);

            // Đọc cấu hình SMTP từ App.config
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            string smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
            bool enableSsl = bool.Parse(ConfigurationManager.AppSettings["EnableSsl"]);
            string fromEmail = ConfigurationManager.AppSettings["FromEmail"];

            _emailSender = new EmailSender(smtpHost, smtpPort, smtpUsername, smtpPassword, enableSsl, fromEmail);
        }

        protected override async void OnStart(string[] args)
        {
            EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Starting Quartz.NET scheduler...", EventLogEntryType.Information);

            try
            {
                // 1. Tạo Scheduler Factory
                // Quartz.NET tự động đọc cấu hình từ App.config nếu có (ví dụ: threadPool.threadCount)
                ISchedulerFactory schedFactory = new StdSchedulerFactory();
                _scheduler = await schedFactory.GetScheduler();

                // 2. Thiết lập Context data cho Job (để truyền các dependency vào Jobs)
                _scheduler.Context.Put("emailTaskRepository", _emailTaskRepository);
                _scheduler.Context.Put("emailSender", _emailSender);
                _scheduler.Context.Put("eventLogSource", EventLogSource); // Để Jobs có thể ghi log

                // 3. Bắt đầu Scheduler
                await _scheduler.Start();
                EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Quartz Scheduler started.", EventLogEntryType.Information);

                // 4. Lên lịch tất cả các tác vụ email hiện có từ DB lần đầu
                await ScheduleAllEmailTasks();

                // 5. Thiết lập một Job định kỳ để tải lại và cập nhật lịch trình
                // Điều này giúp Service tự động nhận diện tác vụ mới/thay đổi mà không cần khởi động lại
                IJobDetail reloadJob = JobBuilder.Create<EmailTaskReloadJob>()
                    .WithIdentity("emailTaskReloadJob", "maintenance")
                    .Build();

                // Trigger chạy mỗi 5 phút
                ITrigger reloadTrigger = TriggerBuilder.Create()
                    .WithIdentity("reloadTrigger", "maintenance")
                    .StartNow()
                    .WithSimpleSchedule(x => x.WithIntervalInMinutes(5).RepeatForever())
                    .Build();

                await _scheduler.ScheduleJob(reloadJob, reloadTrigger);
                EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Scheduled EmailTaskReloadJob to run every 5 minutes.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Error during Quartz startup: {ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error);
                // Cân nhắc dừng Service nếu không thể khởi động Quartz để tránh chạy trong trạng thái lỗi
                this.Stop();
            }
        }

        protected override async void OnStop()
        {
            EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Stopping Quartz.NET scheduler...", EventLogEntryType.Information);
            if (_scheduler != null && _scheduler.IsStarted)
            {
                // Chờ các job đang chạy hoàn thành trước khi tắt
                await _scheduler.Shutdown(waitForJobsToComplete: true);
                EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Quartz Scheduler shut down.", EventLogEntryType.Information);
            }
            EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Service stopped successfully.", EventLogEntryType.Information);
        }

        protected override void OnPause()
        {
            EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Pausing.", EventLogEntryType.Information);
            // Có thể tạm dừng scheduler nếu muốn các job ngừng chạy khi pause
            if (_scheduler != null && _scheduler.IsStarted)
            {
                _scheduler.PauseAll();
            }
            base.OnPause();
        }

        protected override void OnContinue()
        {
            EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Continuing.", EventLogEntryType.Information);
            // Tiếp tục scheduler khi service được tiếp tục
            if (_scheduler != null && _scheduler.IsStarted)
            {
                _scheduler.ResumeAll();
            }
            base.OnContinue();
        }

        // Phương thức này sẽ đọc từ DB và lên lịch các tác vụ email trong Quartz
        // Được gọi khi service khởi động và bởi EmailTaskReloadJob
        public async Task ScheduleAllEmailTasks()
        {
            try
            {
                var tasks = _emailTaskRepository.GetAllActiveTasks();
                EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Found {tasks.Count} active email tasks to schedule/reschedule.", EventLogEntryType.Information);

                // Lấy danh sách các JobKey hiện đang được lên lịch cho email-sending-jobs
                var currentlyScheduledJobKeys = await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("email-sending-jobs"));
                var currentlyScheduledTaskIds = currentlyScheduledJobKeys.Select(jk => Guid.Parse(jk.Name)).ToHashSet();

                // Các TaskId từ DB
                var dbTaskIds = tasks.Select(t => t.TaskId).ToHashSet();

                // 1. Xóa các Job không còn tồn tại trong DB
                foreach (var jobKey in currentlyScheduledJobKeys)
                {
                    Guid scheduledTaskId = Guid.Parse(jobKey.Name);
                    if (!dbTaskIds.Contains(scheduledTaskId))
                    {
                        await _scheduler.DeleteJob(jobKey);
                        EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Deleted unscheduled Job {jobKey.Name}.", EventLogEntryType.Information);
                    }
                }

                // 2. Thêm mới hoặc cập nhật các Job từ DB
                foreach (var dbTask in tasks)
                {
                    JobKey jobKey = new JobKey(dbTask.TaskId.ToString(), "email-sending-jobs");
                    ITrigger newPotentialTrigger = CreateTriggerForEmailTask(dbTask, jobKey.Name, jobKey.Group);

                    if (newPotentialTrigger == null) continue; // Bỏ qua nếu không tạo được trigger hợp lệ

                    if (await _scheduler.CheckExists(jobKey))
                    {
                        // Job đã tồn tại, kiểm tra và cập nhật trigger nếu cần
                        var currentTriggers = await _scheduler.GetTriggersOfJob(jobKey);
                        ITrigger oldTrigger = currentTriggers.FirstOrDefault(); // Giả sử mỗi job chỉ có 1 trigger

                        if (oldTrigger is ICronTrigger oldCronTrigger && newPotentialTrigger is ICronTrigger newCronTrigger)
                        {
                            if (oldCronTrigger.CronExpressionString != newCronTrigger.CronExpressionString)
                            {
                                await _scheduler.RescheduleJob(oldTrigger.Key, newCronTrigger);
                                EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Rescheduled Job {jobKey.Name} due to schedule change (Old: {oldCronTrigger.CronExpressionString}, New: {newCronTrigger.CronExpressionString}).", EventLogEntryType.Information);
                            }
                            // else: Lịch trình không đổi, không làm gì
                        }
                        else if (oldTrigger == null)
                        {
                            // Trường hợp job có thể đã tồn tại nhưng trigger bị mất (lỗi?), tạo lại
                            EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Re-scheduling trigger for existing Job {jobKey.Name} (trigger was missing).", EventLogEntryType.Warning);
                            await _scheduler.ScheduleJob(JobBuilder.Create<EmailSendJob>().WithIdentity(jobKey).Build(), newPotentialTrigger);
                        }
                        // else: Trigger type changed or some other complex scenario, may need more specific handling
                    }
                    else
                    {
                        // Job chưa tồn tại, tạo mới
                        IJobDetail newJob = JobBuilder.Create<EmailSendJob>()
                            .WithIdentity(jobKey)
                            .UsingJobData("TaskId", dbTask.TaskId.ToString())
                            .UsingJobData("RecipientEmail", dbTask.RecipientEmail)
                            .UsingJobData("EmailSubject", dbTask.EmailSubject)
                            .UsingJobData("EmailContent", dbTask.EmailContent)
                            .Build();

                        await _scheduler.ScheduleJob(newJob, newPotentialTrigger);
                        EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Scheduled new Job {newJob.Key.Name} with schedule {((ICronTrigger)newPotentialTrigger).CronExpressionString}.", EventLogEntryType.Information);
                    }
                }
                EventLog.WriteEntry(EventLogSource, "SdvnAutoEmail Service: Email task synchronization complete.", EventLogEntryType.Information);

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Error scheduling email tasks: {ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error);
            }
        }

        // Tạo Trigger từ thông tin EmailTask
        // IMPORTANT: Logic chuyển đổi ScheduleType sang Cron Expression
        private ITrigger CreateTriggerForEmailTask(EmailTask task, string jobName, string jobGroup)
        {
            string cronExpression = "";
            switch (task.ScheduleType.ToLower())
            {
                case "minute":
                    // Chạy mỗi N phút (ví dụ: N=5 -> 0 0/5 * ? * *)
                    cronExpression = $"0 0/{task.ScheduleValue} * ? * *";
                    break;
                case "hourly":
                    // Chạy mỗi N giờ (ví dụ: N=1 -> 0 0 0/1 ? * *)
                    cronExpression = $"0 0 0/{task.ScheduleValue} ? * *";
                    break;
                case "daily":
                    // Chạy vào 9:00 AM mỗi N ngày (cần thêm cột giờ/phút vào DB nếu muốn tùy chỉnh)
                    cronExpression = $"0 0 9 */{task.ScheduleValue} * ?";
                    break;
                case "weekly":
                    // Chạy vào 9:00 AM mỗi thứ Hai (MON) của mỗi N tuần
                    // Cần logic phức tạp hơn nếu muốn chọn ngày cụ thể trong tuần
                    // Ví dụ: N=1 -> 0 0 9 ? * MON
                    // Nếu ScheduleValue là số tuần, thì không nên dùng MON/N, mà cần tính toán khác
                    // Cho ví dụ này, giả sử ScheduleValue là số tuần và luôn chạy vào Thứ Hai.
                    // Nếu muốn chọn ngày linh hoạt hơn, cần lưu MON, TUE... hoặc số 1-7 vào DB
                    cronExpression = $"0 0 9 ? * MON"; // Để đơn giản, luôn chạy vào thứ 2, cứ N tuần 1 lần là phức tạp hơn.
                    // Nếu ScheduleValue là số ngày trong tuần (1=CN, 2=T2...), sẽ dùng:
                    // cronExpression = $"0 0 9 ? * {GetDayOfWeekCronString(task.ScheduleValue)}";
                    break;
                // Thêm các loại chu kỳ khác và logic Cron phức tạp hơn nếu cần
                default:
                    EventLog.WriteEntry(EventLogSource, $"SdvnAutoEmail Service: Unsupported schedule type '{task.ScheduleType}' for Task ID: {task.TaskId}", EventLogEntryType.Warning);
                    return null;
            }

            // Đảm bảo thời gian bắt đầu sau thời gian gửi cuối cùng (nếu có)
            // Hoặc bắt đầu ngay lập tức
            DateTimeOffset startAt = task.LastSentDateTime.HasValue ?
                                     new DateTimeOffset(task.LastSentDateTime.Value.AddMinutes(1)) :
                                     DateTimeOffset.Now;

            return TriggerBuilder.Create()
                .WithIdentity($"trigger-{jobName}", $"trigger-group-{jobGroup}")
                .WithCronSchedule(cronExpression)
                .ForJob(jobName, jobGroup)
                .StartAt(startAt)
                .Build();
        }

        // Helper method cho cron (ví dụ nếu ScheduleValue là số ngày trong tuần)
        /*
        private string GetDayOfWeekCronString(int dayOfWeekValue)
        {
            switch (dayOfWeekValue)
            {
                case 1: return "SUN";
                case 2: return "MON";
                case 3: return "TUE";
                case 4: return "WED";
                case 5: return "THU";
                case 6: return "FRI";
                case 7: return "SAT";
                default: return "?"; // Không xác định
            }
        }
        */
        // Phương thức PUBLIC để gọi OnStart từ Program.cs cho mục đích DEBUG
        public async void Debug_OnStart(string[] args)
        {
            await Task.Run(() => OnStart(args)); // Gọi OnStart trên một luồng khác để không chặn Console
        }

        // Phương thức PUBLIC để gọi OnStop từ Program.cs cho mục đích DEBUG
        public async void Debug_OnStop()
        {
            await Task.Run(() => OnStop()); // Gọi OnStop trên một luồng khác để không chặn Console
        }
    }
}