using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SurveyorFamilyNote
    {
        [Key]
        public int SurveyorFamilyNotesID { get; set; }

        [StringLength(100)]
        public string SurveyCode { get; set; }

        public int? PersonId { get; set; }

        public int? FamilyId { get; set; }

        public string Notes { get; set; }

        public string? Address { get; set; }

        public string? ImagePath { get; set; } // Path of captured image
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string CreatedBy { get; set; }
        public DateTime InsertDate { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false; // Optional: Soft delete if needed

        public string? UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; } = DateTime.Now;
    }
}
