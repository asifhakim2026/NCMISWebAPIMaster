using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class Menu
    {
        [Key]
        public int MenuId { get; set; }

        public int? ParentId { get; set; } // NULL for main menu, value for submenu

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // Example: "Dashboard", "Setup", "Users"

        [StringLength(500)]
        public string? Url { get; set; } // NULL for expandable menus

        [StringLength(100)]
        public string? IconClass { get; set; } // Example: "dashboard", "users"

        public int SortOrder { get; set; }

        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; }

        public int IsVisible { get; set; }
        public bool IsActive { get; set; } = true; // Enable/disable menu
    }
}
