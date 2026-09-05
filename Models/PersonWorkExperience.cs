using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class PersonWorkExperience
    {
        [Key]
        public int WorkExperienceId { get; set; }

        public int PersonId { get; set; }

        public int? FamilyId { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [StringLength(500)]
        public string? NameOfEmployer { get; set; }

        [StringLength(1000)]
        public string? AddressOfEmployer { get; set; }

        // Replacing DesignationId with standardized designation text

        [StringLength(100)]
        public string? Designation { get; set; }
        [StringLength(1000)]
        public string? MajorResponsibility { get; set; }
        public string? IsOngoing { get; set; }

        [StringLength(100)]
        public string? EmploymentStatus { get; set; } // E.g., Unemployed, Retired, Employed
        public decimal? IncomePerMonth { get; set; }


        [StringLength(100)]
        public string? ReasonForInActive { get; set; }
        [StringLength(500)]
        public string? DescriptionForInActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; }

        [NotMapped]
        public List<PersonWorkIncomeComponent> PersonComponentList { get; set; }
    }
}
