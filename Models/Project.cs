using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class Project
    {
        [Key]
        public int ProjectID { get; set; }


        public Guid ProjectGUID { get; set; } 


        [Required(ErrorMessage = "Please select a department.")]
        public int DepartmentID { get; set; }


     
        [Required]
        [StringLength(255)]
        public string ProjectName { get; set; }


        [Required]
        [StringLength(50)]
        public string ProjectShortCode { get; set; }

        public int? ParentID { get; set; }
     

     

      

        [StringLength(10)]
        public string ColorCode { get; set; } = "#17a2b8"; // ✅ Default color (Bootstrap info)

        [StringLength(50)]
        public string IconClass { get; set; } = "bi-star"; // ✅ Default Bootstrap Icon



       

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("Datetime")]
        public DateTime InsertDate { get; set; }

        [Column("CreatedBy")]
        [StringLength(100)]
        public string CreatedBy { get; set; }

        [Column("Remarks")]
        [StringLength(1000)]
        public string? Remarks { get; set; }

        // ✅ Add Navigation Property

       
    }


}
