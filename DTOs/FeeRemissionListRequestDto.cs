namespace NCMISAPI.DTOs;

/// <summary>
/// Query filters for fee remission list.
/// </summary>
public class FeeRemissionListRequestDto
{
    public int? StepId { get; set; }
    public int? SchoolId { get; set; }
    public string? StudentEnrollmentNumber { get; set; }
    /// <summary>
    /// Case current status filter. When null/empty, defaults to "pending".
    /// </summary>
    public string? Status { get; set; }
    public string? CaseStatus { get; set; }
    public string? KeywordFilter { get; set; }
    public int Page { get; set; } = 1;
}
