using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonEducationFundingSource
    {
        [Key]
        public int FundingId { get; set; }

        [Required]
        public int EducationId { get; set; }  // FK to PersonEducationDetail

        [Required]
        [StringLength(500)]
        public string ExpenseType { get; set; }  // e.g., Tuition, Hostel, Stationery

        [Required]
        [StringLength(1000)]
        public string FundingSourceName { get; set; }  // e.g., Self, Family, NGO

        [StringLength(100)]
        public string FundingFrequency { get; set; }
        public int? MonthlyAmount { get; set; }
        public int? YearlyAmount { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(1000)]
        public string CreatedBy { get; set; }
    }
}
