using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class EstateOfficeJKExpense
    {
        [Key]
        public int EstateOfficeJKExpenseId { get; set; }

        public Guid EstateOfficeJKExpenseGuide { get; set; } = Guid.NewGuid();


        [Required, MaxLength(300)]
        public string ReceiptNumber { get; set; } // e.g. "102-20250715-0001"

        public int EstateOfficeExpenseHeadId { get; set; }

        public int JKId { get; set; }

        public string? CrossReference { get; set; }


        public string? Unit { get; set; }

        public decimal TotalAmount { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [StringLength(500)]
        public string AmountJson { get; set; }

        public bool IsActive { get; set; }

        public bool IsCancelled { get; set; }

        [MaxLength(100)]
        public string? CancelledReason { get; set; }

        [MaxLength(500)]
        public string? CancelledDetails { get; set; }

        [MaxLength(300)]
        public string? CancellationReqeustedBy { get; set; }

        public DateTime? CancelledDate { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(300)]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [MaxLength(300)]
        public string? UpdatedBy { get; set; }

        public bool IsClosed { get; set; } = false;

        public int? DepositId { get; set; } // 🔁 foreign key to parent

       


        [MaxLength(1000)]
        public string? Remarks { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
