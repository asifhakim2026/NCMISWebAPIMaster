using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class PersonFamily
    {
        [Key]
        public int PersonFamilyId { get; set; }

        [Required]
        public int PersonId { get; set; }           // Main person (e.g., child)

        [Required]
        public int RelatedPersonId { get; set; }    // Related person (e.g., father or mother)

        [Required]
        public int RelationshipTypeId { get; set; } // From RelationshipType

        public DateTime CreatedDate { get; set; }

        [MaxLength(500)]
        public string CreatedBy { get; set; }


        public DateTime? UpdatedDate { get; set; }

        [MaxLength(500)]
        public string? UpdatedBy { get; set; }


    }


}
