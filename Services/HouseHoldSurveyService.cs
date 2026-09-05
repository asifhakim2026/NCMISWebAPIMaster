using Microsoft.EntityFrameworkCore;
using NCMISAPI.Data;
using NCMISAPI.DTOs;
using NCMISAPI.DTOs.Person;
using NCMISAPI.Helpers;

namespace NCMISAPI.Services;

/// <summary>
/// Household survey question bank from GeneralSetups Type == HouseHold.
/// </summary>
public class HouseHoldSurveyService : PersonServiceBase, IHouseHoldSurveyService
{
    private const string HouseHoldSurveyType = "HouseHold";

    public HouseHoldSurveyService(
        NcmisDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<HouseHoldSurveyService> logger,
        ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {
    }

    public async Task<PersonServiceResult> GetSurveyCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var parentCategoryIds = await _dbContext.GeneralSetups
            .AsNoTracking()
            .Where(u => u.ParentId == 0 && u.Type == HouseHoldSurveyType)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var generalSetupItems = await _dbContext.GeneralSetups
            .AsNoTracking()
            .Where(x => parentCategoryIds.Contains(x.ParentId) && x.IsActive)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.ParentId,
                x.Name,
                x.ShortCode
            })
            .ToListAsync(cancellationToken);

        var parentCategories = await _dbContext.GeneralSetups
            .AsNoTracking()
            .Where(x => parentCategoryIds.Contains(x.Id))
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.ShortCode,
                x.QuestionType,
                x.Description
            })
            .ToListAsync(cancellationToken);

        var questions = parentCategories
            .Select(parent => new HouseHoldSurveyCategoryDto
            {
                ParentId = parent.Id,
                QuestionText = parent.Name,
                ShortCode = parent.ShortCode,
                QuestionType = parent.QuestionType,
                Description = parent.Description,
                Options = generalSetupItems
                    .Where(o => o.ParentId == parent.Id)
                    .Select(o => new HouseHoldSurveyCategoryOptionDto
                    {
                        OptionId = o.Id,
                        Name = o.Name,
                        ShortCode = o.ShortCode
                    })
                    .ToList()
            })
            .ToList();

        var payload = new HouseHoldSurveyFillListDto
        {
            Source = "GeneralSetups",
            Type = HouseHoldSurveyType,
            Questions = questions
        };

        _logger.LogInformation(
            "GetSurveyCategories returned {Count} HouseHold survey questions from GeneralSetups.",
            questions.Count);

        return PersonServiceResult.Ok(OkResult("OK", payload));
    }
}
