using System.Net.Mail;
using System.Net;
using System.Diagnostics; // Dùng để ghi Event Log
using System;

namespace SdvnAutoEmail.Services
{
    public class EmailSender
    {
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;
        private readonly bool _enableSsl;
        private readonly string _fromEmail;

        public EmailSender(string smtpHost, int smtpPort, string smtpUsername, string smtpPassword, bool enableSsl, string fromEmail)
        {
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUsername = smtpUsername;
            _smtpPassword = smtpPassword;
            _enableSsl = enableSsl;
            _fromEmail = fromEmail;
        }

        public void SendEmail(string toEmail, string subject, string body, Guid taskId)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient(_smtpHost))
                {
                    smtpClient.Port = _smtpPort;
                    smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    smtpClient.EnableSsl = _enableSsl;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Timeout = 20000; // Tăng timeout nếu cần (mili giây)

                    using (MailMessage mailMessage = new MailMessage(_fromEmail, toEmail, subject, body))
                    {
                        mailMessage.IsBodyHtml = true; // Đặt true nếu nội dung email là HTML
                        smtpClient.Send(mailMessage);
                        EventLog.WriteEntry("SdvnAutoEmailServiceSource", $"EmailTask {taskId}: Email sent successfully to {toEmail}.", EventLogEntryType.Information);
                    }
                }
            }
            catch (SmtpException smtpEx)
            {
                // Xử lý các lỗi SMTP cụ thể
                EventLog.WriteEntry("SdvnAutoEmailServiceSource", $"EmailTask {taskId}: SMTP Error sending email to {toEmail}. Status: {smtpEx.StatusCode}, Message: {smtpEx.Message}", EventLogEntryType.Error);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi chung
                EventLog.WriteEntry("SdvnAutoEmailServiceSource", $"EmailTask {taskId}: Failed to send email to {toEmail}. Error: {ex.Message}\n{ex.StackTrace}", EventLogEntryType.Error);
            }
        }
    }
}