using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class ExportAuditLog
    {

        [Key]
        public int ExportAuditLogId { get; set; }

        [StringLength(500)]
        public string ExportedBy { get; set; } 

        [StringLength(500)]
        public string ModuleName { get; set; }

        [StringLength(500)]
        public string ExportedFileName { get; set; }


        public DateTime ExportedAt { get; set; }

        public string FiltersJson { get; set; }
        public string? AdditionalNotes { get; set; }
    }
}
