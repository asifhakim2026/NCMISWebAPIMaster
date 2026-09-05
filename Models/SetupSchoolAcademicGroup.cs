using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupSchoolAcademicGroup
    {

        [Key]
        public int SetupSchoolAcademicGroupId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]

        public string SetupSchoolAcademicGroupName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string SetupSchoolAcademicGroupShortCode { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
