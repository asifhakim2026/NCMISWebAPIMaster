using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class BeneficiaryMaster
    {
        [Key]
        public int BeneficiaryID { get; set; }

        [Required, StringLength(100)]
        public string FirstName { get; set; }

        [Required, StringLength(100)]
        public string LastName { get; set; }

        [StringLength(20)]
        public string? GovernmentID { get; set; } // Optional

        [NotMapped]
        public int? DayOfBirth { get; set; } // Day Dropdown

        [NotMapped]
        public int? MonthOfBirth { get; set; } // Month Dropdown

        [NotMapped]
        public int? YearOfBirth { get; set; } // Year Dropdown

        [Required]
        public DateTime DateOfBirth { get; set; } // Actual Date of Birth (Stored)

        [Required]
        public int GenderID { get; set; } // Links to Criteria (Gender)
    
    
        [Required]
        public int MaritalStatusID { get; set; } // Links to Criteria (Marital Status)
     
      
        [Required]
        public int RegisteredFromLocationID { get; set; } // Registered Location


        [NotMapped]
        public virtual Location RegisteredFromLocation { get; set; }

        [StringLength(15)]
        public string? PhoneNumber { get; set; } // Optional

        [StringLength(255)]
        public string? Email { get; set; } // Optional

        public bool IsActive { get; set; } = true;

        [Column("Datetime")]
        public DateTime InsertDate { get; set; }

        [Column("CreatedBy")]
        [StringLength(100)]
        public string CreatedBy { get; set; }

        [Column("UpdateDate")]
        public DateTime? UpdateDate { get; set; }

        [Column("UpdatedBy")]
        [StringLength(100)]
        public string? UpdatedBy { get; set; }

        [Column("Remarks")]
        [StringLength(1000)]
        public string? Remarks { get; set; }


    }


}
