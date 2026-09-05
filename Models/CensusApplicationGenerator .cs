using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class CensusApplicationGenerator
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public Guid ApplicationGuid { get; set; } = Guid.NewGuid();

        [Required, StringLength(60)]
        public string FirstName { get; set; }

        [Required, StringLength(60)]
        public string FatherName { get; set; }

        [StringLength(60)]
        public string? SurName { get; set; }

        [Required, StringLength(50)]
        public string CNIC { get; set; }

        [Required, StringLength(20)]
        public string MobileNumber { get; set; }

        [Required]
        public int NumberOfFamilyMembers { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        [Required]
        public bool IsSelfRegistered { get; set; } // true = link sent to person, false = volunteer-assisted

        [StringLength(100)]
        public string? AssignedToVolunteer { get; set; } // Optional

        public DateTime? AppointmentDateTime { get; set; }

        [StringLength(50)]
        public string? BoothNumber { get; set; }

        [StringLength(3000)]
        public string? GeneratedURL { get; set; } // Link sent to the user

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        [StringLength(300)]
        public string CreatedBy { get; set; } 


        public bool IsSubmitted { get; set; } = false;

        public DateTime? SubmissionDate { get; set; }

        public int? JKID { get; set; } // Optional link to a Jamatkhana

        public int? CensusCampaignID { get; set; } // FK to CensusCampaign

        public bool IsWhatsAppLinkSent { get; set; } = false;

        public DateTime? WhatsAppLinkSentDateTime { get; set; }

        public bool IsCancelled { get; set; } = false;  
    }
}
