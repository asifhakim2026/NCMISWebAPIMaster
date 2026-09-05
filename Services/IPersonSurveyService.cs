using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonSurveyService
{
    Task<PersonServiceResult> HouseholdSurvey(Guid familyGuid);

    Task<PersonServiceResult> SaveHouseHoldSurveyResponse(SurveyRequestDto model, Guid familyGuid);

    Task<PersonServiceResult> HouseholdSurveyAnalysis(Guid familyGuid);

    Task<PersonServiceResult> incomeexpenseload(Guid familyGuid);

    Task<PersonServiceResult> SaveIncomeExpense(SurveyRequestDto model, Guid familyGuid);

    Task<PersonServiceResult> IncomeExpenseSurveyAnalysis(Guid familyGuid);

    Task<PersonServiceResult> AddtionalSupportsurveyload(Guid familyGuid);

    Task<PersonServiceResult> SaveSupportSurveyResponse(SurveyRequestDto model, Guid familyGuid);

    Task<PersonServiceResult> AddtionalSupportSurveyAnalysis(Guid FamilyGUID);

}
