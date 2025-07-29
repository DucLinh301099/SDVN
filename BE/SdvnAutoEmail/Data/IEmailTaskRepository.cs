using SdvnAutoEmail.Models; // Đảm bảo có using này
using System;
using System.Collections.Generic;

namespace SdvnAutoEmail.Data
{
    public interface IEmailTaskRepository
    {
        List<EmailTask> GetAllActiveTasks();
        void UpdateLastSentTime(Guid taskId, DateTime sentTime);
    }
}