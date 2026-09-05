namespace NCMISAPI.DTOs.Person;

public class ApiResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class PersonProjectEnrollmentDto
{
    public int EnrollmentId { get; set; }
    public int ProjectId { get; set; }
    public Guid ProjectGUID { get; set; }
    public string? Module { get; set; }
    public int ReferenceID { get; set; }
    public string? Remarks { get; set; }
    public DateTime InsertDate { get; set; }
}

public class FamilyMemberDto
{
    public int PersonId { get; set; }
    public Guid? PersonGuid { get; set; }
    public string? PersonName { get; set; }
    public string? FirstName { get; set; }
    public string? Surname { get; set; }
    public string? LastName { get; set; }
    public string? CNIC { get; set; }
    public string? IdentificationType { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? RelationshipRole { get; set; }
    public string? FamilyGroupCode { get; set; }
    public DateTime? FamilyCreatedDate { get; set; }
    public string? RelatedTo { get; set; }
    public string? RelationshipName { get; set; }
    public int RelationshipTypeId { get; set; }
    public string? Image { get; set; }
    public string? CNICFront { get; set; }
    public string? CNICBackView { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MaritalStatus { get; set; }
    public bool IsDeceased { get; set; }
    public DateTime? DeceasedDate { get; set; }
    public DateTime? CNICIssueDate { get; set; }
    public DateTime? CNICExpiryDate { get; set; }
    public string? CNICExpiryStatus { get; set; }
    public string? PersonCode { get; set; }
    public string? FamilyCode { get; set; }
    public Guid? FamilyGuid { get; set; }
    public string? Address { get; set; }
    public string? Region { get; set; }
    public string? LocalCouncil { get; set; }
    public string? JK { get; set; }
    public int JKID { get; set; }
    public string? JKShortCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdateDate { get; set; }
    public string? UpdatedBy { get; set; }
    public string? ApplicationStatus { get; set; }
    public string? Headofthefamilyname { get; set; }
    public string? EducationStatus { get; set; }
    public string? YouthEducationStatus { get; set; }
    public string? EmploymentStatus { get; set; }
    public string? SubstanceAbuse { get; set; }
    public string? Disabilities { get; set; }
    public string? SubstanceAbuseStatus { get; set; }
    public string? DisabilityStatus { get; set; }
    public string? BTSCode { get; set; }
    public int? FamilyMemberCount { get; set; }
    public List<PersonProjectEnrollmentDto> PersonProjectEnrollments { get; set; } = [];
}

public class FamilyBasicSummaryDto
{
    public string? FamilyCode { get; set; }
    public int TotalMembers { get; set; }
    public int DeceasedCount { get; set; }
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int Under18Count { get; set; }
    public int AdultCount { get; set; }
    public int SeniorCitizenCount { get; set; }
    public int UnknownAgeCount { get; set; }
    public int EmployedAdults { get; set; }
    public int StudentCount { get; set; }
    public int UnemployedCount { get; set; }
    public int HouseHoldSurveyCount { get; set; }
    public int IncomeandExpenseSurveyCount { get; set; }
    public int AddtionalSupportSurveyCount { get; set; }
    public int FamilyDataVerificationCount { get; set; }
    public DateTime? FamilyDataVerificationDate { get; set; }
    public decimal TotalActiveLoanAmount { get; set; }
    public decimal TotalLoanRepayment { get; set; }
    public decimal TotalInvestment { get; set; }
    public decimal TotalOtherIncome { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal TotalROIOnInvestment { get; set; }
    public decimal OngoingEducationExpense { get; set; }
    public decimal AddtionalSupportIncome { get; set; }
}

public class PaginatedResultDto<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
}

public class SurveyOptionDto
{
    public int OptionId { get; set; }
    public string? Name { get; set; }
    public string? ShortCode { get; set; }
}

public class SurveyQuestionDto
{
    public int ParentId { get; set; }
    public string? QuestionText { get; set; }
    public string? QuestionType { get; set; }
    public string? ShortCode { get; set; }
    public List<SurveyOptionDto> Options { get; set; } = [];
}

public class SurveyResponseItemDto
{
    public int ParentId { get; set; }
    public int OptionId { get; set; }
    public bool IsChecked { get; set; }
    public string? AnswerText { get; set; }
    public string? Name { get; set; }
}

public class SurveyRequestDto
{
    public List<SurveyResponseItemDto> Responses { get; set; } = [];
}

public class SurveyAnalysisResponseItemDto
{
    public int OptionId { get; set; }
    public bool IsChecked { get; set; }
    public string? AnswerText { get; set; }
}

public class SurveyAnalysisEntryDto
{
    public string? CreatedBy { get; set; }
    public DateTime SurveyDate { get; set; }
    public List<SurveyAnalysisResponseItemDto> Responses { get; set; } = [];
}

public class HouseholdSurveyAnalysisDto
{
    public List<SurveyQuestionDto> Questions { get; set; } = [];
    public List<SurveyAnalysisEntryDto> Surveys { get; set; } = [];
}

public class DeceasedViewDto
{
    public int PersonDeceasedId { get; set; }
    public int PersonId { get; set; }
    public string? PersonFullName { get; set; }
    public string? CNIC { get; set; }
    public DateTime DateOfDeath { get; set; }
    public string? TimeOfDeath { get; set; }
    public string? PlaceOfDeath { get; set; }
    public string? ReportedByName { get; set; }
    public string? ReportedByRelation { get; set; }
    public string? DeceasedShortCode { get; set; }
    public string? DeathCertificateFilePath { get; set; }
    public string? AdditionalRemarks { get; set; }
}

public class FundingDto
{
    public string Source { get; set; } = string.Empty;
    public string Frequency { get; set; } = string.Empty;
    public int Amount { get; set; }
}

public class WorkIncomeComponentDto
{
    public string componentType { get; set; } = string.Empty;
    public decimal amount { get; set; }
    public string? frequency { get; set; }
    public string? notes { get; set; }
}
