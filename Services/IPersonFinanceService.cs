using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonFinanceService
{
    Task<PersonServiceResult> LoanByFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SavePersonLoan(
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
            string UploadProofJson);

    Task<PersonServiceResult> MarkLoanAsInactive(int loanId, string reason, string? description);

    Task<PersonServiceResult> MarkLoanAsPaid(int loanId, bool isPaidByYou, string? relation, string? description);

    Task<PersonServiceResult> BankAccountFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SavePersonBankAccount(
            int personId,
            string accountTitle,
            string accountNumber,
            string bankType,
            string? bankName,
            string? branch,
            string? city,
            bool isOwnAccount,
            string? relationshipWithAccountHolder,
            string? reasonForUsingOthersAccount);

    Task<PersonServiceResult> MarkBankAccountAsInactive(int bankAccountId, string reason, string? description);

    Task<PersonServiceResult> InvestmentFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SavePersonInvestment(
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
            string? remarks);

    Task<PersonServiceResult> MarkInvestmentAsInactive(int investmentId, string reason, string? description);

}
