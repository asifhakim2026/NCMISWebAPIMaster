using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class Ticket
    {

        [Key]
        public int TicketId { get; set; }

        public string TicketNumber { get; set; }
        public int UserId { get; set; }
        public string Category { get; set; } // Bug, Support, Training, etc.
        public string Title { get; set; }
        public string Description { get; set; } // HTML from Summernote
        public string? AttachmentPath { get; set; }
        public string Status { get; set; } // Pending, In Review, Resolved, Closed
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastUpdated { get; set; } = DateTime.Now;


        [NotMapped]

        public string CreatedBy { get; set; }

       
    }

}
