using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class LifeSkillsMaster
    {
        [Key]
        public int SkillId { get; set; }

        [Required]
        [MaxLength(255)]
        public string SkillName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; }
    }
}
