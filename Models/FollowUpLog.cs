using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class FollowUpLog
    {

        [Key]
        [Column("FollowUpLogID")]
        public int FollowUpLogID { get; set; }  // Primary key

        public int LogTypeId { get; set; }

        //[StringLength(100)]
        //public string Type { get; set; }  // The error message
        public string Message { get; set; }  // The error message

        public int PersonId { get; set; }
        public int ProjectId { get; set; }  // Primary key of project
        public DateTime LogDate { get; set; } = DateTime.Now;  // Timestamp of the error


        public DateTime? NextFollowUpdate { get; set; }    // Timestamp of the error

        public bool? IsProfileModification { get; set; }

        public bool? SendNotification { get; set; }

        [StringLength(100)]
        public string ContactType { get; set; }

        [StringLength(300)]
        public string UserName { get; set; }  // Timestamp of the error
    }
}
