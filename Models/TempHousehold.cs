using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class TempHouseholdRaw
    {
        [Key]
        public int TempHouseholdRawId { get; set; }

        public int BTSNo { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public int? MemberId { get; set; }
        public int? JKId { get; set; }

        // MANDATORY FIELD (Unique search)
        [Required]
        [MaxLength(20)]
        public string CNIC { get; set; }

        public string Name { get; set; }
        public string Relationship { get; set; }

        // Store full row from Excel
        public string RowJson { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
