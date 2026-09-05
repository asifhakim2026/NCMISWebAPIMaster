using Microsoft.EntityFrameworkCore;
using NCMIS.Models;
using NCMISAPI.Data;
using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public class PersonHelper
{
    private readonly NcmisDbContext _dbContext;
    private readonly ILogger<PersonHelper> _logger;

    public PersonHelper(NcmisDbContext dbContext, ILogger<PersonHelper> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public (bool hasPendingApplication, string? message) CheckIfPersonHasPendingApplication(
        int personId,
        Guid? projectGuid)
    {
        if (projectGuid is null)
            return (false, null);

        var projectId = _dbContext.Projects
            .AsNoTracking()
            .Where(p => p.ProjectGUID == projectGuid)
            .Select(p => (int?)p.ProjectID)
            .FirstOrDefault();

        if (projectId is null)
            return (false, null);

        var hasPending = (
            from fee in _dbContext.FeesRemisssions.AsNoTracking()
            join wt in _dbContext.WorkflowTrackers.AsNoTracking()
                on fee.FeeRemissionId equals wt.ApplicationId
            where fee.PersonId == personId
                  && fee.ProjectId == projectId
                  && wt.ModuleName == "FeesRemission"
                  && fee.IsCurrentStepActive == true
            select fee.FeeRemissionId
        ).Any();

        return (hasPending, hasPending ? "Applicant" : null);
    }

    public List<PersonProjectEnrollmentDto> GetPersonProjectEnrollments(int personId)
    {
        return (
            from e in _dbContext.PersonEnrollments.AsNoTracking()
            join p in _dbContext.Projects.AsNoTracking() on e.ProjectId equals p.ProjectID
            where e.PersonId == personId
            orderby e.InsertDate descending
            select new PersonProjectEnrollmentDto
            {
                EnrollmentId = e.EnrollmemntId,
                ProjectId = e.ProjectId,
                ProjectGUID = p.ProjectGUID,
                Module = e.Module,
                ReferenceID = e.ReferenceID,
                Remarks = e.Remarks,
                InsertDate = e.InsertDate
            }
        ).ToList();
    }

    public PersonalInfo? GetOrCreatePerson(
        string firstName,
        string fathername,
        string lastName,
        string cnic,
        string? phone,
        string? email,
        string gender,
        DateTime? dob,
        int jkId,
        string createdBy,
        int familyGroupId,
        string? completeaddress,
        string? lat,
        string? lon,
        string? maritalstatus,
        string? identificationtype)
    {
        var existing = _dbContext.PersonalInfos.FirstOrDefault(p => p.CNIC == cnic);
        if (existing != null)
            return existing;

        var latestCode = _dbContext.PersonalInfos
            .OrderByDescending(p => p.PersonId)
            .Select(p => p.PersonCode)
            .FirstOrDefault();

        var nextNum = 1;
        if (!string.IsNullOrWhiteSpace(latestCode))
        {
            var digits = new string(latestCode.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out var n))
                nextNum = n + 1;
        }

        var person = new PersonalInfo
        {
            PersonalGuid = Guid.NewGuid(),
            PersonCode = GenerateUniquePersonCode(),
            FirstName = firstName.Trim(),
            LastName = fathername.Trim(),
            Surname = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim(),
            CNIC = cnic.Trim(),
            IdentificationType = identificationtype,
            Phone = phone,
            Email = email,
            Gender = gender,
            DateOfBirth = dob,
            JKID = jkId,
            FamilyId = familyGroupId > 0 ? familyGroupId : null,
            MaritalStatus = maritalstatus,
            IsActive = true,
            IsDeceased = false,
            CreatedBy = createdBy,
            CreatedDate = DateTime.Now,
            CompletedTabs = 0
        };

        _dbContext.PersonalInfos.Add(person);
        _dbContext.SaveChanges();

        if (!string.IsNullOrWhiteSpace(completeaddress))
        {
            double.TryParse(lat, out var latitude);
            double.TryParse(lon, out var longitude);

            _dbContext.PersonAddress.Add(new PersonAddress
            {
                PersonId = person.PersonId,
                AddressGuid = Guid.NewGuid(),
                AddressType = "Current",
                AddressLine1 = completeaddress,
                VillageOrCity = "",
                LocationType = "",
                UnionCouncil = "",
                TaluqaTehsil = "",
                District = "",
                City = "",
                State = "",
                Country = "",
                PostalCode = "",
                HouseOrFlatNumber = "",
                TypeofHouse = "",
                IsOwnedRented = "",
                Latitude = latitude,
                Longitude = longitude,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedDate = DateTime.Now
            });
            _dbContext.SaveChanges();
        }

        if (familyGroupId <= 0)
        {
            var family = new FamilyGroup
            {
                FamilyGroupGuid = Guid.NewGuid(),
                FamilyGroupCode = $"FG-{DateTime.Now:yyyyMM}-{person.PersonId:D4}",
                HeadPersonId = person.PersonId,
                CreatedBy = createdBy,
                CreatedDate = DateTime.Now
            };
            _dbContext.FamilyGroups.Add(family);
            _dbContext.SaveChanges();

            person.FamilyId = family.FamilyId;
            _dbContext.SaveChanges();
        }

        return person;
    }
    private readonly object _PIlock = new object();
    public string GenerateUniquePersonCode()
    {
        lock (_PIlock)
        {
            int nextId = 1;

            var lastCode = _dbContext.PersonalInfos
                .AsEnumerable()
                .Select(p =>
                {
                    if (p.PersonCode != null)
                    {
                        var parts = p.PersonCode.Split('-');
                        if (parts.Length == 3 && int.TryParse(parts[2], out int number))
                        {
                            return number;
                        }
                    }
                    return 0;
                })
                .DefaultIfEmpty()
                .Max();

            nextId = lastCode + 1;

            string datePart = DateTime.Now.ToString("yyMMdd");
            return $"P-{datePart}-{nextId:D4}";
        }
    }
    public void SavePersonRelation(int personId, int? relatedPersonId, int relationshipTypeId, string createdBy)
    {
        if (relatedPersonId is null or <= 0)
            return;

        var existing = _dbContext.PersonFamilies.FirstOrDefault(r =>
            r.PersonId == personId && r.RelatedPersonId == relatedPersonId);

        if (existing != null)
        {
            existing.RelationshipTypeId = relationshipTypeId;
            existing.UpdatedBy = createdBy;
            existing.UpdatedDate = DateTime.Now;
        }
        else
        {
            _dbContext.PersonFamilies.Add(new PersonFamily
            {
                PersonId = personId,
                RelatedPersonId = relatedPersonId.Value,
                RelationshipTypeId = relationshipTypeId,
                CreatedBy = createdBy,
                CreatedDate = DateTime.Now
            });
        }

        _dbContext.SaveChanges();
    }

    public async Task<DeceasedViewDto?> GetDeceasedViewByPersonIdAsync(int personId)
    {
        var deceased = await (
            from d in _dbContext.PersonDeceasedInfos.AsNoTracking()
            join p in _dbContext.PersonalInfos.AsNoTracking() on d.PersonId equals p.PersonId
            where d.PersonId == personId
            select new DeceasedViewDto
            {
                PersonDeceasedId = d.PersonDeceasedId,
                PersonId = p.PersonId,
                PersonFullName = $"{p.FirstName} {p.LastName} {p.Surname}".Trim(),
                CNIC = p.CNIC,
                DateOfDeath = d.DateOfDeath,
                TimeOfDeath = d.TimeOfDeath,
                PlaceOfDeath = d.PlaceOfDeath,
                ReportedByName = d.ReportedByName,
                ReportedByRelation = d.ReportedByRelation,
                DeceasedShortCode = d.DeceasedShortCode,
                DeathCertificateFilePath = d.DeathCertificateFilePath,
                AdditionalRemarks = d.AdditionalRemarks
            }
        ).FirstOrDefaultAsync();

        return deceased;
    }
}

