using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonSurveyMaster
    {
        [Key]
        public int PersonSurveyMasterId { get; set; } // ✅ Primary Key


        public int? FamilyId { get; set; }
        public int? PersonId { get; set; } // ✅ Foreign Key to Person Table

        public string SurveyType { get; set; } // ✅ New: "Household" or "IncomeExpense"

        public Guid SurveyGuid { get; set; } = Guid.NewGuid(); // ✅ Unique Identifier for this survey

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ✅ Auto timestamp

        public string? CreatedBy { get; set; } // ✅ Track who created the survey

        public bool IsActive { get; set; }
    }

}
