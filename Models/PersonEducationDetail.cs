using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class PersonEducationDetail
    {
        [Key]
        public int EducationId { get; set; }

        [Required]
        public int PersonId { get; set; }

        [Required]
        public string BoardType { get; set; }  // e.g., SSC, HSC

        [Required]
        public string Board { get; set; }  // e.g., Karachi Board

        [Required]
        public string Group { get; set; }  // e.g., Science, Arts

        [Required]
        public string DegreeType { get; set; }  // e.g., Matric, Bachelors

        [Required]
        public string FieldOfStudy { get; set; }  // e.g., Pre-Medical

        [Required]
        public string InstitutionName { get; set; }

        public string? City { get; set; }

        public string? CourseDuration { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PassingDate { get; set; }

        public int? TotalMarks { get; set; }
        public int? ObtainedMarks { get; set; }

        public double? Percentage => (TotalMarks > 0) ? (double)ObtainedMarks / TotalMarks * 100 : 0;

        public string? MarksheetPath { get; set; }

        public string? Remarks { get; set; }
        public bool IsActive { get; set; }

        [StringLength(100)]
        public string? ReasonForInActive { get; set; }
        [StringLength(500)]
        public string? DescriptionForInActive { get; set; }
        public bool Isongoing { get; set; }

        public bool Isnotknown { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }


        [NotMapped]
        public List<PersonEducationFundingSource> FundingSources { get; set; }

    }



}
