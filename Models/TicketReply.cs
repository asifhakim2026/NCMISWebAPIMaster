using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class TicketReply
    {
        [Key]
        public int ReplyId { get; set; }
        public int TicketId { get; set; }
        public int RepliedBy { get; set; }
        public string Role { get; set; } // User or Admin
        public string ReplyText { get; set; }
        public string? AttachmentPath { get; set; }
        public DateTime ReplyDate { get; set; } = DateTime.Now;

        
    }
}
