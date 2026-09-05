namespace NCMISAPI.DTOs;

/// <summary>
/// Query filters for fee remission list.
/// </summary>
public class FeeRemissionListRequestDto
{
    public int? StepId { get; set; }
    public int? SchoolId { get; set; }
    public string? StudentEnrollmentNumber { get; set; }
    public string? Status { get; set; }
    public string? CaseStatus { get; set; }
    public string? KeywordFilter { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IgnoreFilters { get; set; }
    public bool IsActionFilter { get; set; }
    public bool UseStageJsonStatus { get; set; }
    public int Page { get; set; } = 1;
}
