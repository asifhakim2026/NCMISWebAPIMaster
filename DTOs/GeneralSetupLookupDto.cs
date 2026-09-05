namespace NCMISAPI.DTOs;

/// <summary>
/// Active GeneralSetup child option used for Income / Expense lookups.
/// </summary>
public class GeneralSetupLookupDto
{
    public int Id { get; set; }

    public int ParentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ShortCode { get; set; }

    public string? Type { get; set; }
}
