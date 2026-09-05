using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonDeceasedService
{
    Task<PersonServiceResult> LoadDeceasedFormByCNIC(string cnic);

    Task<PersonServiceResult> InsertPersonDeceased(
            int PersonId,
            DateTime? DateOfDeath,
            string TimeOfDeath,
            string PlaceOfDeath,
            string ReportedByName,
            string ReportedByRelation,
            int? CauseOfDeathTypeId,
            int? GraveyardId,
            string DeathPrayerCenter,
            string? HealthConditionIdsCsv,
            string? DeathCertificateFilePath,
            string? AdditionalRemarks);

    Task<PersonServiceResult> DeceasedList(
            int page = 1,
            string keyword = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool useDeathDate = false);

    Task<PersonServiceResult> DeceasedAnalysisThisYear();

    PersonServiceResult SaveDeceasedInfoSimple(
            string deceasedFirstName,
            string deceasedFatherName,
            string? deceasedLastName,
            string deceasedCNIC,
            string deceasedIdentificationType,
            int dobDay,
            int dobMonth,
            int dobYear,
            string gender,
            int jkId,
            string completeAddress,
            string? lat,
            string? lon,
            string relativeFirstName,
            string relativeFatherName,
            string? relativeLastName,
            string relativeGender,
            string? relativeMaritalStatus,
            int relativedobDay,
            int relativedobMonth,
            int relativedobYear,
            string relativeCNIC,
            string relativeIdentificationType,
            int relationshipTypeId,
            string phoneNumber,
            string emailAddress);

}
