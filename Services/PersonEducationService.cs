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

public class PersonEducationService : PersonServiceBase, IPersonEducationService
{


    public PersonEducationService(NcmisDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<PersonEducationService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {

    }

    public async Task<PersonServiceResult> EducationFamilyGUID(Guid FamilyGUID)
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
                    EducationList = _dbContext.PersonEducationDetails
                        .Where(l => l.PersonId == p.PersonId)
                        .OrderByDescending(l => l.IsActive)
                        .ThenByDescending(l => l.CreatedDate)
                        .Select(e => new
                        {
                            e.EducationId,
                            e.InstitutionName,
                            e.Board,
                            e.BoardType,
                            e.Group,
                            e.DegreeType,
                            e.FieldOfStudy,
                            e.CourseDuration,
                            e.StartDate,
                            e.EndDate,
                            e.PassingDate,
                            e.TotalMarks,
                            e.ObtainedMarks,
                            e.Remarks,
                            e.IsActive,
                            e.Isongoing,
                            FundingSources = _dbContext.PersonEducationFundingSources
                                .Where(f => f.EducationId == e.EducationId)
                                .ToList()
                        }).ToList()
                }).ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "EducationFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SaveEducation(
            int personId,
            string institutionName,
            string boardType,
            string board,
            string group,
            string degreeType,
            string fieldOfStudy,
            string? courseDuration,
            string? startDate,
            string? endDate,
            string? passingDate,
            int? totalMarks,
            int? obtainedMarks,
            string? remarks,
            string? fundingJson,
            bool isOngoing = false,
            bool notknown = false)
        {
            try
            {
                if (personId <= 0) return PersonServiceResult.BadRequest(FailResult("Invalid person selected."));
                if (string.IsNullOrWhiteSpace(institutionName)) return PersonServiceResult.BadRequest(FailResult("Institution name is required."));
                if (string.IsNullOrWhiteSpace(boardType)) return PersonServiceResult.BadRequest(FailResult("Board type is required."));
                if (string.IsNullOrWhiteSpace(board)) return PersonServiceResult.BadRequest(FailResult("Board name is required."));
                if (string.IsNullOrWhiteSpace(group)) return PersonServiceResult.BadRequest(FailResult("Group is required."));
                if (string.IsNullOrWhiteSpace(degreeType)) return PersonServiceResult.BadRequest(FailResult("Degree type is required."));
                if (string.IsNullOrWhiteSpace(fieldOfStudy)) return PersonServiceResult.BadRequest(FailResult("Field of study is required."));

                if (!isOngoing && !notknown)
                {
                    if (string.IsNullOrWhiteSpace(startDate) || !DateTime.TryParse(startDate, out var parsedStart))
                        return PersonServiceResult.BadRequest(FailResult("Invalid or missing start date."));

                    if (!string.IsNullOrWhiteSpace(endDate))
                    {
                        if (!DateTime.TryParse(endDate, out var parsedEnd))
                            return PersonServiceResult.BadRequest(FailResult("Invalid end date."));
                        if (parsedEnd < parsedStart)
                            return PersonServiceResult.BadRequest(FailResult("End date cannot be before start date."));
                    }

                    if (!string.IsNullOrWhiteSpace(passingDate) && !DateTime.TryParse(passingDate, out _))
                        return PersonServiceResult.BadRequest(FailResult("Invalid passing date."));

                    if (totalMarks == null || totalMarks <= 0)
                        return PersonServiceResult.BadRequest(FailResult("Total marks must be greater than 0."));
                    if (obtainedMarks == null || obtainedMarks < 0 || obtainedMarks > totalMarks)
                        return PersonServiceResult.BadRequest(FailResult("Obtained marks must be valid and = total marks."));
                }

            try
            {

                Dictionary<string, List<FundingDto>> fundingDict;
                if (!string.IsNullOrWhiteSpace(fundingJson))
                {
                    fundingDict = JsonSerializer.Deserialize<Dictionary<string, List<FundingDto>>>(fundingJson) ?? new();
                }
            }
            catch (JsonException)
            {
                return PersonServiceResult.BadRequest(FailResult("Invalid funding JSON format."));
                
            }


            var education = new PersonEducationDetail
                {
                    PersonId = personId,
                    InstitutionName = institutionName,
                    BoardType = boardType,
                    Board = board,
                    Group = group,
                    DegreeType = degreeType,
                    FieldOfStudy = fieldOfStudy,
                    CourseDuration = courseDuration,
                    StartDate = string.IsNullOrEmpty(startDate) ? null : DateTime.Parse(startDate),
                    EndDate = string.IsNullOrEmpty(endDate) ? null : DateTime.Parse(endDate),
                    PassingDate = string.IsNullOrEmpty(passingDate) ? null : DateTime.Parse(passingDate),
                    TotalMarks = totalMarks ?? 0,
                    ObtainedMarks = obtainedMarks ?? 0,
                    Remarks = remarks,
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = CurrentUserName,
                    Isongoing = isOngoing,
                    Isnotknown = notknown
                };

                _dbContext.PersonEducationDetails.Add(education);
                await _dbContext.SaveChangesAsync();

                if (!string.IsNullOrEmpty(fundingJson))
                {
                    var fundingDict = JsonSerializer.Deserialize<Dictionary<string, List<FundingDto>>>(fundingJson);
                    if (fundingDict != null)
                    {
                        foreach (var kvp in fundingDict)
                        {
                            foreach (var source in kvp.Value)
                            {
                                int monthlyAmount = source.Amount;
                                int yearlyAmount = monthlyAmount * 12;

                                _dbContext.PersonEducationFundingSources.Add(new PersonEducationFundingSource
                                {
                                    EducationId = education.EducationId,
                                    ExpenseType = kvp.Key,
                                    FundingSourceName = source.Source,
                                    FundingFrequency = source.Frequency,
                                    MonthlyAmount = monthlyAmount,
                                    YearlyAmount = yearlyAmount,
                                    CreatedDate = DateTime.Now,
                                    CreatedBy = CurrentUserName
                                });
                            }
                        }
                        await _dbContext.SaveChangesAsync();
                    }
                }

                return PersonServiceResult.Ok(OkResult("Education and funding saved successfully."));
            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SaveEducation failed");
                return PersonServiceResult.Ok(FailResult(ex.Message));
            }
        }

    public async Task<PersonServiceResult> MarkEducationAsInactive(int educationId, string reason, string? description)
        {
            try
            {
            var record = await _dbContext.PersonEducationDetails.FindAsync(educationId);
            if (record == null)
                return PersonServiceResult.Ok(FailResult("Record not found."));

            if (!record.IsActive)
                return PersonServiceResult.Ok(FailResult("Record is already inactive."));

            record.IsActive = false;
            record.UpdatedDate = DateTime.UtcNow;
            record.UpdatedBy = CurrentUserName;
            record.ReasonForInActive = reason;
            record.DescriptionForInActive = description;
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Education marked as inactive."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MarkEducationAsInactive failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> YouthEducationFamilyGUID(Guid FamilyGUID)
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
                    YouthEducationList = _dbContext.PersonYouthEducations.Where(u => u.PersonId == p.PersonId).ToList()
                }).ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "YouthEducationFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SaveYouthEducation(
            int personId,
            string className,
            string centerName,
            string completionStatus,
            string? completionDate)
        {
            try
            {
            if (personId <= 0) return PersonServiceResult.BadRequest(FailResult("Invalid Person ID."));
            if (string.IsNullOrWhiteSpace(className)) return PersonServiceResult.BadRequest(FailResult("Class is required."));
            if (string.IsNullOrWhiteSpace(centerName)) return PersonServiceResult.BadRequest(FailResult("Center name is required."));
            if (string.IsNullOrWhiteSpace(completionStatus)) return PersonServiceResult.BadRequest(FailResult("Completion status is required."));

            var person = await _dbContext.PersonalInfos.FindAsync(personId);
            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person not found."));

            var education = new PersonYouthEducation
            {
                PersonId = personId,
                FamilyId = person.FamilyId,
                Class = className,
                CenterName = centerName,
                CompletionStatus = completionStatus,
                CompletionDate = string.IsNullOrWhiteSpace(completionDate) ? null : DateTime.Parse(completionDate),
                CreatedBy = CurrentUserName,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            _dbContext.PersonYouthEducations.Add(education);
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Youth education saved successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SaveYouthEducation failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> MarkAsInactive(int educationId, string reason, string? description)
        {
            try
            {
            var record = await _dbContext.PersonYouthEducations.FindAsync(educationId);
            if (record == null)
                return PersonServiceResult.Ok(FailResult("Education record not found."));

            if (!record.IsActive)
                return PersonServiceResult.Ok(FailResult("Already inactive."));

            record.IsActive = false;
            record.UpdatedDate = DateTime.UtcNow;
            record.UpdatedBy = CurrentUserName;
            record.ReasonForInActive = reason;
            record.DescriptionForInActive = description;
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Education record marked as inactive."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MarkAsInactive failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> LifeSkillsFamilyGUID(Guid FamilyGUID)
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
                    PersonLifeSkillList = _dbContext.PersonLifeSkills.Where(u => u.PersonId == p.PersonId).ToList()
                }).ToListAsync();

            var lifeSkills = await _dbContext.LifeSkillsMasters
                .OrderBy(x => x.Category)
                .ThenBy(x => x.SkillName)
                .ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", new { members = persons, lifeSkills }));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "LifeSkillsFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SaveSkillWithDetails(
            int PersonId,
            int SkillId,
            bool IsCertified,
            string? Proficiency,
            bool IsChecked)
        {
            try
            {
                var entry = await _dbContext.PersonLifeSkills
                    .FirstOrDefaultAsync(x => x.PersonId == PersonId && x.SkillId == SkillId);

                if (IsChecked)
                {
                    if (entry == null)
                    {
                        _dbContext.PersonLifeSkills.Add(new PersonLifeSkill
                        {
                            PersonId = PersonId,
                            SkillId = SkillId,
                            IsCertified = IsCertified,
                            Proficiency = Proficiency,
                            Remarks = "",
                            CreatedBy = CurrentUserName,
                            CreatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        entry.IsCertified = IsCertified;
                        entry.Proficiency = Proficiency;
                    }
                }
                else if (entry != null)
                {
                    _dbContext.PersonLifeSkills.Remove(entry);
                }

                await _dbContext.SaveChangesAsync();
                return PersonServiceResult.Ok(OkResult("OK"));
            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SaveSkillWithDetails failed");
                return PersonServiceResult.Ok(FailResult(ex.Message));
            }
        }

    public async Task<PersonServiceResult> AddNewSkill(string SkillName, string Category)
        {
            try
            {
            var normalized = SkillName.Trim().ToLower();

            bool exists = await _dbContext.LifeSkillsMasters
                .AnyAsync(x => x.SkillName.ToLower() == normalized);

            if (exists)
                return PersonServiceResult.Ok(FailResult("Skill already exists."));

            _dbContext.LifeSkillsMasters.Add(new LifeSkillsMaster
            {
                SkillName = SkillName.Trim(),
                Category = Category.Trim()
            });

            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Skill added successfully!"));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "AddNewSkill failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

}



