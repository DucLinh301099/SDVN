using SdvnAutoEmail.Models; // Đảm bảo có using này
using System;
using System.Collections.Generic;
using System.Data.SqlClient; // Thêm using này

namespace SdvnAutoEmail.Data
{
    public class EmailTaskRepository : IEmailTaskRepository
    {
        private readonly string _connectionString;

        public EmailTaskRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<EmailTask> GetAllActiveTasks()
        {
            var tasks = new List<EmailTask>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                // Lấy tất cả các tác vụ đang hoạt động và sắp xếp theo ModifiedDate hoặc CreatedDate
                var command = new SqlCommand("SELECT TaskId, TaskName, RecipientEmail, EmailSubject, EmailContent, " +
                                             "ScheduleType, ScheduleValue, LastSentDateTime, IsActive, CreatedDate, ModifiedDate " +
                                             "FROM EmailTasks WHERE IsActive = 1 ORDER BY TaskName", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tasks.Add(new EmailTask
                        {
                            TaskId = reader.GetGuid(reader.GetOrdinal("TaskId")),
                            TaskName = reader.GetString(reader.GetOrdinal("TaskName")),
                            RecipientEmail = reader.GetString(reader.GetOrdinal("RecipientEmail")),
                            EmailSubject = reader.GetString(reader.GetOrdinal("EmailSubject")),
                            EmailContent = reader.GetString(reader.GetOrdinal("EmailContent")),
                            ScheduleType = reader.GetString(reader.GetOrdinal("ScheduleType")),
                            ScheduleValue = reader.GetInt32(reader.GetOrdinal("ScheduleValue")),
                            LastSentDateTime = reader.IsDBNull(reader.GetOrdinal("LastSentDateTime")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("LastSentDateTime")),
                            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                            CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                            ModifiedDate = reader.GetDateTime(reader.GetOrdinal("ModifiedDate"))
                        });
                    }
                }
            }
            return tasks;
        }

        public void UpdateLastSentTime(Guid taskId, DateTime sentTime)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var command = new SqlCommand("UPDATE EmailTasks SET LastSentDateTime = @LastSentDateTime, ModifiedDate = GETDATE() WHERE TaskId = @TaskId", connection);
                command.Parameters.AddWithValue("@LastSentDateTime", sentTime);
                command.Parameters.AddWithValue("@TaskId", taskId);
                command.ExecuteNonQuery();
            }
        }
    }
}