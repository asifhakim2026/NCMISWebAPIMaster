using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class RelationshipType
    {
        [Key]
        public int RelationshipTypeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }  // e.g., Father, Mother, Spouse

        public int SortOrder { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        public string CreatedBy { get; set; }
    }
}
