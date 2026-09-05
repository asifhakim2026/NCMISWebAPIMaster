namespace NCMISAPI.DTOs;

/// <summary>
/// Life skills master lookup row.
/// </summary>
public class LifeSkillDto
{
    public int SkillId { get; set; }

    public string SkillName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}
