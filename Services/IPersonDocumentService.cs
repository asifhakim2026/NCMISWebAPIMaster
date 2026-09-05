using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonDocumentService
{
    Task<PersonServiceResult> SeniorCitizenFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SavePersonSeniorCard(
            int personId,
            string cardNumber,
            DateTime? issueDate,
            DateTime? expiryDate,
            string issuerType,
            string issuedBy,
            string? amenities,
            string? description);

    Task<PersonServiceResult> MarkSeniorCardInactive(int id, string reason, string? description);


    Task<PersonServiceResult> SavePersonAttachment(int personId, string attachmentName, string attachmentUrl);

    Task<PersonServiceResult> FamilyAttachmentFamilyGUID(Guid FamilyGUID);

}
