using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IHouseHoldSurveyService
{
    /// <summary>
    /// HouseHold survey question bank from GeneralSetups (parents + active options)
    /// for the Housing and Assets form — no familyGuid required.
    /// </summary>
    Task<PersonServiceResult> GetSurveyCategoriesAsync(CancellationToken cancellationToken = default);
}
