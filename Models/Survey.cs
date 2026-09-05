using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class Survey
    {
        [Key]
        public int SurveyId { get; set; }  // Unique Survey ID

        [Required]
        public int PersonId { get; set; }  // Linked to Person Table

        [Required]
        public Guid SurveyGuid { get; set; } = Guid.NewGuid(); // Unique Identifier for API Calls

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Timestamp

        public DateTime? UpdatedDate { get; set; }
    }
}
