using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupCauseOfDeathType
    {

        [Key]
        public int SetupCauseOfDeathId { get; set; }

        [StringLength(300)]
        public string Name { get; set; } // e.g., Natural, Accident, Health, Unknown
    }
}
