using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupSchoolCaseType
    {
        [Key]
        public int SetupSchoolCaseTypeId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]

        public string SetupSchoolCaseTypeName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string SetupSchoolCaseTypeShortCode { get; set; }

        public string SetupSchoolCaseTypeCategory { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public bool IsApplicableForFeeRemission { get; set; }
    }
}
