using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace NCMIS.Models
{
    public class PersonDeceasedInfo
    {
        [Key]
        public int PersonDeceasedId { get; set; }

        public string DeceasedShortCode { get; set; }
        public int PersonId { get; set; }
     
        public DateTime DateOfDeath { get; set; }
        public string TimeOfDeath { get; set; }
        public string PlaceOfDeath { get; set; }

        public string ReportedByName { get; set; }

        public string ReportedByRelation { get; set; }
      

      
        public string DeathPrayerCenter { get; set; }
       

    
        public int? GraveyardId { get; set; }
     

    
        public int? SetupCauseOfDeathId { get; set; }


        [StringLength(1000)]
        public string? DeathCertificateFilePath { get; set; }

        [StringLength(2000)]
        public string? AdditionalRemarks { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
