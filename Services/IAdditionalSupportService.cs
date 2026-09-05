using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IAdditionalSupportService
{
    /// <summary>
    /// Active SupportSurvey categories from GeneralSetups (parents + active options)
    /// for the Additional Support form — no familyGuid required.
    /// </summary>
    Task<PersonServiceResult> GetSupportCategoriesAsync(CancellationToken cancellationToken = default);
}
