using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonEmploymentService
{
    Task<PersonServiceResult> WorkExperienceFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SaveWorkExperience(
            int personId,
            string? designation,
            decimal? incomePerMonth,
            string? fromDate,
            string? toDate,
            string? isOngoing,
            string? employerName,
            string? employerAddress,
            string? responsibilities,
            string? incomeComponentsJson);

    Task<PersonServiceResult> MarkExperienceAsInactive(int workExperienceId, string reason, string? description);

}
