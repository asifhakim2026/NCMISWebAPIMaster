namespace NCMISAPI.DTOs;

public class FeeRemissionListItemDto
{
    public int? FamilyId { get; set; }
    public int? PersonId { get; set; }
    public int FeeRemissionId { get; set; }
    public Guid FeeRemissionGUID { get; set; }
    public string? CaseNumber { get; set; }
    public string? SurveyConsent { get; set; }
    public string? PreferredSurveyTime { get; set; }
    public string? VisitorName { get; set; }
    public string? Relation { get; set; }
    public string? CaseType { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime InsertDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? StudyingClass { get; set; }
    public string? StudyingSection { get; set; }
    public DateTime? DOB { get; set; }
    public string? StudentFirstName { get; set; }
    public string? StudentLastName { get; set; }
    public string? FatherName { get; set; }
    public string? StudentCNIC { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmailAddress { get; set; }
    public string? CompleteAddress { get; set; }
    public string? SchoolEnrollmentNumber { get; set; }
    public int SchoooID { get; set; }
    public int? StepId { get; set; }
    public bool? IsCurrentStepActive { get; set; }
    public decimal? NetFeeRate { get; set; }
    public decimal? HostelFee { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? CurrentFA_Percentage { get; set; }
    public decimal? CurrentHostelFA_Percentage { get; set; }
    public string? Remarks { get; set; }
    public int JKID { get; set; }
    public string? CurrentStatus { get; set; }
    public string? FatherCNIC { get; set; }
    public string? MotherName { get; set; }
    public string? MotherCNIC { get; set; }
    public string? Gender { get; set; }
    public string? CaseApprovalStatus { get; set; }
    public string? ClientAcceptanceStatus { get; set; }
    public string? CurrentAssignTo { get; set; }
    public string? CurrentStep { get; set; }
    public bool IsManual { get; set; }
    public string? VoucherNumber { get; set; }
    public Guid? PersonGuid { get; set; }
    public Guid? FamilyGUID { get; set; }
    public Guid? ProjectGUID { get; set; }
    public int? ProjectId { get; set; }
}
