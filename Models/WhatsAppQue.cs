using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class WhatsAppQue
    {
        [Key]
        public int QueueID { get; set; }

        [Required]
        public int RecordId { get; set; }

        [Required]
        public string Module { get; set; }

        [Required]
        [MaxLength(20)]
        public string MobileNumber { get; set; }

        [Required]
        [MaxLength(2000)]
        public string MessageText { get; set; }

        [MaxLength(50)]
        public string MessageType { get; set; } // e.g., "Reappointment", "Cancellation", "Reminder"

        public bool IsSent { get; set; } = false;

        public DateTime? SentDateTime { get; set; }

        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string CreatedBy { get; set; }

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        [DefaultValue(0)]
        public int? TryCount { get; set; }
    }

}
