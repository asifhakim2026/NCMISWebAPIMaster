using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class FamilyGroup
    {
        [Key]
        public int FamilyId { get; set; }

        [Required]
        public Guid FamilyGroupGuid { get; set; }

        [Required, MaxLength(100)]
        public string FamilyGroupCode { get; set; } // e.g. FG-202504-0001

        public int? HeadPersonId { get; set; } // FK to PersonalInfo
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(500)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(500)]
        public string? UpdatedBy { get; set; }
    }
}
