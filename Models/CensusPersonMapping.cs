using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class CensusPersonMapping
    {
        [Key]
        public int CensusPersonMappingID { get; set; }

        [Required]
        public Guid MappingGuid { get; set; } = Guid.NewGuid();

        [Required]
        public int PersonID { get; set; } // FK to your existing Person table

        [Required]
        public int CensusCampaignID { get; set; } // FK to CensusCampaign

        [StringLength(50)]
        public string? IPAddress { get; set; }

        public int? JKID { get; set; } // Optional FK to JK table

        [Required, StringLength(100)]
        public string RegisteredBy { get; set; }

        [Required]
        public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

        public bool IsSelfRegistered { get; set; }

        [StringLength(100)]
        public string? Status { get; set; } // e.g., "Submitted", "Pending", "Verified"
    }
}
