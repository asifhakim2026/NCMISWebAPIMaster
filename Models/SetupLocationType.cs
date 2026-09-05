using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{

    
    public class SetupLocationType
    {
        [Key]
        public int LocationTypeId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
    
        public string LocationTypeName { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string LocationTypeShortCode { get; set; }

        public int SortOrder { get; set; }
        public bool IsActive { get; set; }


    }
}
