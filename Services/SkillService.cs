using Microsoft.EntityFrameworkCore;
using NCMISAPI.Data;
using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public class SkillService : ISkillService
{
    private readonly NcmisDbContext _dbContext;
    private readonly ILogger<SkillService> _logger;

    public SkillService(NcmisDbContext dbContext, ILogger<SkillService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<IReadOnlyList<LifeSkillDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var skills = await _dbContext.LifeSkillsMasters
            .AsNoTracking()
            .OrderBy(x => x.Category)
            .ThenBy(x => x.SkillName)
            .Select(x => new LifeSkillDto
            {
                SkillId = x.SkillId,
                SkillName = x.SkillName,
                Category = x.Category
            })
            .ToListAsync(cancellationToken);

        _logger.LogInformation("GetAll skills returned {Count} rows.", skills.Count);
        return skills;
    }
}
