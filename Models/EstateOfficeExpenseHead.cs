using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class EstateOfficeExpenseHead
    {

        [Key]
        public int EstateOfficeExpenseHeadId { get; set; }

       
        [StringLength(500)]
        public string ExpenseHeadName { get; set; }

        [StringLength(300)]
        public string? Description { get; set; }

        public bool IsRequiredTextBoxToType { get; set; }

        public decimal? AcceptThresholdAmount { get; set; }

        public bool IsActive { get; set; } 
    }
}
