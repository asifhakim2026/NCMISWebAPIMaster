using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonFieldWorkService
{
    Task<PersonServiceResult> SurveyorNotesFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SurveyNotesFamily(
            int familyid,
            string notes,
            decimal? latitude,
            decimal? longitude,
            string? imagePath,
            string? address);

    Task<PersonServiceResult> GetSurveyorFamilyNotes(int familyId);

    Task<PersonServiceResult> GetFamilyVerificationSummary(Guid FamilyGUID);

    Task<PersonServiceResult> SaveFamilyVerification(
            string? SignatureBase64,
            int FamilyId,
            string SignedBy,
            string VerifiedDataJson);

    Task<PersonServiceResult> ViewFamilyVerification(Guid FamilyGUID);

}
