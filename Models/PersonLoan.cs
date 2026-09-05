using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonLoan
    {

        [Key]
        public int LoanID { get; set; }

        [StringLength(100)]
        public string LoanCode { get; set; }

        public int PersonId { get; set; }

        public int FamilyId { get; set; }
        public string Source { get; set; }



        public DateTime LoanDate { get; set; }

        public DateTime? LoanClearanceDate { get; set; }

        public decimal LoanAmount { get; set; }

        public decimal TotalPayable { get; set; }

        public decimal MonthlyInstallment { get; set; }

        public int LoanDuration { get; set; }   

        public decimal InterestRate { get; set; }   

        public int Noofinstallmentpaid { get; set; }    


        public string LoanType { get; set; }
      
      
        public string PurposeofLoan { get; set; }

        public string? ReasonofDefault { get; set; }

        public bool IsDefault { get; set; }

        public bool IsActive { get; set; }


        [StringLength(100)]
        public string? ReasonForInActive { get; set; }
        [StringLength(500)]
        public string? DescriptionForInActive { get; set; }

        public bool Isgoing { get; set; }

        public bool? IsPaidByYou { get; set; }

        [StringLength(100)]
        public string? RelationWhoHelpedToPayLoan { get; set; }


        [StringLength(500)]
        public string? DescriptionWhoHelpedToPayLoan { get; set; }  // Store list of file objects

        [StringLength(5000)]
        public string? UploadProofJson { get; set; }  // Store list of file objects


        public string CreatedBy { get; set; }
        public DateTime InsertDate { get; set; } = DateTime.Now;


        public string? UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; } = DateTime.Now;
    }
}


