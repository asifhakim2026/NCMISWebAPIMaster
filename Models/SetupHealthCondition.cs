using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupHealthCondition
    {
        [key]
        public int SetupHealthConditionId { get; set; }

        [StringLength(300)]
        public string ConditionName { get; set; } // e.g., Diabetes, Hypertension, Disability
    }
}
