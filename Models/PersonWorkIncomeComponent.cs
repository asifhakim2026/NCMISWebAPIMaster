using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonWorkIncomeComponent
    {
        [Key]
        public int ComponentId { get; set; }

        public int WorkExperienceId { get; set; }

        public string ComponentType { get; set; }  // e.g., Basic Salary, Bonus, Commission, Allowance, Medical, etc.
        public decimal Amount { get; set; }

        public string? Frequency { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
