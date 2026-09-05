using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public interface IFeeRemissionService
{
    Task<FeeRemissionListResponseDto> GetFeeRemissionListAsync(
        int userId,
        FeeRemissionListRequestDto request,
        CancellationToken cancellationToken = default);
}
