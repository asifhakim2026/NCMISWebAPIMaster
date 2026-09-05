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

public class PersonFinanceService : PersonServiceBase, IPersonFinanceService
{


    public PersonFinanceService(NcmisDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<PersonFinanceService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {

    }

    public async Task<PersonServiceResult> LoanByFamilyGUID(Guid FamilyGUID)
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
                    Loans = _dbContext.PersonLoans
                        .Where(l => l.PersonId == p.PersonId)
                        .OrderByDescending(l => l.IsActive)
                        .ThenByDescending(l => l.LoanDate)
                        .ToList()
                }).ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "LoanByFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SavePersonLoan(
            Guid Personguid,
            string Source,
            string LoanType,
            DateTime? LoanDate,
            decimal? InterestRate,
            decimal? LoanAmount,
            decimal? MonthlyInstallment,
            int? LoanDuration,
            int? Noofinstallmentpaid,
            string PurposeofLoan,
            bool IsDefault,
            string? ReasonofDefault,
            string UploadProofJson)
        {
            try
            {
            if (Personguid == Guid.Empty)
                return PersonServiceResult.BadRequest(FailResult("Person ID is required."));
            if (string.IsNullOrWhiteSpace(Source))
                return PersonServiceResult.BadRequest(FailResult("Loan Provider is required."));
            if (string.IsNullOrWhiteSpace(LoanType))
                return PersonServiceResult.BadRequest(FailResult("Type of Loan is required."));
            if (LoanDate == null)
                return PersonServiceResult.BadRequest(FailResult("Loan Issued Date is required."));
            if (string.IsNullOrWhiteSpace(PurposeofLoan))
                return PersonServiceResult.BadRequest(FailResult("Purpose of Loan is required."));
            if (InterestRate == null || InterestRate < 0 || InterestRate > 100)
                return PersonServiceResult.BadRequest(FailResult("Interest Rate must be between 0 and 100%."));
            if (LoanAmount == null || LoanAmount <= 0)
                return PersonServiceResult.BadRequest(FailResult("Loan Amount must be greater than 0."));
            if (MonthlyInstallment == null || MonthlyInstallment <= 0)
                return PersonServiceResult.BadRequest(FailResult("Monthly Installment must be greater than 0."));
            if (LoanDuration == null || LoanDuration <= 0)
                return PersonServiceResult.BadRequest(FailResult("Loan Term (Months) must be greater than 0."));
            if (Noofinstallmentpaid == null || Noofinstallmentpaid < 0 || Noofinstallmentpaid > LoanDuration)
                return PersonServiceResult.BadRequest(FailResult("Months Paid must be between 0 and total Loan Term."));
            if (IsDefault && string.IsNullOrWhiteSpace(ReasonofDefault))
                return PersonServiceResult.BadRequest(FailResult("Reason of Default is required if person is marked as defaulter."));
            if (string.IsNullOrWhiteSpace(UploadProofJson))
                return PersonServiceResult.BadRequest(FailResult("Proof of loan is required."));

            var person = await _dbContext.PersonalInfos.FirstOrDefaultAsync(u => u.PersonalGuid == Personguid);
            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person does not exist."));

            decimal totalPayable = LoanAmount.Value;
            if (InterestRate.Value > 0 && LoanDuration.Value > 0)
            {
                decimal years = LoanDuration.Value / 12m;
                totalPayable += LoanAmount.Value * (InterestRate.Value / 100m) * years;
            }

            var latestId = await _dbContext.PersonLoans.MaxAsync(x => (int?)x.LoanID) ?? 0;

            var loan = new PersonLoan
            {
                PersonId = person.PersonId,
                FamilyId = (int)person.FamilyId!,
                Source = Source.Trim(),
                LoanType = LoanType.Trim(),
                LoanDate = LoanDate.Value,
                InterestRate = InterestRate.Value,
                LoanAmount = LoanAmount.Value,
                MonthlyInstallment = MonthlyInstallment.Value,
                LoanDuration = LoanDuration.Value,
                Noofinstallmentpaid = Noofinstallmentpaid.Value,
                PurposeofLoan = PurposeofLoan.Trim(),
                IsDefault = IsDefault,
                ReasonofDefault = IsDefault ? ReasonofDefault?.Trim() : null,
                UploadProofJson = UploadProofJson.Trim(),
                InsertDate = DateTime.Now,
                TotalPayable = totalPayable,
                CreatedBy = CurrentUserName,
                LoanCode = "LN-" + (latestId + 1).ToString("D4"),
                IsActive = true,
                Isgoing = true
            };

            _dbContext.PersonLoans.Add(loan);
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Loan data saved successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SavePersonLoan failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> MarkLoanAsInactive(int loanId, string reason, string? description)
        {
            try
            {
            var loan = await _dbContext.PersonLoans.FindAsync(loanId);
            if (loan == null)
                return PersonServiceResult.Ok(FailResult("Loan not found."));

            loan.IsActive = false;
            loan.Isgoing = false;
            loan.ReasonForInActive = reason;
            loan.DescriptionForInActive = description?.Trim();
            loan.UpdateDate = DateTime.Now;
            loan.UpdatedBy = CurrentUserName;
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Loan marked as inactive successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MarkLoanAsInactive failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> MarkLoanAsPaid(int loanId, bool isPaidByYou, string? relation, string? description)
        {
            try
            {
            var loan = await _dbContext.PersonLoans.FindAsync(loanId);
            if (loan == null)
                return PersonServiceResult.Ok(FailResult("Loan not found."));

            loan.Isgoing = false;
            loan.IsPaidByYou = isPaidByYou;
            loan.RelationWhoHelpedToPayLoan = isPaidByYou ? null : relation;
            loan.DescriptionWhoHelpedToPayLoan = isPaidByYou ? null : description;
            loan.UpdatedBy = CurrentUserName;
            loan.UpdateDate = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Loan marked as paid."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MarkLoanAsPaid failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> BankAccountFamilyGUID(Guid FamilyGUID)
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
                    BankAccounts = _dbContext.PersonBankAccounts
                        .Where(l => l.PersonId == p.PersonId)
                        .OrderByDescending(l => l.IsActive)
                        .ThenByDescending(l => l.CreatedDate)
                        .ToList()
                }).ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "BankAccountFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SavePersonBankAccount(
            int personId,
            string accountTitle,
            string accountNumber,
            string bankType,
            string? bankName,
            string? branch,
            string? city,
            bool isOwnAccount,
            string? relationshipWithAccountHolder,
            string? reasonForUsingOthersAccount)
        {
            try
            {
            if (string.IsNullOrWhiteSpace(accountTitle))
                return PersonServiceResult.BadRequest(FailResult("Account Title is required."));
            if (string.IsNullOrWhiteSpace(accountNumber))
                return PersonServiceResult.BadRequest(FailResult("Account Number is required."));
            if (string.IsNullOrWhiteSpace(bankType))
                return PersonServiceResult.BadRequest(FailResult("Bank Type is required."));
            if (!isOwnAccount)
            {
                if (string.IsNullOrWhiteSpace(relationshipWithAccountHolder))
                    return PersonServiceResult.BadRequest(FailResult("Relationship with Account Holder is required when it is not own account."));
                if (string.IsNullOrWhiteSpace(reasonForUsingOthersAccount))
                    return PersonServiceResult.BadRequest(FailResult("Reason for using other's account is required when it is not own account."));
            }

            var newAccount = new PersonBankAccount
            {
                PersonId = personId,
                AccountTitle = accountTitle.Trim(),
                AccountNumber = accountNumber.Trim(),
                BankType = bankType,
                BankName = bankName?.Trim() ?? "",
                Branch = branch?.Trim() ?? "",
                AreaOrCity = city?.Trim() ?? "",
                IsItOwnAccount = isOwnAccount,
                RelationshipWithAccountHolder = isOwnAccount ? null : relationshipWithAccountHolder?.Trim(),
                ReasonForUsingOthersAccount = isOwnAccount ? null : reasonForUsingOthersAccount?.Trim(),
                IsActive = true,
                CreatedBy = CurrentUserName,
                CreatedDate = DateTime.Now
            };

            _dbContext.PersonBankAccounts.Add(newAccount);
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Bank account saved successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SavePersonBankAccount failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> MarkBankAccountAsInactive(int bankAccountId, string reason, string? description)
        {
            try
            {
            var account = await _dbContext.PersonBankAccounts.FindAsync(bankAccountId);
            if (account == null)
                return PersonServiceResult.Ok(FailResult("Bank account not found."));

            account.IsActive = false;
            account.UpdatedDate = DateTime.Now;
            account.UpdatedBy = CurrentUserName;
            account.ReasonForInActive = reason;
            account.DescriptionForInActive = description;
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Bank account marked as inactive."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MarkBankAccountAsInactive failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> InvestmentFamilyGUID(Guid FamilyGUID)
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
                    Investments = _dbContext.PersonInvestments
                        .Where(l => l.PersonId == p.PersonId)
                        .OrderByDescending(l => l.IsActive)
                        .ThenByDescending(l => l.CreatedDate)
                        .ToList()
                }).ToListAsync();

            return PersonServiceResult.Ok(OkResult("OK", persons));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "InvestmentFamilyGUID failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SavePersonInvestment(
            int personId,
            string investmentType,
            decimal amountInvested,
            DateTime investmentDate,
            bool isFixedTerm,
            int? fixedDurationMonths,
            bool isReturnInPercentage,
            decimal? expectedReturnValue,
            decimal? expectedReturnPercentage,
            string returnFrequency,
            string? remarks)
        {
            try
            {
            if (personId <= 0) return PersonServiceResult.BadRequest(FailResult("Invalid Person ID."));
            if (string.IsNullOrWhiteSpace(investmentType)) return PersonServiceResult.BadRequest(FailResult("Investment Type is required."));
            if (amountInvested <= 0) return PersonServiceResult.BadRequest(FailResult("Investment amount must be greater than zero."));
            if (investmentDate == default) return PersonServiceResult.BadRequest(FailResult("Valid Investment Date is required."));
            if (isFixedTerm && (!fixedDurationMonths.HasValue || fixedDurationMonths <= 0))
                return PersonServiceResult.BadRequest(FailResult("Fixed duration must be greater than 0 if investment is fixed term."));
            if (isReturnInPercentage && (!expectedReturnPercentage.HasValue || expectedReturnPercentage <= 0))
                return PersonServiceResult.BadRequest(FailResult("Expected return percentage must be greater than 0."));
            if (!isReturnInPercentage && (!expectedReturnValue.HasValue || expectedReturnValue <= 0))
                return PersonServiceResult.BadRequest(FailResult("Expected return value must be greater than 0."));
            if (string.IsNullOrWhiteSpace(returnFrequency)) return PersonServiceResult.BadRequest(FailResult("Return frequency is required."));

            var person = await _dbContext.PersonalInfos.FirstOrDefaultAsync(u => u.PersonId == personId);
            if (person == null) return PersonServiceResult.Ok(FailResult("Person not found."));

            var latestId = await _dbContext.PersonInvestments.MaxAsync(x => (int?)x.PersonInvestmentId) ?? 0;
            string investmentCode = "INV-" + (latestId + 1).ToString("D4");

            decimal rawReturn = isReturnInPercentage
                ? (amountInvested * expectedReturnPercentage!.Value) / 100
                : expectedReturnValue!.Value;

            decimal monthlyReturn = returnFrequency switch
            {
                "Daily" => rawReturn * 30,
                "Weekly" => rawReturn * 4,
                "Monthly" => rawReturn,
                "Quarterly" => rawReturn / 3,
                "Yearly" => rawReturn / 12,
                _ => 0
            };

            var newInvestment = new PersonInvestment
            {
                PersonId = personId,
                FamilyId = person.FamilyId,
                InvestmentCode = investmentCode,
                InvestmentType = investmentType.Trim(),
                AmountInvested = amountInvested,
                InvestmentDate = investmentDate,
                IsFixedTerm = isFixedTerm,
                FixedDurationMonths = fixedDurationMonths,
                IsReturnInPercentage = isReturnInPercentage,
                ExpectedReturnValue = isReturnInPercentage ? null : expectedReturnValue,
                ExpectedReturnPercentage = isReturnInPercentage ? expectedReturnPercentage : null,
                ReturnFrequency = returnFrequency,
                MonthlyReturn = monthlyReturn,
                Remarks = remarks?.Trim(),
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = CurrentUserName
            };

            _dbContext.PersonInvestments.Add(newInvestment);
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Investment saved successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SavePersonInvestment failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> MarkInvestmentAsInactive(int investmentId, string reason, string? description)
        {
            try
            {
            var investment = await _dbContext.PersonInvestments.FindAsync(investmentId);
            if (investment == null)
                return PersonServiceResult.Ok(FailResult("Investment not found."));

            investment.IsActive = false;
            investment.UpdatedDate = DateTime.Now;
            investment.UpdatedBy = CurrentUserName;
            investment.ReasonForInActive = reason;
            investment.DescriptionForInActive = description;
            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Investment marked as inactive successfully."));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "MarkInvestmentAsInactive failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

}



