using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExactSync.Models
{
    [Table("AuditLog")]
    public class AuditLogModel
    {
        public int Id { get; set; }
        public string Level { get; set; }
        public string EventType { get; set; }
        public string EventAction { get; set; }
        public string Data { get; set; }
        public DateTime UTC { get; set; }
    }
}