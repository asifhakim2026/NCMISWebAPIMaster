using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonHouseHoldResponse
    {
        [Key]
        public int SurveyResponseId { get; set; } // ✅ Primary Key

        public int PersonSurveyMasterId { get; set; } // ✅ Foreign Key to PersonSurveyMaster Table

        public int ParentId { get; set; } // ✅ Question Category (GeneralSetup ParentId)

        public int OptionId { get; set; } // ✅ Answer ID (GeneralSetup Id)

    
        public int? PersonId { get; set; } // ✅ Foreign Key to Person Table

        public int? FamilyId { get; set; }

        public bool IsChecked { get; set; } // ✅ Checkbox Response

        public string? AnswerText { get; set; } // ✅ Optional Text Answer


        public string? Name { get; set; } // ✅ Optional Text Answer

        public DateTime InsertDate { get; set; } = DateTime.UtcNow; // ✅ Timestamp

        public string? CreatedBy { get; set; } // ✅ User who submitted
    }



}
