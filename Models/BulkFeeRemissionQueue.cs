using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class BulkFeeRemissionQueue
    {

        [Key]
        public int QueueId { get; set; }

        // The original pasted row in JSON format
        public string RawJson { get; set; }

        // Pending | Processing | Completed | Error
        public string Status { get; set; }

        // Number of attempts (worker increases this every time)
        public int Attempts { get; set; } = 0;

        public string? ResultMessage { get; set; }

        public DateTime InsertedOn { get; set; } = DateTime.Now;

        public DateTime? UpdatedOn { get; set; }

        // Returned when success
        public Guid? FeeRemissionGuid { get; set; }

        // Optionally store case number generated
        public string? CaseNumber { get; set; }
    }
}
