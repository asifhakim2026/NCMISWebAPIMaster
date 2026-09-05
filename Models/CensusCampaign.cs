using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class CensusCampaign
    {
        [Key]
        public int CensusCampaignID { get; set; }

        [Required]
        public Guid CampaignGuid { get; set; } = Guid.NewGuid();

        [Required, StringLength(200)]
        public string Name { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Required, StringLength(100)]
        public string ObjectiveType { get; set; } 

        public string? LocationScope { get; set; } // Optional field for future use

        // Audit fields
        [Required]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string? URL { get; set; }
    }
}
