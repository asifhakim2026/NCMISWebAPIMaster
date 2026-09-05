using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class UserDepartmentMapping
    {
        [Key]
        public int UserDepartmentID { get; set; }

        [Required]
      
        public int DepartmentID { get; set; }

      

        [Required]
      
        public int UserID { get; set; }

       
        public bool IsActive { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
