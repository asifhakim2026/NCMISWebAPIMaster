using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonService
{
    Task<PersonServiceResult> SearchPersonByCNIC(string cnic);

    Task<PersonServiceResult> SearchPersonByFamilyGUID(Guid FamilyGUID, Guid? ProjectGuid = null);

    Task<PersonServiceResult> familysummary(Guid FamilyGUID);

    Task<PersonServiceResult> personalinformation(Guid FamilyGUID, Guid? ProjectGuid = null);

    Task<PersonServiceResult> UpdatePersonalInfo(
            int personId,
            string? image,
            int jkid,
            int relationshipid,
            string firstName,
            string lastName,
            string? surname,
            string cnicidentificationtype,
            string cnic,
            DateTime? cnicIssueDate,
            DateTime? cnicExpiryDate,
            string? email,
            string? phone,
            string gender,
            string maritalStatus,
            DateTime? dateOfBirth,
            bool isDeceased,
            DateTime? deceasedDate,
            string? cnicfront,
            string? cnicback,
            string educationstatus,
            string youtheducationstatus,
            string workemploymentstatus,
            string disabilitystatus,
            string? disabilities,
            string substanceabusestatus,
            string? substanceabuse,
            string? btscode);

    Task<PersonServiceResult> MakeHeadOfFamily(int personId);

    Task<PersonServiceResult> GetPersonList(
            string? searchTerm,
            string sortColumn = "FirstName",
            bool isAscending = true,
            int page = 1);

    Task<PersonServiceResult> CreateQuickPerson(
            string cnic,
            string identificationtype,
            string firstname,
            string lastname,
            string? surname,
            string? email,
            string? phone,
            string gender,
            int jkid,
            string maritalstatus,
            DateTime? dob,
            Guid familyguid);

    Task<PersonServiceResult> ImportHouseholdData(string? rawText, string? rawTextQuery = null);

}
