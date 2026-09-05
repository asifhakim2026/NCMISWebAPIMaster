using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{

    [Index(nameof(CNIC), IsUnique = true)]
    public class PersonalInfo
    {
        [Key]
        public int PersonId { get; set; } // ✅ Changed from Id to PersonId

        [Required]
        public Guid PersonalGuid { get; set; }

        [StringLength(500)]
        public string? BTSReferenceCode { get; set; }


        [StringLength(500)]
        public string PersonCode { get; set; }


        [Required(ErrorMessage = "First Name is required.")]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required.")]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; }

        [StringLength(50)]
        public string? Surname { get; set; }

        [Required(ErrorMessage = "CNIC is required.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "CNIC must be exactly 13 digits.")]
        public string CNIC { get; set; }

        public DateTime? CNICIssueDate { get; set; }
        public DateTime? CNICExpiryDate { get; set; }


        [StringLength(50)]
        public string? IdentificationType { get; set; }


        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(15)]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        public string Gender { get; set; }


        [StringLength(1000)]
        public string? ImagePath { get; set; }



        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }

        public DateTime? UpdateDate { get; set; }
        public string? UpdatedBy { get; set; }

       public int? RegionId { get; set; }

        // ✅ Virtual List for Addresses

        [NotMapped]
        public virtual List<PersonAddress> Addresses { get; set; } = new List<PersonAddress>();

        [NotMapped]
        public virtual List<PersonEducationDetail> Educations { get; set; } = new List<PersonEducationDetail>();

        [NotMapped]
        public virtual List<PersonFamily> FamilyRelations { get; set; } = new List<PersonFamily>();

        // ✅ Number of Completed Sections
        public int CompletedTabs { get; set; }  // Starts from 0

        // ✅ Computed Property for Profile Completion %
        public double ProfileCompletionPercentage => Math.Round(((double)CompletedTabs / TotalSections) * 100, 2);

        // ✅ Define Total Sections (Adjust as Needed)
        private const int TotalSections = 4; // Example: 10 sections tota

        // ✅ Computed Property for Full Name
        public string FullName => $"{FirstName} {LastName}".Trim();


        public int? FamilyId { get; set; }        // 👉 Current Family
        public int? BirthFamilyId { get; set; }   // 👉 Original Family (where born)
        public int JKID { get; set; }


        public bool IsDeceased { get; set; }

        public DateTime? DeceasedDate { get; set; }


        [StringLength(50)]
        public string? MaritalStatus { get; set; }

        [StringLength(1000)]
        public string? CNICFront { get; set; }

        [StringLength(1000)]
        public string? CNICBack { get; set; }


        [StringLength(200)]
        public string? EducationStatus { get; set; }
       
        [StringLength(200)]
        public string? YouthEducationStatus { get; set; }


        [StringLength(200)]
        public string? EmploymentStatus { get; set; }

        [StringLength(100)]
        public string? DisabilityStatus { get; set; }

        // Disability types (stored as comma-separated: "Blind,Deaf,Mute")
        [StringLength(2000)]
        public string? Disabilities { get; set; }

        [StringLength(100)]
        public string? SubstanceAbuseStatus { get; set; }

        // Substance abuse types (stored as comma-separated: "Cigarette,Gutka")
        [StringLength(2000)]
        public string? SubstanceAbuse { get; set; }

    }
}
