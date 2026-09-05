using Microsoft.EntityFrameworkCore;
using NCMISAPI.Data;
using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public class GeneralSetupService : IGeneralSetupService
{
    private readonly NcmisDbContext _dbContext;
    private readonly ILogger<GeneralSetupService> _logger;

    public GeneralSetupService(NcmisDbContext dbContext, ILogger<GeneralSetupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<GeneralSetupLookupDto>> GetIncomeItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await GetActiveChildrenByRootTypeAsync("Income", cancellationToken);
        _logger.LogInformation("GetIncomeItems returned {Count} rows.", items.Count);
        return items;
    }

    public async Task<IReadOnlyList<GeneralSetupLookupDto>> GetExpenseItemsAsync(
        CancellationToken cancellationToken = default)
    {
        var items = await GetActiveChildrenByRootTypeAsync("Expense", cancellationToken);
        _logger.LogInformation("GetExpenseItems returned {Count} rows.", items.Count);
        return items;
    }

    private async Task<IReadOnlyList<GeneralSetupLookupDto>> GetActiveChildrenByRootTypeAsync(
        string type,
        CancellationToken cancellationToken)
    {
        var parentIds = await _dbContext.GeneralSetups
            .AsNoTracking()
            .Where(u => u.ParentId == 0 && u.Type == type)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        return await _dbContext.GeneralSetups
            .AsNoTracking()
            .Where(x => parentIds.Contains(x.ParentId) && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new GeneralSetupLookupDto
            {
                Id = x.Id,
                ParentId = x.ParentId,
                Name = x.Name,
                ShortCode = x.ShortCode,
                Type = x.Type
            })
            .ToListAsync(cancellationToken);
    }
}
