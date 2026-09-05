using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupSurvey
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "varchar(50)")]
        public string Type { get; set; } // e.g., "household", "survey"

        [Required]
        [Column(TypeName = "varchar(500)")]
        public string Question { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string OptionsJson { get; set; } // JSON serialized list of options

        public bool IsActive { get; set; } = true;
    }
}
