using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class ProjectType
    {
        [Key]
        public int ProjectTypeID { get; set; }

        [Required]
        [StringLength(50)]
        public string ProjectTypeName { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [StringLength(10)]
        public string ColorCode { get; set; } = "#17a2b8"; // Default color

        [StringLength(50)]
        public string IconClass { get; set; } = "bi-tag"; // ✅ Default Bootstrap Icon
    }



}
