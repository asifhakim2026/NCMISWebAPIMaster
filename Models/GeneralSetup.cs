using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{


    public class GeneralSetup
    {
        [Key]
        public int Id { get; set; }

        public int ParentId { get; set; } // Parent ID for categorization (0 = Root)

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, MinimumLength = 2)]
        [Column(TypeName = "varchar(200)")]
        public string Name { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(200)")]
        public string? ShortCode { get; set; }

        [StringLength(20)]
        [Column(TypeName = "varchar(200)")] // ✅ Store the Parent's ShortCode
        public string? ParentShortCode { get; set; }

        [StringLength(255)]
        [Column(TypeName = "varchar(255)")]
        public string? Description { get; set; }

        [Column(TypeName = "varchar(50)")]
        public string? QuestionType { get; set; } // ✅ NEW: Supports "Textbox", "Dropdown", "Checkbox", "Radio"


        [Column(TypeName = "varchar(50)")]
        public string? Type { get; set; } // ✅ NEW: "HouseType", "Support", "Survey", "Education"


        public bool IsActive { get; set; } = true;

        [Column(TypeName = "datetime")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        [Column(TypeName = "varchar(300)")]
        public string CreatedBy { get; set; }

        [Column(TypeName = "datetime")]
        public DateTime? UpdatedDate { get; set; }

        [StringLength(50)]
        [Column(TypeName = "varchar(300)")]
        public string? UpdatedBy { get; set; }
    }




}
