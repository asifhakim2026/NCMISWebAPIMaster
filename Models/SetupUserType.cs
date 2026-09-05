using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupUserType
    {

        [Key]
        public int SetupUserTypeId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]

        public string SetupUserTypeName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string SetupUserTypeShortCode { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
