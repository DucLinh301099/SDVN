using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdvnAutoEmail.Models
{
    public class EmailTask
    {
        public Guid TaskId { get; set; }
        public string TaskName { get; set; }
        public string RecipientEmail { get; set; }
        public string EmailSubject { get; set; }
        public string EmailContent { get; set; }
        public string ScheduleType { get; set; }    // Ví dụ: 'Minute', 'Hourly', 'Daily', 'Weekly'
        public int ScheduleValue { get; set; }      // Ví dụ: 5 (mỗi 5 phút), 24 (mỗi 24 giờ)
        public DateTime? LastSentDateTime { get; set; } // Thời gian gửi gần nhất
        public bool IsActive { get; set; }          // Trạng thái kích hoạt tác vụ
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
