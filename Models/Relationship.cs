using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class Relationship
    {
        [Key]
        public int Id { get; set; }  // Relationship ID (e.g., Father, Son, etc.)

        public string Name { get; set; } // Relationship Name (Father, Son, etc.)

        public int ReverseRelationshipId { get; set; } // ✅ Reverse Relationship ID (e.g., Father -> Son, Son -> Father)
    }

}
