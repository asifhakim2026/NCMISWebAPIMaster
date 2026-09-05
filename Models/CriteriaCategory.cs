using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class CriteriaCategory
    {
        [Key]
        public int CategoryID { get; set; } // Unique ID for the category

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } // E.g., Personal, Financial, Education

        [Required]
        [StringLength(500)]
        public string CategoryDescription { get; set; } // Brief description of the category

        [Column("IsActive")]
        public bool IsActive { get; set; } = true; // Active or inactive
    }

}
