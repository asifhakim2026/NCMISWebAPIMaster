using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupHouseHoldCategory
    {
        [Key]
        public int SetupHouseHoldCategoryId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]



        public string SetupHouseHoldCategoryName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string SupportType { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string SetupHouseHoldCategoryShortCode { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? Icon { get; set; }


        [StringLength(20)]
        public string? MainIcon { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
