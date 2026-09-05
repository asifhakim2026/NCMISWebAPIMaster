namespace NCMISAPI.DTOs;

/// <summary>
/// Active GeneralSetups option under a SupportSurvey parent category
/// (matches MVC AddtionalSupportsurveyload Options).
/// </summary>
public class AdditionalSupportCategoryOptionDto
{
    public int OptionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ShortCode { get; set; }
}

/// <summary>
/// SupportSurvey parent category from GeneralSetups (ParentId == 0, Type == SupportSurvey).
/// </summary>
public class AdditionalSupportCategoryDto
{
    /// <summary>GeneralSetups.Id of the parent category (used as ParentId when saving responses).</summary>
    public int ParentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ShortCode { get; set; }

    public string? QuestionType { get; set; }

    public string? Description { get; set; }

    public List<AdditionalSupportCategoryOptionDto> Options { get; set; } = [];
}

/// <summary>
/// Fill-list payload for Additional Support (SupportSurvey) categories.
/// </summary>
public class AdditionalSupportFillListDto
{
    /// <summary>
    /// Active SupportSurvey parent categories with active child options,
    /// ordered by parent Id then option Name.
    /// </summary>
    public List<AdditionalSupportCategoryDto> Categories { get; set; } = [];
}
