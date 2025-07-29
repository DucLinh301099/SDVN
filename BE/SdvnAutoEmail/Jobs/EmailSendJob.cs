using Quartz;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SdvnAutoEmail.Data;    // Thêm using này
using SdvnAutoEmail.Services; // Thêm using này

namespace SdvnAutoEmail.Jobs
{
    [DisallowConcurrentExecution] // Đảm bảo chỉ một instance của job này chạy tại một thời điểm (cho cùng một Trigger)
    public class EmailSendJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            // Lấy dữ liệu JobData Map mà chúng ta đã đặt khi tạo JobDetail
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // Đọc dữ liệu tác vụ từ JobDataMap
            Guid taskId = Guid.Parse(dataMap.GetString("TaskId"));
            string recipientEmail = dataMap.GetString("RecipientEmail");
            string emailSubject = dataMap.GetString("EmailSubject");
            string emailContent = dataMap.GetString("EmailContent");

            // Lấy các Dependency từ Scheduler Context
            var emailSender = context.Scheduler.Context.Get("emailSender") as EmailSender;
            var emailTaskRepository = context.Scheduler.Context.Get("emailTaskRepository") as IEmailTaskRepository;
            var eventLogSource = context.Scheduler.Context.Get("eventLogSource") as string;

            if (emailSender == null || emailTaskRepository == null || string.IsNullOrEmpty(eventLogSource))
            {
                EventLog.WriteEntry("SdvnAutoEmailServiceSource", $"EmailSendJob {taskId}: Critical: Dependencies not found in scheduler context. Cannot proceed.", EventLogEntryType.Error);
                return;
            }

            EventLog.WriteEntry(eventLogSource, $"EmailSendJob {taskId}: Executing email send task for {recipientEmail}...", EventLogEntryType.Information);

            try
            {
                // Gửi email
                emailSender.SendEmail(recipientEmail, emailSubject, emailContent, taskId);

                // Cập nhật thời gian gửi cuối cùng vào DB
                emailTaskRepository.UpdateLastSentTime(taskId, DateTime.Now);

                EventLog.WriteEntry(eventLogSource, $"EmailSendJob {taskId}: Email sent and last sent time updated successfully.", EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                // Mọi lỗi trong quá trình gửi email hoặc cập nhật DB đều được ghi log
                EventLog.WriteEntry(eventLogSource, $"EmailSendJob {taskId}: Error during execution: {ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error);
            }

            await Task.CompletedTask; // Để báo hiệu rằng tác vụ async đã hoàn thành
        }
    }
}