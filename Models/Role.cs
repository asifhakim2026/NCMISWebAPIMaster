using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(100)]
        public string RoleName { get; set; } // Example: "Admin", "Editor", "Viewer"
    }
}
