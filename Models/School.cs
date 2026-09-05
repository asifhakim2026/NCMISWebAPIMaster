using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    using System.ComponentModel.DataAnnotations;


 
    public class School
    {
        [Key]
        public int SchoolId { get; set; }

        public Guid SchoolGUID { get; set; } = Guid.NewGuid();

        [Required(ErrorMessage = "School Name is required.")]
        [StringLength(100, MinimumLength = 5)]
        [Display(Name = "School Name")]  // 👈 Add this
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "School Type is required.")]
        [StringLength(100)]
        [Display(Name = "Type of School")]  // 👈 Custom Label
     
        public string SchoolType { get; set; }  // 👈 Change from string to Enum

        [StringLength(100)]
        [Display(Name = "Unit Name")]
        public string? UnitName { get; set; }

        [StringLength(1000)]
        [Display(Name = "Address")]  // 👈 Fix Spelling & Add Display Name
        public string? Address { get; set; }  

        [StringLength(500)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [StringLength(500)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        public int TotalCapacity { get; set; }


        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; }


        public int AKESPAPIID { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(500)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(500)]
        public string? UpdatedBy { get; set; }
    }

}
