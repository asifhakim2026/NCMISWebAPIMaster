namespace NCMISAPI.DTOs;

/// <summary>
/// Active GeneralSetups option under a HouseHold survey parent question
/// (matches MVC SurveyOptionModel / PersonController.HouseholdSurvey).
/// </summary>
public class HouseHoldSurveyCategoryOptionDto
{
    public int OptionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ShortCode { get; set; }
}

/// <summary>
/// HouseHold survey parent question from GeneralSetups (ParentId == 0, Type == HouseHold).
/// Shape aligns with MVC SurveyQuestionModel used by _householdAdd.cshtml
/// and with PersonSurveyService.HouseholdSurvey.
/// </summary>
public class HouseHoldSurveyCategoryDto
{
    /// <summary>GeneralSetups.Id of the parent question (ParentId when saving responses).</summary>
    public int ParentId { get; set; }

    /// <summary>Question text (GeneralSetups.Name of the parent).</summary>
    public string QuestionText { get; set; } = string.Empty;

    public string? ShortCode { get; set; }

    public string? QuestionType { get; set; }

    public string? Description { get; set; }

    public List<HouseHoldSurveyCategoryOptionDto> Options { get; set; } = [];
}

/// <summary>
/// Fill-list payload for Housing and Assets (HouseHold) survey questions.
/// Distinct from HouseHoldSupportFillListDto (SetupHouseHoldCategories Food/Health/…).
/// </summary>
public class HouseHoldSurveyFillListDto
{
    /// <summary>Always "GeneralSetups" — distinguishes from SetupHouseHoldCategories.</summary>
    public string Source { get; set; } = "GeneralSetups";

    /// <summary>Always "HouseHold" — GeneralSetups.Type filter used.</summary>
    public string Type { get; set; } = "HouseHold";

    /// <summary>
    /// HouseHold parent questions with active child options
    /// (GeneralSetups Type == HouseHold, ParentId == 0), ordered by parent Id.
    /// Named Questions (not Categories) to avoid confusion with HouseHold Support.
    /// </summary>
    public List<HouseHoldSurveyCategoryDto> Questions { get; set; } = [];
}
