using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NCMISAPI.Services;

namespace NCMISAPI.Controllers;

/// <summary>
/// PersonFinanceController - HTTP layer; routes preserved under api/Person.
/// Unhandled exceptions → ExceptionHandlingMiddleware → ErrorLogHelper.
/// </summary>
[Authorize]
[Route("api/Person")]
[ApiController]
public class PersonFinanceController : ApiControllerBase
{
    private readonly IPersonFinanceService _financeService;

    public PersonFinanceController(IPersonFinanceService financeService)
    {
        _financeService = financeService;
    }

    [HttpGet("LoanByFamilyGUID")]
    public async Task<IActionResult> LoanByFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _financeService.LoanByFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SavePersonLoan")]
    public async Task<IActionResult> SavePersonLoan([FromForm] Guid Personguid, [FromForm] string Source, [FromForm] string LoanType, [FromForm] DateTime? LoanDate, [FromForm] decimal? InterestRate, [FromForm] decimal? LoanAmount, [FromForm] decimal? MonthlyInstallment, [FromForm] int? LoanDuration, [FromForm] int? Noofinstallmentpaid, [FromForm] string PurposeofLoan, [FromForm] bool IsDefault, [FromForm] string? ReasonofDefault, [FromForm] string UploadProofJson)
    {
        var result = await _financeService.SavePersonLoan(Personguid, Source, LoanType, LoanDate, InterestRate, LoanAmount, MonthlyInstallment, LoanDuration, Noofinstallmentpaid, PurposeofLoan, IsDefault, ReasonofDefault, UploadProofJson);
        return FromService(result);
    }

    [HttpPost("MarkLoanAsInactive")]
    public async Task<IActionResult> MarkLoanAsInactive([FromForm] int loanId, [FromForm] string reason, [FromForm] string? description)
    {
        var result = await _financeService.MarkLoanAsInactive(loanId, reason, description);
        return FromService(result);
    }

    [HttpPost("MarkLoanAsPaid")]
    public async Task<IActionResult> MarkLoanAsPaid([FromForm] int loanId, [FromForm] bool isPaidByYou, [FromForm] string? relation, [FromForm] string? description)
    {
        var result = await _financeService.MarkLoanAsPaid(loanId, isPaidByYou, relation, description);
        return FromService(result);
    }

    [HttpGet("BankAccountFamilyGUID")]
    public async Task<IActionResult> BankAccountFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _financeService.BankAccountFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SavePersonBankAccount")]
    public async Task<IActionResult> SavePersonBankAccount([FromForm] int personId, [FromForm] string accountTitle, [FromForm] string accountNumber, [FromForm] string bankType, [FromForm] string? bankName, [FromForm] string? branch, [FromForm] string? city, [FromForm] bool isOwnAccount, [FromForm] string? relationshipWithAccountHolder, [FromForm] string? reasonForUsingOthersAccount)
    {
        var result = await _financeService.SavePersonBankAccount(personId, accountTitle, accountNumber, bankType, bankName, branch, city, isOwnAccount, relationshipWithAccountHolder, reasonForUsingOthersAccount);
        return FromService(result);
    }

    [HttpPost("MarkBankAccountAsInactive")]
    public async Task<IActionResult> MarkBankAccountAsInactive([FromForm] int bankAccountId, [FromForm] string reason, [FromForm] string? description)
    {
        var result = await _financeService.MarkBankAccountAsInactive(bankAccountId, reason, description);
        return FromService(result);
    }

    [HttpGet("InvestmentFamilyGUID")]
    public async Task<IActionResult> InvestmentFamilyGUID([FromQuery] Guid FamilyGUID)
    {
        var result = await _financeService.InvestmentFamilyGUID(FamilyGUID);
        return FromService(result);
    }

    [HttpPost("SavePersonInvestment")]
    public async Task<IActionResult> SavePersonInvestment([FromForm] int personId, [FromForm] string investmentType, [FromForm] decimal amountInvested, [FromForm] DateTime investmentDate, [FromForm] bool isFixedTerm, [FromForm] int? fixedDurationMonths, [FromForm] bool isReturnInPercentage, [FromForm] decimal? expectedReturnValue, [FromForm] decimal? expectedReturnPercentage, [FromForm] string returnFrequency, [FromForm] string? remarks)
    {
        var result = await _financeService.SavePersonInvestment(personId, investmentType, amountInvested, investmentDate, isFixedTerm, fixedDurationMonths, isReturnInPercentage, expectedReturnValue, expectedReturnPercentage, returnFrequency, remarks);
        return FromService(result);
    }

    [HttpPost("MarkInvestmentAsInactive")]
    public async Task<IActionResult> MarkInvestmentAsInactive([FromForm] int investmentId, [FromForm] string reason, [FromForm] string? description)
    {
        var result = await _financeService.MarkInvestmentAsInactive(investmentId, reason, description);
        return FromService(result);
    }
}
