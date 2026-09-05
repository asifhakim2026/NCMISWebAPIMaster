using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NCMIS.Models;
using NCMISAPI.Data;
using NCMISAPI.DTOs.Person;
using NCMISAPI.Helpers;

namespace NCMISAPI.Services;

public class PersonDocumentService : PersonServiceBase, IPersonDocumentService
{


    public PersonDocumentService(NcmisDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<PersonDocumentService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {

    }

    public async Task<PersonServiceResult> SeniorCitizenFamilyGUID(Guid FamilyGUID)
        {
            try
            {
            var familyGroup = await _dbContext.FamilyGroups
                .FirstOrDefaultAsync(fg => fg.FamilyGroupGuid == FamilyGUID);

            if (familyGroup == null)
                return PersonServiceResult.Ok(FailResult("Family group not found"));

            var today = DateTime.Today;

            var persons = await (
                from fg in _dbContext.FamilyGroups
                join p in _dbContext.PersonalInfos on fg.FamilyId equals p.FamilyId
                join pf in _dbContext.PersonFamilies on p.PersonId equals pf.PersonId
                join rt in _dbContext.RelationshipTypes on pf.RelationshipTypeId equals rt.RelationshipTypeId
                join jk in _dbContext.Locations on p.JKID equals jk.LocationID
                join lc in _dbContext.Locations on jk.ParentID equals lc.LocationID
                join rc in _dbContext.Locations on lc.ParentID equals rc.LocationID
                where p.FamilyId == familyGroup.FamilyId
                      && !p.IsDeceased
                      && p.DateOfBirth != null
                      && EF.Functions.DateDiffYear(p.DateOfBirth.Value, today) >= 60
                orderby rt.SortOrder
                select new
                {
                    PersonGuid = p.PersonalGuid,
                    p.PersonId,
                    PersonName = p.FirstName + " " + (p.LastName ?? ""),
                    p.CNIC,
                    p.IdentificationType,
                    p.Gender,
                    p.DateOfBirth,
                    RelationshipRole = p.PersonId == familyGroup.HeadPersonId ? "Head of Family" : "Family Member",
                    familyGroup.FamilyGroupCode,
                    FamilyCreatedDate = familyGroup.CreatedDate,
                    RelatedTo = p.PersonId == familyGroup.HeadPersonId ? "Self" : "Family Member",
                    RelationshipName = rt.Name,
                    rt.RelationshipTypeId,
                    Image = p.ImagePath ?? "/img/noimage.png",
                    Email = p.Email ?? "NA",
                    Phone = p.Phone ?? "NA",
                    MaritalStatus = p.MaritalStatus ?? "NA",
                    p.CNICIssueDate,
                    p.CNICExpiryDate,
                    CNICExpiryStatus = p.CNICExpiryDate != null ? (p.CNICExpiryDate < DateTime.Now ? "Expired" : "") : "",
                    p.PersonCode,
                    FamilyCode = fg.FamilyGroupCode,
                    p.CreatedDate,
                    p.CreatedBy,
                    UpdateDate = p.UpdateDate,
                    p.UpdatedBy,
                    SeniorCitizens = _dbContext.PersonSeniorCitizens
                        .Where(l => l.PersonId == p.PersonId)
                        .OrderByDescending(l => l.IsActive)
                        .ThenByDescending(l => l.CreatedDate)
                        .ToList()
                }).ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SeniorCitizenFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SavePersonSeniorCard(
            int personId,
            string cardNumber,
            DateTime? issueDate,
            DateTime? expiryDate,
            string issuerType,
            string issuedBy,
            string? amenities,
            string? description)
        {
            try
            {
            if (personId <= 0) return PersonServiceResult.BadRequest(FailResult("Invalid person."));
            if (string.IsNullOrWhiteSpace(cardNumber)) return PersonServiceResult.BadRequest(FailResult("Card number is required."));
            if (string.IsNullOrWhiteSpace(issuerType)) return PersonServiceResult.BadRequest(FailResult("Please select issuer type."));
            if (string.IsNullOrWhiteSpace(issuedBy)) return PersonServiceResult.BadRequest(FailResult("Issued by is required."));
            if (issueDate != null && expiryDate != null && issueDate >= expiryDate)
                return PersonServiceResult.BadRequest(FailResult("Issue date must be earlier than expiry date."));

            var seniorCard = new PersonSeniorCitizen
            {
                PersonId = personId,
                CardNumber = cardNumber.Trim(),
                IssueDate = issueDate,
                ExpiryDate = expiryDate,
                IssuerType = issuerType.Trim(),
                IssuedBy = issuedBy.Trim(),
                Amenities = amenities?.Trim() ?? "",
                Description = description?.Trim(),
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = CurrentUserName
            };

            _dbContext.PersonSeniorCitizens.Add(seniorCard);
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Senior Citizen Card saved successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SavePersonSeniorCard failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> MarkSeniorCardInactive(int id, string reason, string? description)
        {
            try
            {
            var card = await _dbContext.PersonSeniorCitizens.FindAsync(id);
            if (card == null)
                return PersonServiceResult.Ok(FailResult("Card not found."));

            card.IsActive = false;
            card.ReasonForInActive = reason;
            card.DescriptionForInActive = description;
            card.UpdatedBy = CurrentUserName;
            card.UpdatedDate = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Senior Citizen Card marked as inactive."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MarkSeniorCardInactive failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

 

    public async Task<PersonServiceResult> SavePersonAttachment(int personId, string attachmentName, string attachmentUrl)
        {
            try
            {
            if (string.IsNullOrWhiteSpace(attachmentName) || string.IsNullOrWhiteSpace(attachmentUrl) || personId == 0)
                return PersonServiceResult.BadRequest(FailResult("Invalid input data."));

            var person = await _dbContext.PersonalInfos.FindAsync(personId);
            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person not found."));

            var attachment = new PersonAttachment
            {
                PersonId = personId,
                FamilyId = person.FamilyId ?? 0,
                AttachmentName = attachmentName,
                AttachmentURL = attachmentUrl,
                CreatedDate = DateTime.Now,
                CreatedBy = CurrentUserName,
                Status = "Uploaded",
                Isactive = true
            };

            _dbContext.PersonAttachments.Add(attachment);
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Attachment saved."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SavePersonAttachment failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> FamilyAttachmentFamilyGUID(Guid FamilyGUID)
        {
            try
            {
            var familyGroup = await _dbContext.FamilyGroups
                .FirstOrDefaultAsync(fg => fg.FamilyGroupGuid == FamilyGUID);

            if (familyGroup == null)
                return PersonServiceResult.Ok(FailResult("Family group not found"));

            var persons = await (
                from fg in _dbContext.FamilyGroups
                join p in _dbContext.PersonalInfos on fg.FamilyId equals p.FamilyId
                join pf in _dbContext.PersonFamilies on p.PersonId equals pf.PersonId
                join rt in _dbContext.RelationshipTypes on pf.RelationshipTypeId equals rt.RelationshipTypeId
                join jk in _dbContext.Locations on p.JKID equals jk.LocationID
                join lc in _dbContext.Locations on jk.ParentID equals lc.LocationID
                join rc in _dbContext.Locations on lc.ParentID equals rc.LocationID
                where p.FamilyId == familyGroup.FamilyId && !p.IsDeceased
                orderby rt.SortOrder
                select new
                {
                    p.PersonId,
                    PersonName = p.FirstName + " " + (p.LastName ?? ""),
                    AttachmentList = _dbContext.PersonAttachments.Where(u => u.PersonId == p.PersonId).ToList()
                }).ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "FamilyAttachmentFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

}



