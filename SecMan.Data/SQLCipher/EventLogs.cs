using SecMan.BL.Common;
using SecMan.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecMan.Data.SQLCipher
{
    public class EventLogs
    {
        [Key]
        public int EventLogId { get; set; }
        public string EventType { get; set; }
        public string EventSubType { get; set; }
        public EventSeverity Severity { get; set; }
        public User User { get; set; }
        public User SigningUser { get; set; }
        public User AuthorizingUser { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
