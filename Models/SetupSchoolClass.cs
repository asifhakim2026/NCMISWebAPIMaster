using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupSchoolClass
    {
        [Key]
        public int SetupSchoolClassId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]

      

        public string SetupSchoolClassName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string SetupSchoolClassShortCode { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
