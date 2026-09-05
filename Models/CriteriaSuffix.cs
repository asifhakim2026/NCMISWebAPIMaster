using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class CriteriaSuffix
    {
        [Key]
        public int SuffixID { get; set; } // Unique Suffix ID

        [Required]
        [StringLength(50)]
        public string SuffixName { get; set; } // Example: "Years", "USD", "KG", "Meters"

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
