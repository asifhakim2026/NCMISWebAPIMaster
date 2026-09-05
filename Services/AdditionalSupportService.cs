using Microsoft.EntityFrameworkCore;
using NCMISAPI.Data;
using NCMISAPI.DTOs;
using NCMISAPI.DTOs.Person;
using NCMISAPI.Helpers;

namespace NCMISAPI.Services;

/// <summary>
/// Additional Support categories from GeneralSetups Type == SupportSurvey.
/// </summary>
public class AdditionalSupportService : PersonServiceBase, IAdditionalSupportService
{
    private const string SupportSurveyType = "SupportSurvey";

    public AdditionalSupportService(
        NcmisDbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AdditionalSupportService> logger,
        ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {
    }

    public async Task<PersonServiceResult> GetSupportCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var parents = await _dbContext.GeneralSetups
            .AsNoTracking()
            .Where(u => u.ParentId == 0 && u.Type == SupportSurveyType && u.IsActive)
            .OrderBy(u => u.Id)
            .Select(u => new
            {
                u.Id,
                u.Name,
                u.ShortCode,
                u.QuestionType,
                u.Description
            })
            .ToListAsync(cancellationToken);

        var parentIds = parents.Select(p => p.Id).ToList();

        var options = await _dbContext.GeneralSetups
            .AsNoTracking()
            .Where(x => parentIds.Contains(x.ParentId) && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.ParentId,
                x.Name,
                x.ShortCode
            })
            .ToListAsync(cancellationToken);

        var categories = parents
            .Select(parent => new AdditionalSupportCategoryDto
            {
                ParentId = parent.Id,
                Name = parent.Name,
                ShortCode = parent.ShortCode,
                QuestionType = parent.QuestionType,
                Description = parent.Description,
                Options = options
                    .Where(o => o.ParentId == parent.Id)
                    .Select(o => new AdditionalSupportCategoryOptionDto
                    {
                        OptionId = o.Id,
                        Name = o.Name,
                        ShortCode = o.ShortCode
                    })
                    .ToList()
            })
            .ToList();

        var payload = new AdditionalSupportFillListDto
        {
            Categories = categories
        };

        _logger.LogInformation(
            "GetSupportCategories returned {Count} active SupportSurvey categories from GeneralSetups.",
            categories.Count);

        return PersonServiceResult.Ok(OkResult("OK", payload));
    }
}
