using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonSeniorCitizen
    {
        public int PersonSeniorCitizenId { get; set; }

        public int PersonId { get; set; }

        [StringLength(100)]
        public string CardNumber { get; set; }

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        [StringLength(500)]
        public string IssuerType { get; set; }  // e.g., Government, NGO, Community

        [StringLength(500)]
        public string IssuedBy { get; set; }

       
        public string Amenities { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(500)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [StringLength(500)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string? ReasonForInActive { get; set; }

        [StringLength(500)]
        public string? DescriptionForInActive { get; set; }
    }
}
