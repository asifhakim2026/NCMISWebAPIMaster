using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NCMIS.Models;
using NCMISAPI.Data;
using NCMISAPI.Helpers;

namespace NCMISAPI.Services;

public class PersonFieldWorkService : PersonServiceBase, IPersonFieldWorkService
{
    public PersonFieldWorkService(NcmisDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<PersonFieldWorkService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {
    }

    public async Task<PersonServiceResult> SurveyorNotesFamilyGUID(Guid FamilyGUID)
    {
        var familyGroup = await _dbContext.FamilyGroups
            .FirstOrDefaultAsync(fg => fg.FamilyGroupGuid == FamilyGUID);

        if (familyGroup == null)
            return PersonServiceResult.Ok(FailResult("Family group not found"));

        var notes = await _dbContext.SurveyorFamilyNotes
            .Where(n => n.FamilyId == familyGroup.FamilyId)
            .OrderByDescending(n => n.InsertDate)
            .ToListAsync();

        return PersonServiceResult.Ok(OkResult("OK", new { familyId = familyGroup.FamilyId, notes }));
    }

    public async Task<PersonServiceResult> SurveyNotesFamily(
            int familyid,
            string notes,
            decimal? latitude,
            decimal? longitude,
            string? imagePath,
            string? address)
    {
        var note = new SurveyorFamilyNote
        {
            FamilyId = familyid,
            Notes = notes,
            Latitude = latitude,
            Longitude = longitude,
            Address = address,
            ImagePath = imagePath,
            CreatedBy = CurrentUserName,
            InsertDate = DateTime.Now,
            SurveyCode = "SN-" + Guid.NewGuid().ToString("N")[..8].ToUpper()
        };

        _dbContext.SurveyorFamilyNotes.Add(note);
        await _dbContext.SaveChangesAsync();
        return PersonServiceResult.Ok(OkResult("Survey notes saved!"));
    }

    public async Task<PersonServiceResult> GetSurveyorFamilyNotes(int familyId)
    {
        if (familyId <= 0)
            return PersonServiceResult.BadRequest(FailResult("Invalid family."));

        var model = await _dbContext.SurveyorFamilyNotes
            .AsNoTracking()
            .Where(x => x.FamilyId == familyId)
            .OrderByDescending(x => x.InsertDate)
            .ToListAsync();

        return PersonServiceResult.Ok(OkResult("OK", model));
    }

    public async Task<PersonServiceResult> GetFamilyVerificationSummary(Guid FamilyGUID)
    {
        var familyGroup = await _dbContext.FamilyGroups.FirstOrDefaultAsync(f => f.FamilyGroupGuid == FamilyGUID);
        if (familyGroup == null)
            return PersonServiceResult.Ok(FailResult("Family not found"));

        var persons = await (
            from p in _dbContext.PersonalInfos
            where p.FamilyId == familyGroup.FamilyId
            select new
            {
                Name = p.FirstName + " " + p.LastName,
                p.Gender,
                DateOfBirth = (p.DateOfBirth.HasValue && p.DateOfBirth != new DateTime(1900, 1, 1))
                    ? p.DateOfBirth.Value.ToString("MMM dd,yyyy")
                    : "Unknown",
                Address = _dbContext.PersonAddress.Where(a => a.PersonId == p.PersonId).Select(a => a.AddressLine1).FirstOrDefault() ?? "-"
            }).ToListAsync();

        decimal TotalLoan = await _dbContext.PersonLoans
            .Where(l => l.FamilyId == familyGroup.FamilyId && l.IsActive && l.Isgoing)
            .SumAsync(l => (decimal?)l.TotalPayable) ?? 0;

        decimal MonthlyLoanRepayment = await _dbContext.PersonLoans
            .Where(l => l.FamilyId == familyGroup.FamilyId && l.IsActive && l.Isgoing)
            .SumAsync(l => (decimal?)l.MonthlyInstallment) ?? 0;

        decimal TotalInvestment = await _dbContext.PersonInvestments
            .Where(i => i.FamilyId == familyGroup.FamilyId && i.IsActive)
            .SumAsync(i => (decimal?)i.AmountInvested) ?? 0;

        decimal ROI = await _dbContext.PersonInvestments
            .Where(i => i.FamilyId == familyGroup.FamilyId && i.IsActive)
            .SumAsync(i => (decimal?)i.MonthlyReturn) ?? 0;

        decimal WorkIncome = await _dbContext.PersonWorkExperiences
            .Where(w => w.FamilyId == familyGroup.FamilyId && w.IsActive)
            .SumAsync(w => (decimal?)w.IncomePerMonth) ?? 0;

        var incomeParentId = await _dbContext.GeneralSetups.Where(g => g.ParentId == 0 && g.Type == "Income").Select(g => g.Id).FirstAsync();
        var expenseParentId = await _dbContext.GeneralSetups.Where(g => g.ParentId == 0 && g.Type == "Expense").Select(g => g.Id).FirstAsync();

        var optionMap = await _dbContext.GeneralSetups.Where(g => g.IsActive).Select(g => new { g.Id, g.ParentId }).ToListAsync();
        var incomeExpenseIds = await _dbContext.PersonSurveyMasters
            .Where(s => (s.FamilyId == familyGroup.FamilyId || s.PersonId == familyGroup.HeadPersonId) && s.SurveyType == "IncomeExpense" && s.IsActive)
            .Select(s => s.PersonSurveyMasterId)
            .ToListAsync();

        var responses = await _dbContext.PersonHouseHoldResponses
            .Where(r => incomeExpenseIds.Contains(r.PersonSurveyMasterId) && r.AnswerText != null)
            .ToListAsync();

        decimal OtherIncome = responses
            .Where(r => decimal.TryParse(r.AnswerText, out _) &&
                        optionMap.Any(o => o.Id == r.OptionId && o.ParentId == incomeParentId))
            .Sum(r => decimal.Parse(r.AnswerText!));

        decimal TotalExpense = responses
            .Where(r => decimal.TryParse(r.AnswerText, out _) &&
                        optionMap.Any(o => o.Id == r.OptionId && o.ParentId == expenseParentId))
            .Sum(r => decimal.Parse(r.AnswerText!));

        var model = new
        {
            FamilyId = familyGroup.FamilyId,
            FamilyCode = familyGroup.FamilyGroupCode,
            Members = persons,
            TotalIncome = WorkIncome,
            TotalOtherIncome = OtherIncome,
            TotalExpense,
            TotalActiveLoan = TotalLoan,
            LoanRepayment = MonthlyLoanRepayment,
            TotalInvestment,
            ROIOnInvestment = ROI
        };

        return PersonServiceResult.Ok(OkResult("OK", model));
    }

    public async Task<PersonServiceResult> SaveFamilyVerification(
            string? SignatureBase64,
            int FamilyId,
            string SignedBy,
            string VerifiedDataJson)
    {
        var familyGroup = await _dbContext.FamilyGroups.FirstOrDefaultAsync(f => f.FamilyId == FamilyId);
        int? headPersonId = familyGroup?.HeadPersonId;

        var record = new FamilyVerificationRecord
        {
            FamilyId = FamilyId,
            PersonId = headPersonId,
            SignedBy = SignedBy,
            SignatureImagePath = SignatureBase64 ?? "",
            VerifiedDataJson = VerifiedDataJson,
            VerifiedOn = DateTime.Now,
            CreatedBy = CurrentUserName
        };

        _dbContext.FamilyVerificationRecords.Add(record);
        await _dbContext.SaveChangesAsync();
        return PersonServiceResult.Ok(OkResult("Verification saved in database."));
    }

    public async Task<PersonServiceResult> ViewFamilyVerification(Guid FamilyGUID)
    {
        var family = await _dbContext.FamilyGroups
            .FirstOrDefaultAsync(u => u.FamilyGroupGuid == FamilyGUID);

        if (family == null)
            return PersonServiceResult.Ok(FailResult("Family group not found."));

        var records = await _dbContext.FamilyVerificationRecords
            .Where(v => v.FamilyId == family.FamilyId)
            .OrderByDescending(v => v.VerifiedOn)
            .ToListAsync();

        if (records.Count == 0)
            return PersonServiceResult.Ok(FailResult("No verification records found."));

        var verifiedList = records.Select(record => new
        {
            record.SignedBy,
            record.VerifiedOn,
            SignatureBase64 = record.SignatureImagePath,
            Data = string.IsNullOrWhiteSpace(record.VerifiedDataJson)
                ? (object?)null
                : JsonSerializer.Deserialize<object>(record.VerifiedDataJson),
            Username = record.CreatedBy
        }).ToList();

        return PersonServiceResult.Ok(OkResult("OK", verifiedList));
    }
}
