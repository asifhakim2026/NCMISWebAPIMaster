using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class FeesRemission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FeeRemissionId { get; set; } // Auto-increment ID

        [StringLength(500, ErrorMessage = "School Enrollment Number cannot exceed 500 characters.")]

        [Required(ErrorMessage = "School Enrollment is required.")]
        public string SchoolEnrollmentNumber { get; set; }

        [Required(ErrorMessage = "School is required.")]
        public int SchoooID { get; set; }

        //[Required(ErrorMessage = "Region is required.")]
        //public int RegionID { get; set; }

        [StringLength(500)]
        public string CaseNumber { get; set; } // ✅ Unique Case Number per School

        [StringLength(500)]
        public string? VoucherNumber { get; set; }

        [Required(ErrorMessage = "Fee Remission is required.")]
        public Guid FeeRemissionGUID { get; set; }

        [Required(ErrorMessage = "Gender is required.")]
        [StringLength(50, ErrorMessage = "Gender cannot exceed 50 characters.")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Date of Birth is required.")]
        public DateTime? DateOfBirth { get; set; }



        [Required(ErrorMessage = "Student First Name is required.")]
        [StringLength(500, ErrorMessage = "Student First Name cannot exceed 500 characters.")]
        public string StudentFirstName { get; set; }

        [Required(ErrorMessage = "Student Last Name is required.")]
        [StringLength(500, ErrorMessage = "Student Last Name cannot exceed 500 characters.")]
        public string StudentLastName { get; set; }

        [StringLength(100, ErrorMessage = "Student CNIC should not exceed 100 characters.")]
        [Required(ErrorMessage = "Student Bform/CNIC is required.")]
        public string StudentCNIC { get; set; }

        [Required(ErrorMessage = "Father's Name is required.")]
        [StringLength(1000, ErrorMessage = "Father's Name cannot exceed 1000 characters.")]
        public string FatherName { get; set; }

        //[Required(ErrorMessage = "Mother's Name is required.")]
        [StringLength(1000, ErrorMessage = "Mother's Name cannot exceed 1000 characters.")]
        public string? MotherName { get; set; }

        [Required(ErrorMessage = "Studying Class is required.")]
        [StringLength(500, ErrorMessage = "Studying Class cannot exceed 500 characters.")]
        public string StudyingClass { get; set; }

        [Required(ErrorMessage = "Section is required.")]
        [StringLength(50, ErrorMessage = "Section cannot exceed 50 characters.")]
        public string Section { get; set; }


        [Required(ErrorMessage = "Case Type is required.")]
        [StringLength(500, ErrorMessage = "Case Type cannot exceed 500 characters.")]
        public string CaseType { get; set; }

        [Required(ErrorMessage = "Father's CNIC is required.")]
        [StringLength(100, ErrorMessage = "Father's CNIC should not exceed 100 characters.")]
        public string FatherCNIC { get; set; }

        [StringLength(100, ErrorMessage = "Mother's CNIC should not exceed 100 characters.")]
        public string? MotherCNIC { get; set; }

        [Required(ErrorMessage = "Complete Address is required.")]
        [StringLength(3000, ErrorMessage = "Complete Address cannot exceed 3000 characters.")]
        public string CompleteAddress { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        [Required(ErrorMessage = "Phone Number is required.")]
        [StringLength(50, ErrorMessage = "Phone Number cannot exceed 11 characters.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone Number must be exactly 11 digits.")]
        public string PhoneNumber { get; set; }

        [StringLength(50, ErrorMessage = "Secondary Phone Number cannot exceed 11 characters.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Secondary Phone Number must be exactly 11 digits.")]
        public string? SecondaryPhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(1000, ErrorMessage = "Email Address cannot exceed 1000 characters.")]
        public string? EmailAddress { get; set; }

        [Required(ErrorMessage = "Net Fee Rate is required.")]
        public decimal? NetFeeRate { get; set; }

        [Required(ErrorMessage = "Current FA Percentage is required.")]
        public decimal? CurrentFA_Percentage { get; set; } = 0;

        public decimal? HostelFee { get; set; } = 0;

        public decimal? CurrentHostelFA_Percentage { get; set; } = 0;

        public decimal? CurrentBalance { get; set; } = 0;

        [StringLength(200, ErrorMessage = "Remarks cannot exceed 200 characters.")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "Insert Date is required.")]
        public DateTime InsertDate { get; set; }

        [Required(ErrorMessage = "Created By is required.")]
        [StringLength(1000, ErrorMessage = "Created By cannot exceed 1000 characters.")]
        public string CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(1000, ErrorMessage = "Updated By cannot exceed 1000 characters.")]
        public string? UpdatedBy { get; set; }

        public int? PersonId { get; set; }

       


        public decimal? ApproveFees { get; set; }

        public decimal? ApproveHostelFees { get; set; }


       public string? FeeStructureDetailsJson { get; set; }



        [StringLength(50)]
        public string? CaseApprovalStatus { get; set; } //Pending Approved Reject

        public DateTime? CaseApprovalDate { get; set; }

        [StringLength(50)]
        public string? ClientAcceptanceStatus { get; set; } //Pending Approved Refused

        public DateTime? ClientAcceptanceDate { get; set; }


        [StringLength(50)]
        public string? ClientAcceptanceNotes { get; set; } 


        [StringLength(1000)]
        public string? UploadVoucher { get; set; }


        [StringLength(50)]
        public string SurveyConsent { get; set; }


        [StringLength(1000)]
        public string? PreferredSurveyTime { get; set; }

        public string VisitorName { get;set; }
        public string Relation { get; set; }

        public int JKID { get; set; }

        public DateTime? AdmissionDate { get; set; }



        #region Stage Log

       

        [Required(ErrorMessage = "Project ID is required.")]
        public int ProjectId { get; set; }

        public int? StepID { get; set; }

        public bool? IsCurrentStepActive { get; set; }

        [StringLength(200, ErrorMessage = "Current Status cannot exceed 200 characters.")]
        public string? CurrentStatus { get; set; }

        [StringLength(200, ErrorMessage = "Current Step cannot exceed 200 characters.")]
        public string? CurrentStep { get; set; }

        public DateTime? CurrentStatusDate { get; set; }

        [StringLength(500, ErrorMessage = "Current Status Updated By cannot exceed 200 characters.")]
        public string? CurrentStatusUpdatedBy { get; set; }


        [StringLength(500, ErrorMessage = "Current Status Updated By cannot exceed 200 characters.")]
        public string? CurrentAssignTo { get; set; }

        public string? StageJsonData { get; set; } // For storing JSON data related to the fee remission 


        #endregion

        public string? FeeStructureApprovalDetailsJson { get; set; }

        public bool IsManual { get; set; }



        public bool IsInstallment { get; set; }

        public int? NoOfInstallment { get; set; }


        public int? ApprovedInstallmentCount { get; set; }
    }


}
