using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonInvestment
    {
        public int PersonInvestmentId { get; set; }
        public int PersonId { get; set; }

        public int? FamilyId { get; set; }

        public string InvestmentCode { get; set; } // Auto-generated code like "INV-0001"

        public string InvestmentType { get; set; } // e.g., shares, e-commerce, etc.
        public decimal AmountInvested { get; set; }

        public DateTime InvestmentDate { get; set; } // When investment started
        public bool IsFixedTerm { get; set; } // Is it locked for a period?
        public int? FixedDurationMonths { get; set; } // If yes, how long is it fixed

        public bool IsReturnInPercentage { get; set; } // How ROI is recorded
        public decimal? ExpectedReturnValue { get; set; } // If fixed value
        public decimal? ExpectedReturnPercentage { get; set; } // If percentage
        public string ReturnFrequency { get; set; } // e.g., Daily, Monthly, Yearly

        public decimal MonthlyReturn { get; set; }
        public string? Remarks { get; set; } // Optional notes

        // Audit + Inactivation fields
        public bool IsActive { get; set; } = true;
        public string? ReasonForInActive { get; set; }
        public string? DescriptionForInActive { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
