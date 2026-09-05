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

public class PersonDeceasedService : PersonServiceBase, IPersonDeceasedService
{
    private readonly PersonHelper _personHelper;
    private readonly object _deceasedCodeLock = new();

    public PersonDeceasedService(
        NcmisDbContext dbContext,
        PersonHelper personHelper,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PersonDeceasedService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {
        _personHelper = personHelper;
    }

    public async Task<PersonServiceResult> LoadDeceasedFormByCNIC(string cnic)
        {
            try
            {
            if (string.IsNullOrWhiteSpace(cnic))
                return PersonServiceResult.BadRequest(FailResult("Please enter a CNIC."));

            var person = await _dbContext.PersonalInfos
                .Where(p => p.CNIC == cnic && p.IsActive)
                .FirstOrDefaultAsync();

            if (person == null)
                return PersonServiceResult.Ok(FailResult("No person found for this CNIC."));

            var deceasedView = await _personHelper.GetDeceasedViewByPersonIdAsync(person.PersonId);
            if (deceasedView != null)
                return PersonServiceResult.Ok(OkResult("Already deceased", new { existing = true, deceasedView }));

            var healthConditions = await _dbContext.SetupHealthConditions
                .OrderBy(h => h.ConditionName)
                .ToListAsync();

            var graveyards = await _dbContext.Graveyards
                .OrderBy(g => g.Name)
                .ToListAsync();

            var causeOfDeathTypes = await _dbContext.SetupCauseOfDeathTypes
                .OrderBy(c => c.Name)
                .ToListAsync();

            var model = new
            {
                person.PersonId,
                PersonFullName = $"{person.FirstName} {person.LastName} {person.Surname}".Trim(),
                person.CNIC,
                Age = person.DateOfBirth != null ? (int)((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365) : (int?)null,
                Image = person.ImagePath ?? "/img/noimage.png",
                PersonShortCode = person.PersonCode,
                HealthConditions = healthConditions,
                Graveyards = graveyards,
                CauseOfDeathTypes = causeOfDeathTypes
            };

            return PersonServiceResult.Ok(OkResult("OK", new { existing = false, form = model }));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "LoadDeceasedFormByCNIC failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> InsertPersonDeceased(
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
            string? AdditionalRemarks)
        {
            try
            {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                if (PersonId <= 0)
                    return PersonServiceResult.BadRequest(FailResult("Invalid person selection."));
                if (DateOfDeath == null)
                    return PersonServiceResult.BadRequest(FailResult("Date of death is required."));
                if (string.IsNullOrWhiteSpace(TimeOfDeath))
                    return PersonServiceResult.BadRequest(FailResult("Time of death is required."));
                if (string.IsNullOrWhiteSpace(PlaceOfDeath))
                    return PersonServiceResult.BadRequest(FailResult("Place of death is required."));
                if (string.IsNullOrWhiteSpace(ReportedByName))
                    return PersonServiceResult.BadRequest(FailResult("Reported By (Name) is required."));
                if (string.IsNullOrWhiteSpace(ReportedByRelation))
                    return PersonServiceResult.BadRequest(FailResult("Relation of reporter is required."));
                if (CauseOfDeathTypeId is null or <= 0)
                    return PersonServiceResult.BadRequest(FailResult("Please select a valid cause of death."));
                if (GraveyardId is null or <= 0)
                    return PersonServiceResult.BadRequest(FailResult("Please select a valid graveyard."));
                if (string.IsNullOrWhiteSpace(DeathPrayerCenter))
                    return PersonServiceResult.BadRequest(FailResult("Death prayer center is required."));

                var person = await _dbContext.PersonalInfos.FirstOrDefaultAsync(u => u.PersonId == PersonId);
                if (person == null)
                    return PersonServiceResult.Ok(FailResult("No person found for the given ID."));

                var deceased = new PersonDeceasedInfo
                {
                    PersonId = PersonId,
                    DateOfDeath = DateOfDeath.Value,
                    TimeOfDeath = TimeOfDeath,
                    PlaceOfDeath = PlaceOfDeath,
                    ReportedByName = ReportedByName,
                    ReportedByRelation = ReportedByRelation,
                    SetupCauseOfDeathId = CauseOfDeathTypeId.Value,
                    GraveyardId = GraveyardId.Value,
                    DeathPrayerCenter = DeathPrayerCenter,
                    DeathCertificateFilePath = DeathCertificateFilePath,
                    AdditionalRemarks = AdditionalRemarks,
                    DeceasedShortCode = GenerateDeceasedShortCode(),
                    CreatedDate = DateTime.Now,
                    CreatedBy = CurrentUserName
                };

                _dbContext.PersonDeceasedInfos.Add(deceased);
                await _dbContext.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(HealthConditionIdsCsv))
                {
                    var conditionIds = HealthConditionIdsCsv.Split(',')
                        .Select(id => int.TryParse(id, out var parsedId) ? parsedId : 0)
                        .Where(id => id > 0)
                        .ToList();

                    if (conditionIds.Count > 0)
                    {
                        var healthEntries = conditionIds.Select(cid => new PersonHealthCondition
                        {
                            PersonDeceasedInfoId = deceased.PersonDeceasedId,
                            SetupHealthConditionId = cid
                        }).ToList();

                        _dbContext.PersonHealthConditions.AddRange(healthEntries);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                person.IsDeceased = true;
                person.DeceasedDate = DateOfDeath.Value;
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return PersonServiceResult.Ok(OkResult("Deceased info saved."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                LogAndPersistError(ex, "InsertPersonDeceased failed");
                return PersonServiceResult.Ok(FailResult("Error: " + ex.Message));
            }

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "InsertPersonDeceased failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> DeceasedList(
            int page = 1,
            string keyword = "",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool useDeathDate = false)
        {
            try
            {
            const int pageSize = 10;
            fromDate ??= DateTime.Today.AddMonths(-1);
            toDate ??= DateTime.Today;

            var query =
                from d in _dbContext.PersonDeceasedInfos
                join p in _dbContext.PersonalInfos on d.PersonId equals p.PersonId
                join g in _dbContext.Graveyards on d.GraveyardId equals g.Id
                join c in _dbContext.SetupCauseOfDeathTypes on d.SetupCauseOfDeathId equals c.SetupCauseOfDeathId
                join f in _dbContext.FamilyGroups on p.FamilyId equals f.FamilyId
                where p.IsActive &&
                      (string.IsNullOrEmpty(keyword) ||
                       (p.FirstName + " " + p.LastName).Contains(keyword) ||
                       p.CNIC.Contains(keyword) ||
                       p.PersonCode.Contains(keyword)) &&
                      (useDeathDate
                          ? d.DateOfDeath.Date >= fromDate && d.DateOfDeath.Date <= toDate
                          : d.CreatedDate.Date >= fromDate && d.CreatedDate.Date <= toDate)
                orderby d.DateOfDeath descending
                select new
                {
                    d.PersonDeceasedId,
                    p.PersonId,
                    PersonShortCode = p.PersonCode,
                    PersonFullName = p.FirstName + " " + p.LastName,
                    p.CNIC,
                    FamilyCode = f.FamilyGroupCode,
                    Age = (p.DateOfBirth.HasValue)
                        ? d.DateOfDeath.Year - p.DateOfBirth.Value.Year
                        : 0,
                    p.Gender,
                    Image = p.ImagePath,
                    d.DateOfDeath,
                    d.TimeOfDeath,
                    d.PlaceOfDeath,
                    d.ReportedByName,
                    d.ReportedByRelation,
                    GraveyardName = g.Name,
                    CauseOfDeath = c.Name,
                    AdditionalNotes = d.AdditionalRemarks,
                    d.DeathPrayerCenter,
                    d.DeathCertificateFilePath,
                    d.CreatedDate,
                    d.CreatedBy,
                    d.DeceasedShortCode
                };

            var totalRecords = await query.CountAsync();
            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var deceasedIds = data.Select(x => x.PersonDeceasedId).ToList();

            var healthConditions = await (
                from h in _dbContext.PersonHealthConditions
                join s in _dbContext.SetupHealthConditions on h.SetupHealthConditionId equals s.SetupHealthConditionId
                where deceasedIds.Contains(h.PersonDeceasedInfoId)
                select new { h.PersonDeceasedInfoId, s.ConditionName }).ToListAsync();

            var items = data.Select(item => new
            {
                item.PersonDeceasedId,
                item.PersonId,
                item.PersonShortCode,
                item.PersonFullName,
                item.CNIC,
                item.FamilyCode,
                item.Age,
                item.Gender,
                item.Image,
                item.DateOfDeath,
                item.TimeOfDeath,
                item.PlaceOfDeath,
                item.ReportedByName,
                item.ReportedByRelation,
                item.GraveyardName,
                item.CauseOfDeath,
                item.AdditionalNotes,
                item.DeathPrayerCenter,
                item.DeathCertificateFilePath,
                item.CreatedDate,
                item.CreatedBy,
                item.DeceasedShortCode,
                HealthConditions = healthConditions
                    .Where(h => h.PersonDeceasedInfoId == item.PersonDeceasedId)
                    .Select(h => h.ConditionName)
                    .ToList()
            }).ToList();

            return PersonServiceResult.Ok(new PaginatedResultDto<object>
            {
                Items = items.Cast<object>().ToList(),
                Page = page,
                PageSize = pageSize,
                TotalRecords = totalRecords
            });

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "DeceasedList failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> DeceasedAnalysisThisYear()
        {
            try
            {
            var currentYear = DateTime.Now.Year;

            var deceasedData = await (
                from d in _dbContext.PersonDeceasedInfos
                join p in _dbContext.PersonalInfos on d.PersonId equals p.PersonId
                join c in _dbContext.SetupCauseOfDeathTypes on d.SetupCauseOfDeathId equals c.SetupCauseOfDeathId into causeJoin
                from cause in causeJoin.DefaultIfEmpty()
                where d.DateOfDeath.Year == currentYear
                select new { Deceased = d, Person = p, Cause = cause }).ToListAsync();

            var jkIds = deceasedData.Select(x => x.Person.JKID).Distinct().ToList();

            var jkWithParents = await (
                from jk in _dbContext.Locations
                join lc in _dbContext.Locations on jk.ParentID equals lc.LocationID
                join region in _dbContext.Locations on lc.ParentID equals region.LocationID
                where jkIds.Contains(jk.LocationID)
                select new
                {
                    JKID = jk.LocationID,
                    LCName = lc.LocationName,
                    RegionName = region.LocationName,
                    JKName = jk.LocationName
                }).ToListAsync();

            var lcStats = deceasedData
                .Join(jkWithParents, d => d.Person.JKID, l => l.JKID, (d, l) => new { l.LCName })
                .GroupBy(x => x.LCName)
                .ToDictionary(g => g.Key, g => g.Count());

            var regionStats = deceasedData
                .Join(jkWithParents, d => d.Person.JKID, l => l.JKID, (d, l) => new { l.RegionName })
                .GroupBy(x => x.RegionName)
                .ToDictionary(g => g.Key, g => g.Count());

            var deceasedIds = deceasedData.Select(x => x.Deceased.PersonDeceasedId).ToList();

            var healthConditions = await (
                from h in _dbContext.PersonHealthConditions
                join s in _dbContext.SetupHealthConditions on h.SetupHealthConditionId equals s.SetupHealthConditionId
                where deceasedIds.Contains(h.PersonDeceasedInfoId)
                select new { h.PersonDeceasedInfoId, Condition = s.ConditionName }).ToListAsync();

            var graveyards = await (
                from d in _dbContext.PersonDeceasedInfos
                join g in _dbContext.Graveyards on d.GraveyardId equals g.Id
                where deceasedIds.Contains(d.PersonDeceasedId)
                select new { d.PersonDeceasedId, Graveyard = g.Name }).ToListAsync();

            var model = new
            {
                TotalDeceased = deceasedData.Count,
                MonthWiseStats = deceasedData
                    .GroupBy(d => d.Deceased.DateOfDeath.Month)
                    .Select(g => new
                    {
                        Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(g.Key),
                        Male = g.Count(d => d.Person.Gender == "Male"),
                        Female = g.Count(d => d.Person.Gender == "Female")
                    }).ToList(),
                JKWiseStats = deceasedData
                    .Join(jkWithParents, d => d.Person.JKID, l => l.JKID, (d, l) => l.JKName)
                    .GroupBy(x => x)
                    .Select(g => new { JKName = g.Key, Count = g.Count() })
                    .ToList(),
                CauseOfDeathStats = deceasedData
                    .Where(d => d.Cause != null)
                    .GroupBy(d => d.Cause!.Name)
                    .Select(g => new { Cause = g.Key, Count = g.Count() })
                    .ToList(),
                HealthConditionStats = healthConditions
                    .GroupBy(h => h.Condition)
                    .Select(g => new { Condition = g.Key, Count = g.Count() })
                    .ToList(),
                AgeGroupStats = deceasedData
                    .Select(d => new
                    {
                        Age = d.Person.DateOfBirth.HasValue
                            ? (int)((d.Deceased.DateOfDeath - d.Person.DateOfBirth.Value).TotalDays / 365.25)
                            : 0
                    })
                    .GroupBy(x =>
                        x.Age <= 5 ? "0-5" :
                        x.Age <= 18 ? "6-18" :
                        x.Age <= 40 ? "19-40" :
                        x.Age <= 60 ? "41-60" : "60+")
                    .Select(g => new
                    {
                        Group = g.Key,
                        Description =
                            g.Key == "0-5" ? "Infants & Toddlers" :
                            g.Key == "6-18" ? "Children & Teens" :
                            g.Key == "19-40" ? "Young Adults" :
                            g.Key == "41-60" ? "Middle Aged Adults" :
                            "Seniors",
                        Count = g.Count()
                    })
                    .OrderBy(g => g.Group)
                    .ToList(),
                PlaceOfDeathStats = deceasedData
                    .GroupBy(d => d.Deceased.PlaceOfDeath)
                    .Select(g => new { Place = g.Key, Count = g.Count() })
                    .ToList(),
                OtherHealthRemarks = deceasedData
                    .Where(d => !string.IsNullOrEmpty(d.Deceased.AdditionalRemarks))
                    .Select(d => d.Deceased.AdditionalRemarks)
                    .ToList(),
                GraveyardStats = graveyards
                    .GroupBy(x => x.Graveyard)
                    .Select(g => new { Graveyard = g.Key, Count = g.Count() })
                    .ToList(),
                LocalCouncilWiseStats = lcStats,
                RegionWiseStats = regionStats
            };

            return PersonServiceResult.Ok(OkResult("OK", model));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "DeceasedAnalysisThisYear failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public PersonServiceResult SaveDeceasedInfoSimple(
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
            string emailAddress)
        {
            try
            {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(deceasedFirstName)) errors.Add("Deceased first name is required.");
            if (string.IsNullOrWhiteSpace(deceasedFatherName)) errors.Add("Deceased father/husband name is required.");
            if (string.IsNullOrWhiteSpace(deceasedCNIC)) errors.Add("Deceased CNIC is required.");
            if (string.IsNullOrWhiteSpace(deceasedIdentificationType)) errors.Add("Deceased Identification Type is required.");
            if (dobDay <= 0 || dobMonth <= 0 || dobYear <= 0) errors.Add("Deceased Date of Birth is required.");
            if (string.IsNullOrWhiteSpace(gender)) errors.Add("Deceased gender is required.");
            if (relationshipTypeId <= 0) errors.Add("Relationship with deceased is required.");
            if (string.IsNullOrWhiteSpace(relativeCNIC)) errors.Add("Relative CNIC is required.");
            if (string.IsNullOrWhiteSpace(relativeIdentificationType)) errors.Add("Relative Identification Type is required.");
            if (string.IsNullOrWhiteSpace(relativeFirstName)) errors.Add("Relative first name is required.");
            if (string.IsNullOrWhiteSpace(relativeFatherName)) errors.Add("Relative father/husband name is required.");
            if (relativedobDay <= 0 || relativedobMonth <= 0 || relativedobYear <= 0) errors.Add("Relative Date of Birth is required.");
            if (string.IsNullOrWhiteSpace(relativeGender)) errors.Add("Relative gender is required.");
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length != 11) errors.Add("Phone number must be 11 digits.");
            if (string.IsNullOrWhiteSpace(emailAddress)) errors.Add("Email address is required.");
            if (string.IsNullOrWhiteSpace(completeAddress)) errors.Add("Complete address is required.");
            if (jkId == 0) errors.Add("Center is required.");

            var deceasedCNICValidation = GlobalHelper.ValidateCnicByType(deceasedIdentificationType, deceasedCNIC);
            if (!string.IsNullOrEmpty(deceasedCNICValidation))
                return PersonServiceResult.BadRequest(FailResult("Deceased " + deceasedCNICValidation));

            var relativeCNICValidation = GlobalHelper.ValidateCnicByType(relativeIdentificationType, relativeCNIC);
            if (!string.IsNullOrEmpty(relativeCNICValidation))
                return PersonServiceResult.BadRequest(FailResult("Relative " + relativeCNICValidation));

            if (!string.IsNullOrWhiteSpace(deceasedCNIC) &&
                !string.IsNullOrWhiteSpace(relativeCNIC) &&
                deceasedCNIC.Trim() == relativeCNIC.Trim())
            {
                errors.Add("Deceased CNIC and Relative CNIC cannot be the same.");
            }

            if (errors.Count > 0)
                return PersonServiceResult.BadRequest(FailResult(string.Join(", ", errors)));

            try
            {
                using var transaction = _dbContext.Database.BeginTransaction();

                DateTime deceasedDob = new(dobYear, dobMonth, dobDay);
                DateTime relativeDob = new(relativedobYear, relativedobMonth, relativedobDay);

                var relative = _personHelper.GetOrCreatePerson(
                    relativeFirstName,
                    relativeFatherName,
                    relativeLastName ?? "",
                    relativeCNIC,
                    phoneNumber,
                    emailAddress,
                    relativeGender,
                    relativeDob,
                    jkId,
                    CurrentUserName,
                    0,
                    completeAddress,
                    lat,
                    lon,
                    relativeMaritalStatus,
                    relativeIdentificationType);

                int familyId = 0;
                if (relative is { FamilyId: not null and not 0 })
                    familyId = relative.FamilyId.Value;

                var deceased = _personHelper.GetOrCreatePerson(
                    deceasedFirstName,
                    deceasedFatherName,
                    deceasedLastName ?? "",
                    deceasedCNIC,
                    phoneNumber,
                    emailAddress,
                    gender,
                    deceasedDob,
                    jkId,
                    CurrentUserName,
                    familyId,
                    completeAddress,
                    lat,
                    lon,
                    "",
                    deceasedIdentificationType);

                _personHelper.SavePersonRelation(deceased!.PersonId, relative?.PersonId, relationshipTypeId, CurrentUserName);
                transaction.Commit();

                return PersonServiceResult.Ok(OkResult("Relative record created successfully. Please proceed to fill in the deceased person's information."));
            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SaveDeceasedInfoSimple failed");
                return PersonServiceResult.Ok(FailResult(ex.Message));
            }

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SaveDeceasedInfoSimple failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }


    private string GenerateDeceasedShortCode()
    {
        lock (_deceasedCodeLock)
        {
            string today = DateTime.Now.ToString("yyMMdd");

            int nextNumber = _dbContext.PersonDeceasedInfos
                .Where(d => d.DeceasedShortCode.StartsWith($"D-{today}"))
                .Select(d => d.DeceasedShortCode)
                .AsEnumerable()
                .Select(code =>
                {
                    var parts = code.Split('-');
                    return parts.Length == 3 && int.TryParse(parts[2], out var num) ? num : 0;
                })
                .DefaultIfEmpty()
                .Max() + 1;

            return $"D-{today}-{nextNumber:D4}";
        }
    }
}



