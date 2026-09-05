using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class PersonAttachment
    {
        [Key]
        public int PersonAttachmentId { get; set; }

        public int? Fileuploadid { get; set; }

        public int FamilyId { get; set; }
        public int PersonId { get; set; }


        [StringLength(500)]
        public string AttachmentName { get; set; }

        public string AttachmentURL { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } 

        [StringLength(300)]
        public string Status { get; set; }

        [StringLength(300)]
        public string CreatedBy { get; set; } // Store actual username instead of UserId

        public bool Isactive { get; set; }


       
    }
}
