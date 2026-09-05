using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SoftwareSetup
    {
        [Key]
        public int SoftwareSetupId { get; set; }

        [Required]
        [StringLength(255)]
        public string SetupName { get; set; }

        public string SetupJson { get; set; }  // This will store JSON settings


    }
}
