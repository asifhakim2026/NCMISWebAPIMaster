using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class Criteria
    {
        [Key]
        public int CriteriaID { get; set; } // Unique ID

        [Required(ErrorMessage = "Criteria is required.")]
        [StringLength(255)]
        public string CriteriaName { get; set; } // Example: Age, Income, Education

        [Required(ErrorMessage = "ShortCode is required.")]
        [StringLength(50)]
        public string ShortCode { get; set; } // Example: "gender", "marital_status", "income_level" (🔹 Unique & Fixed)

        [Required(ErrorMessage = "Data Type is required.")]
        [StringLength(50)]
        public string DataType { get; set; } // Example: "Number", "Text", "Dropdown"

        [Required(ErrorMessage = "Criteria Category is required.")]
        public int CategoryID { get; set; }


        [NotMapped]
        public virtual CriteriaCategory Category { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; } = true; // Active status

        // Add this navigation property

        [NotMapped]
        public virtual ICollection<CriteriaValue> CriteriaValues { get; set; } // New navigation property
    }

}
