using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditLogId { get; set; }

      

        [Required]
        public string TableName { get; set; } // Table affected (e.g., PersonalInfo, Address)

        [Required]
        public int RecordId { get; set; } // The primary key of the modified record

        [Required]
        public string Action { get; set; } // "Insert", "Update", or "Delete"

        public string? OldValues { get; set; } // JSON string of old data

        public string? NewValues { get; set; } // JSON string of new data

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Change timestamp


        [StringLength(300)]
        public string? UserName { get; set; } // Store actual username instead of UserId
    }
}
