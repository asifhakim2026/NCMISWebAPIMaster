using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public interface IGeneralSetupService
{
    Task<IReadOnlyList<GeneralSetupLookupDto>> GetIncomeItemsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GeneralSetupLookupDto>> GetExpenseItemsAsync(CancellationToken cancellationToken = default);
}
