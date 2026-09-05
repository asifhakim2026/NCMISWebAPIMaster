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

public class PersonService : PersonServiceBase, IPersonService
{
    private readonly PersonHelper _personHelper;

    public PersonService(NcmisDbContext dbContext, PersonHelper personHelper, IHttpContextAccessor httpContextAccessor, ILogger<PersonService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {
        _personHelper = personHelper;
    }

    public async Task<PersonServiceResult> SearchPersonByCNIC(string cnic)
        {
            try
            {
            if (string.IsNullOrWhiteSpace(cnic))
                return PersonServiceResult.BadRequest(FailResult("Please enter a CNIC."));

            var persons = await _dbContext.PersonalInfos
                .AsNoTracking()
                .Where(p => p.CNIC == cnic && p.IsActive)
                .Select(p => new
                {
                    p.PersonId,
                    p.FirstName,
                    p.LastName,
                    p.FullName,
                    p.Surname,
                    p.PersonalGuid,
                    p.Email,
                    p.Phone
                })
                .ToListAsync();

            if (persons.Count == 0)
                return PersonServiceResult.Ok(FailResult("No records found."));

            return PersonServiceResult.Ok(OkResult("OK", new { persons }));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SearchPersonByCNIC failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SearchPersonByFamilyGUID(Guid FamilyGUID, Guid? ProjectGuid = null)
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
                where p.FamilyId == familyGroup.FamilyId
                orderby rt.SortOrder
                select new FamilyMemberDto
                {
                    PersonId = p.PersonId,
                    PersonName = p.FullName ,
                    FirstName=p.FirstName,
                    LastName=p.LastName,
                   
                    CNIC = p.CNIC,
                    IdentificationType = p.IdentificationType,
                    Gender = p.Gender,
                    DateOfBirth = p.DateOfBirth,
                    RelationshipRole = p.PersonId == familyGroup.HeadPersonId ? "Head of Family" : "Family Member",
                    FamilyGroupCode = familyGroup.FamilyGroupCode,
                    FamilyCreatedDate = familyGroup.CreatedDate,
                    RelatedTo = p.PersonId == familyGroup.HeadPersonId ? "Self" : "Family Member",
                    RelationshipName = rt.Name,
                    RelationshipTypeId = rt.RelationshipTypeId,
                    Image = p.ImagePath ?? "/img/noimage.png",
                    Email = p.Email ?? "NA",
                    Phone = p.Phone ?? "NA",
                    MaritalStatus = p.MaritalStatus ?? "NA",
                    IsDeceased = p.IsDeceased,
                    DeceasedDate = p.DeceasedDate,
                    CNICIssueDate = p.CNICIssueDate,
                    CNICExpiryDate = p.CNICExpiryDate,
                    CNICExpiryStatus = p.CNICExpiryDate != null
                        ? (p.CNICExpiryDate < DateTime.Now ? "Expired" : "")
                        : "",
                    PersonCode = p.PersonCode,
                    FamilyCode = fg.FamilyGroupCode,
                    Address = _dbContext.PersonAddress
                        .Where(u => u.PersonId == p.PersonId && u.IsActive && u.AddressType == "Current")
                        .Select(a => a.AddressLine1)
                        .FirstOrDefault(),
                    Region = rc.LocationName,
                    LocalCouncil = lc.LocationName,
                    JK = jk.LocationName,
                    JKID = jk.LocationID,
                    JKShortCode = jk.ShortCode,
                    CreatedDate = p.CreatedDate,
                    CreatedBy = p.CreatedBy,
                    UpdateDate = p.UpdateDate,
                    UpdatedBy = p.UpdatedBy,
                    ApplicationStatus = "",
                    SubstanceAbuse = p.SubstanceAbuse,
                    Disabilities = p.Disabilities,
                    EducationStatus = p.EducationStatus,
                    YouthEducationStatus = p.YouthEducationStatus,
                    EmploymentStatus = p.EmploymentStatus,
                    DisabilityStatus = p.DisabilityStatus,
                    SubstanceAbuseStatus = p.SubstanceAbuseStatus
                }).ToListAsync();

            if (ProjectGuid != null)
            {
                foreach (var person in persons)
                {
                    var applicationInfo = _personHelper.CheckIfPersonHasPendingApplication(person.PersonId, ProjectGuid);
                    if (applicationInfo.hasPendingApplication)
                        person.ApplicationStatus = "Applicant";
                }
            }

            foreach (var person in persons)
                person.PersonProjectEnrollments = _personHelper.GetPersonProjectEnrollments(person.PersonId);

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SearchPersonByFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> familysummary(Guid FamilyGUID)
        {
            try
            {
            var today = DateTime.Today;

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
                where p.FamilyId == familyGroup.FamilyId && rt.Name.ToUpper() != "UN ASSIGNED"
                orderby rt.SortOrder
                select p).ToListAsync();

            var alivePersons = persons.Where(p => !p.IsDeceased).ToList();
            var deceasedPersons = persons.Where(p => p.IsDeceased).ToList();

            int surveyCount = await _dbContext.PersonSurveyMasters
                .Where(s => (s.FamilyId == familyGroup.FamilyId || s.PersonId == familyGroup.HeadPersonId)
                            && s.SurveyType == "HouseHold")
                .CountAsync();

            int IncomeAndExpensesurveyCount = await _dbContext.PersonSurveyMasters
                .Where(s => (s.FamilyId == familyGroup.FamilyId || s.PersonId == familyGroup.HeadPersonId)
                            && s.SurveyType == "IncomeExpense")
                .CountAsync();

            int AddtionalSurveyCount = await _dbContext.PersonSurveyMasters
                .Where(s => (s.PersonId == familyGroup.HeadPersonId || s.FamilyId == familyGroup.FamilyId)
                            && s.SurveyType == "SupportSurvey")
                .CountAsync();

            int FamilyDataVerificationCount = await _dbContext.FamilyVerificationRecords
                .Where(v => v.FamilyId == familyGroup.FamilyId)
                .CountAsync();

            DateTime? lastVerificationDate = await _dbContext.FamilyVerificationRecords
                .Where(v => v.FamilyId == familyGroup.FamilyId)
                .OrderByDescending(v => v.VerifiedOn)
                .Select(v => (DateTime?)v.VerifiedOn)
                .FirstOrDefaultAsync();

            decimal TotalActiveLoanAmount = await _dbContext.PersonLoans
                .Where(l => l.FamilyId == familyGroup.FamilyId && l.IsActive && l.Isgoing)
                .SumAsync(l => (decimal?)l.TotalPayable) ?? 0;

            decimal loanrepaymentmonthly = await _dbContext.PersonLoans
                .Where(l => l.FamilyId == familyGroup.FamilyId && l.IsActive && l.Isgoing)
                .SumAsync(l => (decimal?)l.MonthlyInstallment) ?? 0;

            decimal TotalInvestment = await _dbContext.PersonInvestments
                .Where(l => l.FamilyId == familyGroup.FamilyId && l.IsActive)
                .SumAsync(l => (decimal?)l.AmountInvested) ?? 0;

            decimal totalROIonInvestment = await _dbContext.PersonInvestments
                .Where(l => l.FamilyId == familyGroup.FamilyId && l.IsActive)
                .SumAsync(l => (decimal?)l.MonthlyReturn) ?? 0;

            decimal totalincomefromjob = await _dbContext.PersonWorkExperiences
                .Where(l => l.FamilyId == familyGroup.FamilyId && l.IsActive)
                .SumAsync(l => (decimal?)l.IncomePerMonth) ?? 0;

            var incomeParentId = await _dbContext.GeneralSetups
                .Where(g => g.ParentId == 0 && g.Type == "Income")
                .Select(g => g.Id)
                .FirstAsync();

            var expenseParentId = await _dbContext.GeneralSetups
                .Where(g => g.ParentId == 0 && g.Type == "Expense")
                .Select(g => g.Id)
                .FirstAsync();

            var optionMap = await _dbContext.GeneralSetups
                .Where(g => g.IsActive)
                .Select(g => new { g.Id, g.ParentId })
                .ToListAsync();

            var latestIncomeExpenseSurveyId = await _dbContext.PersonSurveyMasters
                .Where(s => (s.FamilyId == familyGroup.FamilyId || s.PersonId == familyGroup.HeadPersonId)
                            && s.SurveyType == "IncomeExpense" && s.IsActive)
                .OrderByDescending(s => s.PersonSurveyMasterId)
                .Select(s => s.PersonSurveyMasterId)
                .FirstOrDefaultAsync();

            var responses = await _dbContext.PersonHouseHoldResponses
                .Where(r => r.PersonSurveyMasterId == latestIncomeExpenseSurveyId && r.AnswerText != null)
                .ToListAsync();

            decimal totalIncome = responses
                .Where(r => decimal.TryParse(r.AnswerText, out _) &&
                            optionMap.Any(o => o.Id == r.OptionId && o.ParentId == incomeParentId))
                .Sum(r => decimal.Parse(r.AnswerText!));

            decimal totalExpense = responses
                .Where(r => decimal.TryParse(r.AnswerText, out _) &&
                            optionMap.Any(o => o.Id == r.OptionId && o.ParentId == expenseParentId))
                .Sum(r => decimal.Parse(r.AnswerText!));

            var personIds = alivePersons.Select(p => p.PersonId).ToList();

            var ongoingEducationIds = await _dbContext.PersonEducationDetails
                .Where(e => personIds.Contains(e.PersonId) && e.Isongoing && e.IsActive)
                .Select(e => e.EducationId)
                .ToListAsync();

            decimal ongoingEducationExpense = await _dbContext.PersonEducationFundingSources
                .Where(f => ongoingEducationIds.Contains(f.EducationId))
                .SumAsync(f => (decimal?)f.MonthlyAmount) ?? 0;

            var latestSupportSurveyId = await _dbContext.PersonSurveyMasters
                .Where(s => (s.FamilyId == familyGroup.FamilyId || s.PersonId == familyGroup.HeadPersonId)
                            && s.SurveyType == "SupportSurvey" && s.IsActive)
                .OrderByDescending(s => s.PersonSurveyMasterId)
                .Select(s => s.PersonSurveyMasterId)
                .FirstOrDefaultAsync();

            var supportResponses = await _dbContext.PersonHouseHoldResponses
                .Where(r => r.PersonSurveyMasterId == latestSupportSurveyId && !string.IsNullOrWhiteSpace(r.AnswerText))
                .ToListAsync();

            decimal additionalSurveyIncome = supportResponses
                .Sum(r => decimal.TryParse(r.AnswerText, out var amount) ? amount : 0m);

            static int CalcAge(DateTime dob, DateTime today) =>
                today.Year - dob.Year - (today.DayOfYear < dob.DayOfYear ? 1 : 0);

            var summary = new FamilyBasicSummaryDto
            {
                FamilyCode = familyGroup.FamilyGroupCode,
                TotalMembers = alivePersons.Count,
                DeceasedCount = deceasedPersons.Count,
                MaleCount = alivePersons.Count(p => p.Gender == "Male"),
                FemaleCount = alivePersons.Count(p => p.Gender == "Female"),
                Under18Count = alivePersons.Count(p =>
                    p.DateOfBirth.HasValue &&
                    p.DateOfBirth.Value != new DateTime(1900, 1, 1) &&
                    CalcAge(p.DateOfBirth.Value, today) < 18),
                AdultCount = alivePersons.Count(p =>
                    p.DateOfBirth.HasValue &&
                    p.DateOfBirth.Value != new DateTime(1900, 1, 1) &&
                    CalcAge(p.DateOfBirth.Value, today) is >= 18 and < 60),
                SeniorCitizenCount = alivePersons.Count(p =>
                    p.DateOfBirth.HasValue &&
                    p.DateOfBirth.Value != new DateTime(1900, 1, 1) &&
                    CalcAge(p.DateOfBirth.Value, today) >= 60),
                UnknownAgeCount = alivePersons.Count(p =>
                    p.DateOfBirth.HasValue && p.DateOfBirth.Value == new DateTime(1900, 1, 1)),
                EmployedAdults = alivePersons.Count(p =>
                    p.EmploymentStatus != null &&
                    (p.EmploymentStatus == "Paid Employed" || p.EmploymentStatus == "Self Employed" ||
                     p.EmploymentStatus == "Unpaid Family Worker")),
                StudentCount = alivePersons.Count(p =>
                    p.EducationStatus != null && p.EducationStatus.Contains("Enrolled")),
                UnemployedCount = alivePersons.Count(p =>
                    p.EmploymentStatus != null && p.EmploymentStatus == "Unemployed"),
                HouseHoldSurveyCount = surveyCount,
                IncomeandExpenseSurveyCount = IncomeAndExpensesurveyCount,
                AddtionalSupportSurveyCount = AddtionalSurveyCount,
                FamilyDataVerificationCount = FamilyDataVerificationCount,
                FamilyDataVerificationDate = lastVerificationDate,
                TotalActiveLoanAmount = TotalActiveLoanAmount,
                TotalLoanRepayment = loanrepaymentmonthly,
                TotalInvestment = TotalInvestment,
                TotalOtherIncome = totalIncome,
                TotalIncome = totalincomefromjob,
                TotalExpense = totalExpense,
                TotalROIOnInvestment = totalROIonInvestment,
                OngoingEducationExpense = ongoingEducationExpense,
                AddtionalSupportIncome = additionalSurveyIncome
            };

            return PersonServiceResult.Ok(summary);

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "familysummary failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> personalinformation(Guid FamilyGUID, Guid? ProjectGuid = null)
        {
            try
            {
            var familyGroup = await _dbContext.FamilyGroups
                .FirstOrDefaultAsync(fg => fg.FamilyGroupGuid == FamilyGUID);

            if (familyGroup == null)
                return PersonServiceResult.Ok(FailResult("Family group not found"));

            var headPerson = await _dbContext.PersonalInfos
                .FirstOrDefaultAsync(p => p.PersonId == familyGroup.HeadPersonId);

            string headFullName = headPerson != null
                ? $"{headPerson.FirstName} {headPerson.LastName ?? ""}".Trim()
                : "Head of Family";

            var relationships = await _dbContext.RelationshipTypes
                .AsNoTracking()
                .OrderBy(r => r.SortOrder)
                .ToListAsync();

            var persons = (
                from fg in _dbContext.FamilyGroups
                join p in _dbContext.PersonalInfos on fg.FamilyId equals p.FamilyId
                join pf in _dbContext.PersonFamilies on p.PersonId equals pf.PersonId
                join rt in _dbContext.RelationshipTypes on pf.RelationshipTypeId equals rt.RelationshipTypeId
                join jk in _dbContext.Locations on p.JKID equals jk.LocationID
                join lc in _dbContext.Locations on jk.ParentID equals lc.LocationID
                join rc in _dbContext.Locations on lc.ParentID equals rc.LocationID
                where p.FamilyId == familyGroup.FamilyId && p.IsActive
                orderby rt.SortOrder
                select new { p, fg, rt, jk, lc, rc }
            )
            .AsEnumerable()
            .GroupBy(x => x.p.PersonId)
            .Select(g => g.First())
            .Select(x => new FamilyMemberDto
            {
                PersonGuid = x.p.PersonalGuid,
                PersonId = x.p.PersonId,
                PersonName = x.p.FirstName + " " + (x.p.LastName ?? ""),
                FirstName = x.p.FirstName,
                Surname = x.p.Surname,
                LastName = x.p.LastName,
                CNIC = x.p.CNIC,
                CNICFront = x.p.CNICFront ?? "/img/noimage.png",
                CNICBackView = x.p.CNICBack ?? "/img/noimage.png",
                IdentificationType = x.p.IdentificationType,
                Gender = x.p.Gender,
                DateOfBirth = x.p.DateOfBirth,
                RelationshipRole = x.p.PersonId == familyGroup.HeadPersonId ? "Head of Family" : "Family Member",
                FamilyGroupCode = x.fg.FamilyGroupCode,
                FamilyCreatedDate = x.fg.CreatedDate,
                RelatedTo = x.p.PersonId == familyGroup.HeadPersonId ? "Self" : "Family Member",
                RelationshipName = x.rt.Name,
                RelationshipTypeId = x.rt.RelationshipTypeId,
                Image = x.p.ImagePath ?? "/img/noimage.png",
                Email = x.p.Email,
                Phone = x.p.Phone,
                MaritalStatus = x.p.MaritalStatus ?? "NA",
                IsDeceased = x.p.IsDeceased,
                DeceasedDate = x.p.DeceasedDate,
                CNICIssueDate = x.p.CNICIssueDate,
                CNICExpiryDate = x.p.CNICExpiryDate,
                CNICExpiryStatus = x.p.CNICExpiryDate != null
                    ? (x.p.CNICExpiryDate < DateTime.Now ? "Expired" : "")
                    : "",
                PersonCode = x.p.PersonCode,
                FamilyCode = x.fg.FamilyGroupCode,
                Address = _dbContext.PersonAddress
                    .Where(u => u.PersonId == x.p.PersonId && u.IsActive && u.AddressType == "Current")
                    .Select(a => a.AddressLine1)
                    .FirstOrDefault(),
                Region = x.rc.LocationName,
                LocalCouncil = x.lc.LocationName,
                JK = x.jk.LocationName,
                JKID = x.jk.LocationID,
                JKShortCode = x.jk.ShortCode,
                CreatedDate = x.p.CreatedDate,
                CreatedBy = x.p.CreatedBy,
                UpdateDate = x.p.UpdateDate,
                UpdatedBy = x.p.UpdatedBy,
                ApplicationStatus = "",
                Headofthefamilyname = headFullName,
                EducationStatus = x.p.EducationStatus,
                YouthEducationStatus = x.p.YouthEducationStatus,
                EmploymentStatus = x.p.EmploymentStatus,
                SubstanceAbuse = x.p.SubstanceAbuse,
                Disabilities = x.p.Disabilities,
                SubstanceAbuseStatus = x.p.SubstanceAbuseStatus,
                DisabilityStatus = x.p.DisabilityStatus,
                BTSCode = x.p.BTSReferenceCode
            }).ToList();

            if (ProjectGuid != null)
            {
                foreach (var person in persons)
                {
                    var applicationInfo = _personHelper.CheckIfPersonHasPendingApplication(person.PersonId, ProjectGuid);
                    if (applicationInfo.hasPendingApplication)
                        person.ApplicationStatus = "Applicant";
                    person.PersonProjectEnrollments = _personHelper.GetPersonProjectEnrollments(person.PersonId);
                }
            }

            return PersonServiceResult.Ok(OkResult("OK", new { members = persons, relationships }));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "personalinformation failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> UpdatePersonalInfo(
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
            string? btscode)
        {
            try
            {
            if (string.IsNullOrWhiteSpace(firstName) || firstName.Length < 2)
                return PersonServiceResult.BadRequest(FailResult("First Name is required and must be at least 2 characters."));

            if (string.IsNullOrWhiteSpace(lastName) || lastName.Length < 2)
                return PersonServiceResult.BadRequest(FailResult("Father/Husband Name is required and must be at least 2 characters."));

            if (string.IsNullOrWhiteSpace(cnicidentificationtype))
                return PersonServiceResult.BadRequest(FailResult("Identification type is required."));

            if (string.IsNullOrWhiteSpace(cnic))
                return PersonServiceResult.BadRequest(FailResult("Identification number is required."));

            var errorMessage = GlobalHelper.ValidateCnicByType(cnicidentificationtype, cnic);
            if (!string.IsNullOrEmpty(errorMessage))
                return PersonServiceResult.BadRequest(FailResult(errorMessage));

            if (!dateOfBirth.HasValue)
                return PersonServiceResult.BadRequest(FailResult("Date of Birth is required."));

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phone))
                return PersonServiceResult.BadRequest(FailResult("Either Email or Phone number is required."));

            if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
                return PersonServiceResult.BadRequest(FailResult("Email must be valid."));

            if (!string.IsNullOrWhiteSpace(phone) && phone.Length < 10)
                return PersonServiceResult.BadRequest(FailResult("Phone number must be at least 10 digits if provided."));

            if (string.IsNullOrWhiteSpace(gender))
                return PersonServiceResult.BadRequest(FailResult("Gender is required."));

            if (string.IsNullOrWhiteSpace(maritalStatus))
                return PersonServiceResult.BadRequest(FailResult("Marital Status is required."));

            if (jkid <= 0)
                return PersonServiceResult.BadRequest(FailResult("JK ID must be selected."));

            if (relationshipid <= 0)
                return PersonServiceResult.BadRequest(FailResult("Relationship must be selected."));

            if (string.IsNullOrWhiteSpace(educationstatus))
                return PersonServiceResult.BadRequest(FailResult("Education status is required."));

            if (string.IsNullOrWhiteSpace(youtheducationstatus))
                return PersonServiceResult.BadRequest(FailResult("Youth education status is required."));

            if (string.IsNullOrWhiteSpace(workemploymentstatus))
                return PersonServiceResult.BadRequest(FailResult("Work/Employment status is required."));

            if (isDeceased && !deceasedDate.HasValue)
                return PersonServiceResult.BadRequest(FailResult("Deceased Date is required if person is marked as deceased."));

            if (string.IsNullOrWhiteSpace(disabilitystatus))
                return PersonServiceResult.BadRequest(FailResult("Disability status is required."));

            if (string.IsNullOrWhiteSpace(substanceabusestatus))
                return PersonServiceResult.BadRequest(FailResult("Substance abuse status is required."));

            var person = await _dbContext.PersonalInfos.FindAsync(personId);
            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person not found."));

            var existingPerson = await _dbContext.PersonalInfos.FirstOrDefaultAsync(p => p.CNIC == cnic && p.PersonId!=personId);
            if (existingPerson != null)
            {
                string fullName = $"{existingPerson.FirstName} {existingPerson.Surname} {existingPerson.LastName}".Trim();
                return PersonServiceResult.Ok(FailResult("This record cannot be inserted as CNIC is already registered.", new
                {
                    personCode = existingPerson.PersonCode,
                    fullName
                }));
            }


            if (image != null) person.ImagePath = image;
            if (cnicfront != null) person.CNICFront = cnicfront;
            if (cnicback != null) person.CNICBack = cnicback;

            person.JKID = jkid;
            person.FirstName = firstName;
            person.LastName = lastName;
            person.Surname = surname;
            person.IdentificationType = cnicidentificationtype;
            person.CNIC = cnic;
            person.CNICIssueDate = cnicIssueDate;
            person.CNICExpiryDate = cnicExpiryDate;
            person.Email = email;
            person.Phone = phone;
            person.Gender = gender;
            person.MaritalStatus = maritalStatus;
            person.DateOfBirth = dateOfBirth;
            person.IsDeceased = isDeceased;
            person.DeceasedDate = deceasedDate;
            person.EducationStatus = educationstatus;
            person.YouthEducationStatus = youtheducationstatus;
            person.EmploymentStatus = workemploymentstatus;
            person.DisabilityStatus = disabilitystatus;
            person.Disabilities = disabilities;
            person.SubstanceAbuseStatus = substanceabusestatus;
            person.SubstanceAbuse = substanceabuse;
            person.UpdateDate = DateTime.Now;
            person.UpdatedBy = CurrentUserName;
            person.BTSReferenceCode = btscode;

            await _dbContext.SaveChangesAsync();

            if (person.FamilyId is > 0)
            {
                var familyGroup = await _dbContext.FamilyGroups
                    .FirstOrDefaultAsync(f => f.FamilyId == person.FamilyId);

                if (familyGroup != null)
                {
                    var currentHeadPersonId = familyGroup.HeadPersonId;

                    var existingRelation = await _dbContext.PersonFamilies
                        .FirstOrDefaultAsync(r => r.PersonId == personId && r.RelatedPersonId == currentHeadPersonId);

                    if (existingRelation != null)
                    {
                        existingRelation.RelationshipTypeId = relationshipid;
                        existingRelation.UpdatedDate = DateTime.Now;
                        existingRelation.UpdatedBy = CurrentUserName;
                    }
                    else
                    {
                        _dbContext.PersonFamilies.Add(new PersonFamily
                        {
                            PersonId = personId,
                            RelatedPersonId = (int)currentHeadPersonId!,
                            RelationshipTypeId = relationshipid,
                            CreatedDate = DateTime.Now,
                            CreatedBy = CurrentUserName
                        });
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }

            return PersonServiceResult.Ok(OkResult("Personal Info updated successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "UpdatePersonalInfo failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> MakeHeadOfFamily(int personId)
        {
            try
            {
            var person = await _dbContext.PersonalInfos.FindAsync(personId);

            if (person == null || person.FamilyId == null)
                return PersonServiceResult.Ok(FailResult("Person not found or not linked to a family."));

            var familyGroup = await _dbContext.FamilyGroups
                .FirstOrDefaultAsync(f => f.FamilyId == person.FamilyId);

            if (familyGroup == null)
                return PersonServiceResult.Ok(FailResult("Family group not found."));

            var oldHeadId = familyGroup.HeadPersonId;

            if (oldHeadId == personId)
                return PersonServiceResult.Ok(FailResult("This person is already the Head of Family."));

            familyGroup.HeadPersonId = personId;
            familyGroup.UpdatedDate = DateTime.Now;
            familyGroup.UpdatedBy = CurrentUserName;
            await _dbContext.SaveChangesAsync();

            var relationsToUpdate = await _dbContext.PersonFamilies
                .Where(r => r.RelatedPersonId == oldHeadId)
                .ToListAsync();

            foreach (var relation in relationsToUpdate)
            {
                relation.RelatedPersonId = personId;
                relation.UpdatedDate = DateTime.Now;
                relation.UpdatedBy = CurrentUserName;
            }
            await _dbContext.SaveChangesAsync();

            var oldHeadRelation = await _dbContext.PersonFamilies
                .FirstOrDefaultAsync(r => r.PersonId == oldHeadId && r.RelatedPersonId == personId);

            if (oldHeadRelation != null)
            {
                oldHeadRelation.RelationshipTypeId = 12;
                oldHeadRelation.UpdatedDate = DateTime.Now;
                oldHeadRelation.UpdatedBy = CurrentUserName;
            }

            var newHeadRelation = await _dbContext.PersonFamilies
                .FirstOrDefaultAsync(r => r.PersonId == personId && r.RelatedPersonId == personId);

            if (newHeadRelation != null)
            {
                newHeadRelation.RelationshipTypeId = 9;
                newHeadRelation.UpdatedDate = DateTime.Now;
                newHeadRelation.UpdatedBy = CurrentUserName;
            }

            await _dbContext.SaveChangesAsync();

            return PersonServiceResult.Ok(OkResult("Head of Family updated successfully. Make sure you correct all member relationships as per the new head."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MakeHeadOfFamily failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> GetPersonList(
            string? searchTerm,
            string sortColumn = "FirstName",
            bool isAscending = true,
            int page = 1)
        {
            try
            {
                const int pageSize = 10;

                var query = from person in _dbContext.PersonalInfos
                            join family in _dbContext.FamilyGroups on person.FamilyId equals family.FamilyId into familyJoin
                            join jk in _dbContext.Locations on person.JKID equals jk.LocationID
                            join lc in _dbContext.Locations on jk.ParentID equals lc.LocationID
                            join rc in _dbContext.Locations on lc.ParentID equals rc.LocationID
                            from family in familyJoin.DefaultIfEmpty()
                            select new { person, family, jk, lc, rc };

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var term = searchTerm.ToLower();
                    query = query.Where(p =>
                        p.person.FirstName.ToLower().Contains(term) ||
                        p.person.LastName.ToLower().Contains(term) ||
                        (p.person.Surname != null && p.person.Surname.ToLower().Contains(term)) ||
                        p.person.Gender.ToLower().Contains(term) ||
                        p.person.CNIC.ToLower().Contains(term) ||
                        (p.person.Email != null && p.person.Email.ToLower().Contains(term)) ||
                        (p.person.Phone != null && p.person.Phone.ToLower().Contains(term)) ||
                        p.person.PersonCode.ToLower().Contains(term));
                }

                query = sortColumn?.ToLower() switch
                {
                    "lastname" => isAscending ? query.OrderBy(p => p.person.LastName) : query.OrderByDescending(p => p.person.LastName),
                    "surname" => isAscending ? query.OrderBy(p => p.person.Surname) : query.OrderByDescending(p => p.person.Surname),
                    "gender" => isAscending ? query.OrderBy(p => p.person.Gender) : query.OrderByDescending(p => p.person.Gender),
                    "dateofbirth" => isAscending ? query.OrderBy(p => p.person.DateOfBirth) : query.OrderByDescending(p => p.person.DateOfBirth),
                    _ => isAscending ? query.OrderBy(p => p.person.FirstName) : query.OrderByDescending(p => p.person.FirstName),
                };

                int totalRecords = await query.CountAsync();

                var beneficiaries = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new FamilyMemberDto
                    {
                        PersonId = p.person.PersonId,
                        PersonGuid = p.person.PersonalGuid,
                        PersonCode = p.person.PersonCode,
                        FirstName = p.person.FirstName,
                        LastName = p.person.LastName,
                        Gender = p.person.Gender,
                        CNIC = p.person.CNIC,
                        Email = p.person.Email,
                        Phone = p.person.Phone,
                        CreatedDate = p.person.CreatedDate,
                        FamilyGuid = p.family != null ? p.family.FamilyGroupGuid : null,
                        FamilyCode = p.family != null ? p.family.FamilyGroupCode : null,
                        FamilyMemberCount = p.person.FamilyId != null
                            ? _dbContext.PersonalInfos.Count(x => x.FamilyId == p.person.FamilyId)
                            : (int?)null,
                        Address = _dbContext.PersonAddress
                            .Where(u => u.PersonId == p.person.PersonId && u.IsActive && u.AddressType == "Current")
                            .Select(a => a.AddressLine1)
                            .FirstOrDefault(),
                        Region = p.rc.LocationName,
                        LocalCouncil = p.lc.LocationName,
                        JK = p.jk.LocationName,
                        IdentificationType = p.person.IdentificationType
                    })
                    .ToListAsync();

                var result = new PaginatedResultDto<FamilyMemberDto>
                {
                    Items = beneficiaries,
                    Page = page,
                    PageSize = pageSize,
                    TotalRecords = totalRecords
                };

                return PersonServiceResult.Ok(result);
            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "GetPersonList failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
        }

    public async Task<PersonServiceResult> CreateQuickPerson(
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
            Guid familyguid)
        {
            try
            {
            var validationErrors = new List<string>();

            if (string.IsNullOrWhiteSpace(cnic)) validationErrors.Add("CNIC is required.");
            if (string.IsNullOrWhiteSpace(firstname)) validationErrors.Add("First Name is required.");
            if (string.IsNullOrWhiteSpace(lastname)) validationErrors.Add("Last Name is required.");
            if (string.IsNullOrWhiteSpace(gender)) validationErrors.Add("Gender is required.");
            if (jkid == 0) validationErrors.Add("Please select a center (JKID is required).");
            if (string.IsNullOrWhiteSpace(maritalstatus)) validationErrors.Add("Marital Status is required.");
            if (!dob.HasValue) validationErrors.Add("Date of Birth is required.");

            if (string.IsNullOrWhiteSpace(identificationtype))
                return PersonServiceResult.BadRequest(FailResult("Identification type is required."));

            if (string.IsNullOrWhiteSpace(cnic))
                return PersonServiceResult.BadRequest(FailResult("Identification number is required."));

            var cnicError = GlobalHelper.ValidateCnicByType(identificationtype, cnic);
            if (!string.IsNullOrEmpty(cnicError))
                return PersonServiceResult.BadRequest(FailResult(cnicError));

            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
                validationErrors.Add("Either Phone Number or Email Address is required.");

            if (!string.IsNullOrWhiteSpace(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\d{11}$"))
                validationErrors.Add("Phone number must be exactly 11 digits.");

            if (!string.IsNullOrWhiteSpace(email) && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))
                validationErrors.Add("Invalid Email Address format.");

            if (validationErrors.Count > 0)
                return PersonServiceResult.BadRequest(FailResult(string.Join(", ", validationErrors)));


            switch (identificationtype.Trim().ToUpper())
            {
                case "CNIC":
                case "SNIC":
                case "NICOP":
                    if (cnic.Length != 13 || !cnic.All(char.IsDigit))
                    {
                        validationErrors.Add($"{identificationtype} must be exactly 13 numeric digits.");
                        return PersonServiceResult.BadRequest(FailResult(string.Join(", ", validationErrors)));
                    }
                    break;

                case "BFORM":
                    if (cnic.Length < 11 || cnic.Length > 13 || !cnic.All(char.IsDigit))
                    {
                        validationErrors.Add("B-Form must be between 11 to 13 numeric digits.");
                        return PersonServiceResult.BadRequest(FailResult(string.Join(", ", validationErrors)));
                    }
                    break;

                case "POC":
                case "FRC":
                    // Allow alphanumeric, typically 10-16 characters for foreign documents
                    if (cnic.Length < 10 || cnic.Length > 16)
                    { validationErrors.Add($"{identificationtype} must be between 10 to 16 characters.");
                    return PersonServiceResult.BadRequest(FailResult(string.Join(", ", validationErrors)));
                    }
        
                    break;
                default:
                    validationErrors.Add("Unsupported identification type selected.");
                    break;
            }


            var existingPerson = await _dbContext.PersonalInfos.FirstOrDefaultAsync(p => p.CNIC == cnic);
            if (existingPerson != null)
            {
                string fullName = $"{existingPerson.FirstName} {existingPerson.Surname} {existingPerson.LastName}".Trim();
                return PersonServiceResult.Ok(FailResult("This record cannot be inserted as CNIC is already registered.", new
                {
                    personCode = existingPerson.PersonCode,
                    fullName
                }));
            }

            var familyGroup = await _dbContext.FamilyGroups.FirstOrDefaultAsync(f => f.FamilyGroupGuid == familyguid);
            if (familyGroup == null)
                return PersonServiceResult.Ok(FailResult("Family group not found."));

            string? address = null;
            string? lat = null;
            string? lon = null;

            var personAddress = await _dbContext.PersonAddress.FirstOrDefaultAsync(p => p.PersonId == familyGroup.HeadPersonId);
            if (personAddress != null)
            {
                address = personAddress.AddressLine1;
                lat = personAddress.Latitude.ToString();
                lon = personAddress.Longitude.ToString();
            }

            var person = _personHelper.GetOrCreatePerson(
                firstName: firstname,
                fathername: lastname ?? "",
                lastName: surname ?? "",
                cnic: cnic,
                phone: phone,
                email: email,
                gender: gender,
                dob: dob,
                jkId: jkid,
                createdBy: CurrentUserName,
                familyGroupId: familyGroup.FamilyId,
                completeaddress: address,
                lat: lat,
                lon: lon,
                maritalstatus: maritalstatus,
                identificationtype: identificationtype);

            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person could not be created."));

            _personHelper.SavePersonRelation(person.PersonId, familyGroup.HeadPersonId, 12, CurrentUserName);

            return PersonServiceResult.Ok(OkResult("Person created successfully.", new { personId = person.PersonId }));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "CreateQuickPerson failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> ImportHouseholdData(string? rawText, string? rawTextQuery = null)
        {
            try
            {
            rawText ??= rawTextQuery;
            if (string.IsNullOrWhiteSpace(rawText))
                return PersonServiceResult.BadRequest(FailResult("No data received."));

            int inserted = 0;
            int skipped = 0;

            try
            {
                var lines = rawText
                    .Replace("\r\n", "\n")
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.TrimEnd())
                    .ToList();

                if (lines.Count < 2)
                    return PersonServiceResult.BadRequest(FailResult("No rows found."));

                var headers = lines[0]
                    .Split('\t')
                    .Select(h => h.Trim())
                    .ToList();

                int columnCount = headers.Count;

                for (int i = 1; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        skipped++;
                        continue;
                    }

                    var cols = line.Split('\t');
                    string cnic = GetSafe(cols, headers, "CNIC #");
                    if (string.IsNullOrWhiteSpace(cnic))
                        cnic = GetSafe(cols, headers, "CNIC");

                    if (string.IsNullOrWhiteSpace(cnic))
                    {
                        skipped++;
                        continue;
                    }

                    int btsNo = ToInt(GetSafe(cols, headers, "BTS No."));
                    int? memberId = ToNullableInt(GetSafe(cols, headers, "Member ID"));
                    int? jkId = ToNullableInt(GetSafe(cols, headers, "JK"));
                    string memberName = GetSafe(cols, headers, "Household head + Members' name");
                    string relationship = GetSafe(cols, headers, "Relationship");

                    DateTime? registrationDate =
                        ToNullableDate(GetSafe(cols, headers, "registrationDate")) ??
                        ToNullableDate(GetSafe(cols, headers, "RegistrationDate"));

                    var jsonObj = new Dictionary<string, object>();
                    for (int c = 0; c < columnCount; c++)
                    {
                        string header = headers[c];
                        string value = c < cols.Length ? cols[c].Trim() : "";
                        jsonObj[header] = value;
                    }

                    string rowJson = JsonSerializer.Serialize(jsonObj);

                    _dbContext.TempHouseholdRaws.Add(new TempHouseholdRaw
                    {
                        BTSNo = btsNo,
                        RegistrationDate = registrationDate,
                        MemberId = memberId,
                        JKId = jkId,
                        CNIC = cnic,
                        Name = string.IsNullOrWhiteSpace(memberName) ? cnic : memberName,
                        Relationship = string.IsNullOrWhiteSpace(relationship) ? "Unknown" : relationship,
                        RowJson = rowJson,
                        CreatedOn = DateTime.Now
                    });
                    inserted++;
                }

                await _dbContext.SaveChangesAsync();
                return PersonServiceResult.Ok(OkResult($"Imported: {inserted}, Skipped: {skipped}"));
            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "ImportHouseholdData failed");
                return PersonServiceResult.Ok(FailResult(ex.Message));
            }

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "ImportHouseholdData failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }


    private static string GetSafe(string[] cols, List<string> headers, string headerName)
    {
        int idx = headers.FindIndex(h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
        if (idx == -1 || idx >= cols.Length) return "";
        return cols[idx]?.Trim() ?? "";
    }

    private static int ToInt(string val) => int.TryParse(val, out var n) ? n : 0;

    private static int? ToNullableInt(string val) => int.TryParse(val, out var n) ? n : null;

    private static DateTime? ToNullableDate(string val)
    {
        if (string.IsNullOrWhiteSpace(val)) return null;
        return DateTime.TryParse(val, out var d) ? d : null;
    }
}



