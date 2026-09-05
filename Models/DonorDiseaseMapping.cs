using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class DonorDiseaseMapping
    {
        [Key]
        public int MappingId { get; set; }

        [Required]
        public int DonorId { get; set; }

        [Required]
        public int DiseaseSetupId { get; set; }

        // ✅ Extra fields
        [Range(0, 100)]
        public decimal Percentage { get; set; }  // e.g., 25.5 (%)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountAgreed { get; set; }

        // ✅ Navigation
        public Donor Donor { get; set; }
        public DiseaseSetup Disease { get; set; }
    }
}
