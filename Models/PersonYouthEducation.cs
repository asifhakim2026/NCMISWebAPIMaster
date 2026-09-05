using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonYouthEducation
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "PersonId is required.")]
        public int PersonId { get; set; } // ✅ Foreign Key to PersonalInfo

        public int? FamilyId { get; set; }


        [StringLength(100)]
        public string Class { get; set; }


        [StringLength(1000)]
        public string CenterName { get; set; }


        [StringLength(100)]
        public string CompletionStatus { get; set; }

        public DateTime? CompletionDate { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "CreatedBy is required.")]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;



        [StringLength(100)]
        public string? ReasonForInActive { get; set; }
        [StringLength(500)]
        public string? DescriptionForInActive { get; set; }



    }
}
