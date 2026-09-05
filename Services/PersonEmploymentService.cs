using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NCMIS.Models;
using NCMISAPI.Data;
using NCMISAPI.Helpers;
using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public class PersonEmploymentService : PersonServiceBase, IPersonEmploymentService
{
    public PersonEmploymentService(NcmisDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<PersonEmploymentService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {
    }

    public async Task<PersonServiceResult> WorkExperienceFamilyGUID(Guid FamilyGUID)
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
                ExperienceList = _dbContext.PersonWorkExperiences
                    .Where(e => e.PersonId == p.PersonId)
                    .OrderByDescending(e => e.IsActive)
                    .ThenByDescending(e => e.CreatedDate)
                    .Select(e => new
                    {
                        e.WorkExperienceId,
                        e.FromDate,
                        e.ToDate,
                        e.NameOfEmployer,
                        e.AddressOfEmployer,
                        e.Designation,
                        e.MajorResponsibility,
                        e.IsOngoing,
                        e.EmploymentStatus,
                        e.IncomePerMonth,
                        e.IsActive,
                        e.CreatedDate,
                        e.CreatedBy,
                        e.UpdatedDate,
                        e.UpdatedBy,
                        PersonComponentList = _dbContext.PersonWorkIncomeComponents
                            .Where(c => c.WorkExperienceId == e.WorkExperienceId)
                            .Select(c => new
                            {
                                c.ComponentId,
                                c.WorkExperienceId,
                                c.ComponentType,
                                c.Amount,
                                c.Frequency,
                                c.Notes,
                                c.CreatedDate,
                                c.CreatedBy
                            }).ToList()
                    }).ToList()
            }).ToListAsync();

        return PersonServiceResult.Ok(OkResult("OK", persons));
    }

    public async Task<PersonServiceResult> SaveWorkExperience(
            int personId,
            string? designation,
            decimal? incomePerMonth,
            string? fromDate,
            string? toDate,
            string? isOngoing,
            string? employerName,
            string? employerAddress,
            string? responsibilities,
            string? incomeComponentsJson)
    {
        if (personId <= 0) return PersonServiceResult.BadRequest(FailResult("Invalid person."));
        if (string.IsNullOrWhiteSpace(designation)) return PersonServiceResult.BadRequest(FailResult("Designation is required."));
        if (incomePerMonth == null || incomePerMonth <= 0)
            return PersonServiceResult.BadRequest(FailResult("Income per month must be greater than 0."));
        if (string.IsNullOrWhiteSpace(fromDate) || !DateTime.TryParse(fromDate, out DateTime fromDt))
            return PersonServiceResult.BadRequest(FailResult("Valid 'From Date' is required."));
        if (string.IsNullOrWhiteSpace(isOngoing) || (isOngoing.ToUpper() != "YES" && isOngoing.ToString() != "NO"))
            return PersonServiceResult.BadRequest(FailResult("Please select if the job is ongoing."));

        DateTime? toDt = null;
        if (isOngoing == "No")
        {
            if (string.IsNullOrWhiteSpace(toDate) || !DateTime.TryParse(toDate, out DateTime parsedToDate))
                return PersonServiceResult.BadRequest(FailResult("'To Date' is required when job is not ongoing."));
            if (parsedToDate < fromDt)
                return PersonServiceResult.BadRequest(FailResult("'To Date' cannot be earlier than 'From Date'."));
            toDt = parsedToDate;
        }

        if (string.IsNullOrWhiteSpace(employerName)) return PersonServiceResult.BadRequest(FailResult("Employer name is required."));
        if (string.IsNullOrWhiteSpace(employerAddress)) return PersonServiceResult.BadRequest(FailResult("Employer address is required."));
        if (string.IsNullOrWhiteSpace(responsibilities)) return PersonServiceResult.BadRequest(FailResult("Major responsibilities are required."));

        var person = await _dbContext.PersonalInfos.FirstOrDefaultAsync(u => u.PersonId == personId);
        if (person == null) return PersonServiceResult.Ok(FailResult("Person not found."));

        List<WorkIncomeComponentDto>? components = null;
        if (!string.IsNullOrWhiteSpace(incomeComponentsJson))
        {
            components = JsonSerializer.Deserialize<List<WorkIncomeComponentDto>>(incomeComponentsJson);
            if (components != null)
            {
                foreach (var c in components)
                {
                    if (string.IsNullOrWhiteSpace(c.componentType))
                        return PersonServiceResult.BadRequest(FailResult("Component type is required for all components."));
                    if (c.amount <= 0)
                        return PersonServiceResult.BadRequest(FailResult($"Amount must be greater than 0 for component '{c.componentType}'."));
                    if (string.IsNullOrWhiteSpace(c.frequency))
                        return PersonServiceResult.BadRequest(FailResult($"Frequency is required for component '{c.componentType}'."));
                }
            }
        }

        var workExperience = new PersonWorkExperience
        {
            PersonId = personId,
            Designation = designation,
            FamilyId = person.FamilyId,
            IncomePerMonth = incomePerMonth,
            FromDate = fromDt,
            ToDate = toDt,
            IsOngoing = isOngoing,
            NameOfEmployer = employerName,
            AddressOfEmployer = employerAddress,
            MajorResponsibility = responsibilities,
            CreatedDate = DateTime.Now,
            CreatedBy = CurrentUserName,
            IsActive = true
        };

        _dbContext.PersonWorkExperiences.Add(workExperience);
        await _dbContext.SaveChangesAsync();

        if (components != null)
        {
            foreach (var comp in components)
            {
                _dbContext.PersonWorkIncomeComponents.Add(new PersonWorkIncomeComponent
                {
                    WorkExperienceId = workExperience.WorkExperienceId,
                    ComponentType = comp.componentType,
                    Amount = comp.amount,
                    Frequency = comp.frequency,
                    Notes = comp.notes,
                    CreatedBy = CurrentUserName,
                    CreatedDate = DateTime.Now
                });
            }
            await _dbContext.SaveChangesAsync();
        }

        return PersonServiceResult.Ok(OkResult("Work experience saved successfully."));
    }

    public async Task<PersonServiceResult> MarkExperienceAsInactive(int workExperienceId, string reason, string? description)
    {
        var experience = await _dbContext.PersonWorkExperiences
            .FirstOrDefaultAsync(x => x.WorkExperienceId == workExperienceId);

        if (experience == null)
            return PersonServiceResult.Ok(FailResult("Experience not found."));

        experience.IsActive = false;
        experience.UpdatedDate = DateTime.Now;
        experience.UpdatedBy = CurrentUserName;
        experience.ReasonForInActive = reason;
        experience.DescriptionForInActive = description;
        await _dbContext.SaveChangesAsync();
        return PersonServiceResult.Ok(OkResult("Experience marked as inactive."));
    }
}
