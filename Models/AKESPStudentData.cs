using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class AKESPStudentData
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        public Guid StudentIdGuid { get; set; } = Guid.NewGuid(); // Default new GUID

       
    
        public int SchoolId { get; set; }


     

        public int StudentEnrollmentNumber { get; set; }

        public string StudentFullName { get; set; }


        public int? BFormCNIC { get; set; }


        public string? FatherName { get; set; }

        public int? FatherCNIC { get; set; }

        public string? MotherName { get; set; }

        public int? MotherCNIC { get; set; }
        public int? ClassId { get; set; }

        public string? Section { get; set; } //A B C D

        public string? Shift { get; set;  }//Morning Afternoon Evening

        public string? Gender { get; set; }
   

        public int? AcademicGroupId { get; set; }

        public decimal TuitionFees { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow; // Default value

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true; // Default active

        public string? Remarks { get; set; }
    }

}
