using Quartz;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SdvnAutoEmail.Data;
using SdvnAutoEmail.Models;
using Quartz.Impl.Matchers; // THÊM DÒNG NÀY

namespace SdvnAutoEmail.Jobs
{
    [DisallowConcurrentExecution] // Đảm bảo chỉ có một instance của job reload chạy tại một thời điểm
    public class EmailTaskReloadJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var emailTaskRepository = context.Scheduler.Context.Get("emailTaskRepository") as IEmailTaskRepository;
            var scheduler = context.Scheduler;
            var eventLogSource = context.Scheduler.Context.Get("eventLogSource") as string;

            if (emailTaskRepository == null || scheduler == null || string.IsNullOrEmpty(eventLogSource))
            {
                EventLog.WriteEntry(eventLogSource, "EmailTaskReloadJob: Critical: Dependencies not found for reloading. Cannot proceed.", EventLogEntryType.Error);
                return;
            }

            EventLog.WriteEntry(eventLogSource, "EmailTaskReloadJob: Checking for updated email tasks...", EventLogEntryType.Information);

            try
            {
                var currentDbTasks = emailTaskRepository.GetAllActiveTasks();

                // Lấy tất cả các JobKey thuộc nhóm "email-sending-jobs"
                var scheduledJobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("email-sending-jobs"));
                var scheduledTaskIds = scheduledJobKeys.ToDictionary(k => Guid.Parse(k.Name), k => k); // Dùng Dictionary để tra cứu nhanh hơn

                // 1. Xóa các Job không còn tồn tại trong DB hoặc đã bị vô hiệu hóa
                foreach (var jobKey in scheduledJobKeys)
                {
                    Guid scheduledTaskId = Guid.Parse(jobKey.Name);
                    // Kiểm tra xem task này có còn trong DB và active không
                    if (!currentDbTasks.Any(t => t.TaskId == scheduledTaskId && t.IsActive))
                    {
                        await scheduler.DeleteJob(jobKey);
                        EventLog.WriteEntry(eventLogSource, $"EmailTaskReloadJob: Deleted unscheduled/inactive Job {jobKey.Name}.", EventLogEntryType.Information);
                    }
                }

                // 2. Thêm mới hoặc cập nhật các Job từ DB
                foreach (var dbTask in currentDbTasks)
                {
                    if (!dbTask.IsActive) continue; // Chỉ xử lý các tác vụ đang hoạt động

                    JobKey jobKey = new JobKey(dbTask.TaskId.ToString(), "email-sending-jobs");

                    // Khai báo newPotentialTrigger ở đây để nó có thể truy cập được ở mọi nơi trong vòng lặp này
                    ITrigger newPotentialTrigger = CreateTriggerForEmailTask(dbTask, jobKey.Name, jobKey.Group, eventLogSource);

                    if (newPotentialTrigger == null) // Nếu không tạo được trigger hợp lệ, bỏ qua task này
                    {
                        EventLog.WriteEntry(eventLogSource, $"EmailTaskReloadJob: Could not create valid trigger for Task ID: {dbTask.TaskId}. Skipping.", EventLogEntryType.Warning);
                        continue;
                    }

                    if (await scheduler.CheckExists(jobKey))
                    {
                        // Job đã tồn tại, kiểm tra và cập nhật trigger nếu cần
                        var currentTriggers = await scheduler.GetTriggersOfJob(jobKey);
                        ITrigger oldTrigger = currentTriggers.FirstOrDefault(); // Giả sử mỗi job chỉ có 1 trigger

                        if (oldTrigger is ICronTrigger oldCronTrigger && newPotentialTrigger is ICronTrigger newCronTrigger)
                        {
                            if (oldCronTrigger.CronExpressionString != newCronTrigger.CronExpressionString)
                            {
                                await scheduler.RescheduleJob(oldTrigger.Key, newCronTrigger);
                                EventLog.WriteEntry(eventLogSource, $"EmailTaskReloadJob: Rescheduled Job {jobKey.Name} due to schedule change (Old: {oldCronTrigger.CronExpressionString}, New: {newCronTrigger.CronExpressionString}).", EventLogEntryType.Information);
                            }
                            // else: Lịch trình không đổi, không làm gì
                        }
                        else if (oldTrigger == null)
                        {
                            // Trường hợp job có thể đã tồn tại nhưng trigger bị mất (lỗi?), tạo lại trigger
                            // Cần đảm bảo JobDetail vẫn còn, nếu không phải tạo lại cả job
                            IJobDetail existingJob = await scheduler.GetJobDetail(jobKey);
                            if (existingJob != null)
                            {
                                EventLog.WriteEntry(eventLogSource, $"EmailTaskReloadJob: Re-scheduling trigger for existing Job {jobKey.Name} (trigger was missing).", EventLogEntryType.Warning);
                                await scheduler.ScheduleJob(existingJob, newPotentialTrigger);
                            }
                            else
                            {
                                // Job cũng bị mất, tạo lại cả Job và Trigger
                                await ScheduleNewEmailJob(scheduler, dbTask, jobKey, newPotentialTrigger, eventLogSource);
                            }
                        }
                        // else: Trigger type changed or some other complex scenario, may need more specific handling
                    }
                    else
                    {
                        // Job chưa tồn tại, tạo mới
                        await ScheduleNewEmailJob(scheduler, dbTask, jobKey, newPotentialTrigger, eventLogSource);
                    }
                }
                EventLog.WriteEntry(eventLogSource, "EmailTaskReloadJob: Email task synchronization complete.", EventLogEntryType.Information);

            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(eventLogSource, $"EmailTaskReloadJob: Error during task reload: {ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error);
            }
            await Task.CompletedTask;
        }

        // Helper để tạo trigger từ thông tin EmailTask
        private ITrigger CreateTriggerForEmailTask(EmailTask task, string jobName, string jobGroup, string eventLogSource)
        {
            string cronExpression = "";
            switch (task.ScheduleType.ToLower())
            {
                case "minute":
                    cronExpression = $"0 0/{task.ScheduleValue} * ? * *";
                    break;
                case "hourly":
                    cronExpression = $"0 0 0/{task.ScheduleValue} ? * *";
                    break;
                case "daily":
                    cronExpression = $"0 0 9 */{task.ScheduleValue} * ?"; // Chạy vào 9 AM mỗi N ngày
                    break;
                case "weekly":
                    // Chạy vào 9 AM mỗi thứ Hai (MON) của mỗi N tuần
                    // Để đơn giản ví dụ, giả sử ScheduleValue là số tuần, và luôn chạy vào Thứ Hai.
                    // Nếu muốn linh hoạt hơn, bạn cần lưu DayOfWeek vào DB hoặc Cron Expression đầy đủ.
                    cronExpression = $"0 0 9 ? * MON";
                    break;
                default:
                    EventLog.WriteEntry(eventLogSource, $"EmailTaskReloadJob: Unsupported schedule type '{task.ScheduleType}' for Task ID: {task.TaskId}", EventLogEntryType.Warning);
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

        // Helper để tạo và lên lịch một Job mới
        private async Task ScheduleNewEmailJob(IScheduler scheduler, EmailTask dbTask, JobKey jobKey, ITrigger trigger, string eventLogSource)
        {
            IJobDetail newJob = JobBuilder.Create<EmailSendJob>()
                .WithIdentity(jobKey)
                .UsingJobData("TaskId", dbTask.TaskId.ToString())
                .UsingJobData("RecipientEmail", dbTask.RecipientEmail)
                .UsingJobData("EmailSubject", dbTask.EmailSubject)
                .UsingJobData("EmailContent", dbTask.EmailContent)
                .Build();

            await scheduler.ScheduleJob(newJob, trigger);
            EventLog.WriteEntry(eventLogSource, $"EmailTaskReloadJob: Scheduled new Job {newJob.Key.Name} with schedule {((ICronTrigger)trigger).CronExpressionString}.", EventLogEntryType.Information);
        }
    }
}