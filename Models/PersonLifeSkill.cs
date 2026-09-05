using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonLifeSkill
    {
        [Key]
        public int PersonSkillId { get; set; }

        [Required]
        public int PersonId { get; set; }

        [Required]
        public int SkillId { get; set; }

        [Required]
        public bool IsCertified { get; set; }

        [MaxLength(50)]
        public string? Proficiency { get; set; } // Basic, Medium, Advanced

        [MaxLength(500)]
        public string? Remarks { get; set; }

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
