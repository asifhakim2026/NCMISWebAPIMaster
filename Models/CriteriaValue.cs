using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class CriteriaValue
    {
        [Key]
        public int ValueID { get; set; }

        [Required]
        public int CriteriaID { get; set; }


        [NotMapped]
        public virtual Criteria Criteria { get; set; }

        // Numeric Range (Used for Age, Income, etc.)
        public int? MinValue { get; set; }  // ✅ Optional for numeric ranges
        public int? MaxValue { get; set; }  // ✅ Optional for numeric ranges

        // Text Value (Used for Dropdowns like Marital Status, Education Level, etc.)
        [StringLength(255)]
        public string? TextValue { get; set; }

        // Optional Suffix (Years, USD, kg, etc.)
        public int? SuffixID { get; set; }


        [NotMapped]
        public virtual CriteriaSuffix? Suffix { get; set; }

        [Required]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

}
