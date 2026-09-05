using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class EstateOfficeExpenseDeposit
    {
        [Key]
        public int DepositId { get; set; }


        public Guid DepositGuid { get; set; } = Guid.NewGuid();

        [Required, MaxLength(300)]
        public string DepositNumber { get; set; } // e.g. "102-20250715-0001"

        public int JKId { get; set; }

        public DateTime Month { get; set; } // set to first day of the month for tracking

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(300)]
        public string CreatedBy { get; set; }

        public bool IsApproved { get; set; }

        [MaxLength(300)]
        public string? ApproverName { get; set; }

        [MaxLength(300)]
        public string? ApproverDesignation { get; set; }

       
        public string? Esign { get; set; }

        public DateTime? ApprovalDate { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }


        public decimal TotalAmount { get; set; }

        public int TotalTransactions { get; set; }
    }
}
