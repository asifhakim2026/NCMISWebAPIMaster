using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public interface ISkillService
{
    Task<IReadOnlyList<LifeSkillDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
