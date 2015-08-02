using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ExactSync.Models
{
    [Table("SyncHistory")]
    public class SyncHistoryModel
    {
        public int Id { get; set; }
        public string AspNetUID { get; set; }
        [DisplayName("Date")]
        public DateTime DateTimeUtc { get; set; }
        [DisplayName("Total Files")]
        public int TotalFiles { get; set; }
        public bool Status { get; set; }
    }

    [Table("SyncHistoryDetail")]
    public class SyncHistoryDetailModel
    {
        public int Id { get; set; }
        public int SyncHistoryId { get; set; }
        [DisplayName("Name")]
        public string FileName { get; set; }
        [DisplayName("Type")]
        public string MimeType { get; set; }
        public string Size { get; set; }
        [DisplayName("Date Modified")]
        public string DateTimeModified { get; set; }
        public string Rev { get; set; }
        public string Action { get; set; }
        public bool Status { get; set; }
        public string Remark { get; set; }
    }
}