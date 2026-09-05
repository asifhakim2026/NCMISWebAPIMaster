using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class DiseaseSetup
    {
        [Key]
        public int DiseaseSetupId { get; set; }

        [Required]
        [StringLength(255)]
        public string DiseaseName { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("Datetime")]
        public DateTime InsertDate { get; set; }

        [Column("CreatedBy")]
        [StringLength(300)]
        public string? CreatedBy { get; set; }


        [Column("ModifiedBy")]
        [StringLength(300)]
        public string? ModifiedBy { get; set; }

        [Column("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }

        [Column("Remarks")]
        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
