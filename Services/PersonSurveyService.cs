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

public class PersonSurveyService : PersonServiceBase, IPersonSurveyService
{


    public PersonSurveyService(NcmisDbContext dbContext, IHttpContextAccessor httpContextAccessor, ILogger<PersonSurveyService> logger, ErrorLogHelper errorLogHelper)
        : base(dbContext, httpContextAccessor, logger, errorLogHelper)
    {

    }

    public async Task<PersonServiceResult> HouseholdSurvey(Guid familyGuid)
    {
        if (familyGuid == Guid.Empty)
            return PersonServiceResult.BadRequest(FailResult("Invalid Family GUID."));

        var person = await _dbContext.FamilyGroups
            .Where(p => p.FamilyGroupGuid == familyGuid)
            .Select(p => new { p.HeadPersonId, p.FamilyId })
            .FirstOrDefaultAsync();

        if (person == null)
            return PersonServiceResult.Ok(FailResult("Person not found."));

        // Parent categories (Type == HouseHold, ParentId == 0)
        var parentCategoryIds = await _dbContext.GeneralSetups
            .Where(u => u.ParentId == 0 && u.Type == "HouseHold")
            .Select(u => u.Id)
            .ToListAsync();

        var generalSetupItems = await _dbContext.GeneralSetups
            .Where(x => parentCategoryIds.Contains(x.ParentId) && x.IsActive)
            .ToListAsync();

        var parentCategories = await _dbContext.GeneralSetups
            .Where(x => parentCategoryIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.QuestionType })
            .ToListAsync();

        // Shape matches MVC SurveyQuestionModel: ParentId, QuestionText, QuestionType, Options[{OptionId, Name}]
        var surveyQuestions = parentCategories.Select(parent => new
        {
            ParentId = parent.Id,
            QuestionText = parent.Name,
            QuestionType = parent.QuestionType,
            Options = generalSetupItems
                .Where(x => x.ParentId == parent.Id)
                .Select(x => new
                {
                    OptionId = x.Id,
                    Name = x.Name
                }).ToList()
        }).ToList();

        return PersonServiceResult.Ok(OkResult("OK", surveyQuestions));
    }

    public async Task<PersonServiceResult> SaveHouseHoldSurveyResponse(SurveyRequestDto model, Guid familyGuid)
    {
        if (model == null)
            return PersonServiceResult.BadRequest(FailResult("Invalid request data."));

        if (familyGuid == Guid.Empty)
            return PersonServiceResult.BadRequest(FailResult("Invalid Family GUID."));

        var person = await _dbContext.FamilyGroups
            .Where(p => p.FamilyGroupGuid == familyGuid)
            .Select(p => new { p.HeadPersonId, p.FamilyId })
            .FirstOrDefaultAsync();

        if (person == null)
            return PersonServiceResult.BadRequest(FailResult("Family not found."));

        // Deactivate prior active HouseHold surveys for family / head person
        await InactivateSurveysAsync(person.FamilyId, person.HeadPersonId, "HouseHold");

        var personSurveyMaster = new PersonSurveyMaster
        {
            PersonId = person.HeadPersonId,
            FamilyId = person.FamilyId,
            CreatedBy = CurrentUserName,
            IsActive = true,
            SurveyType = "HouseHold"
        };

        _dbContext.PersonSurveyMasters.Add(personSurveyMaster);
        await _dbContext.SaveChangesAsync();

        foreach (var response in model.Responses ?? [])
        {
            _dbContext.PersonHouseHoldResponses.Add(new PersonHouseHoldResponse
            {
                PersonSurveyMasterId = personSurveyMaster.PersonSurveyMasterId,
                PersonId = person.HeadPersonId,
                FamilyId = person.FamilyId,
                ParentId = response.ParentId,
                OptionId = response.OptionId,
                IsChecked = response.IsChecked,
                AnswerText = response.AnswerText,
                CreatedBy = CurrentUserName
            });
        }

        await _dbContext.SaveChangesAsync();
        return PersonServiceResult.Ok(OkResult("Survey saved successfully!"));
    }

    public async Task<PersonServiceResult> HouseholdSurveyAnalysis(Guid familyGuid)
    {
        if (familyGuid == Guid.Empty)
            return PersonServiceResult.BadRequest(FailResult("Invalid Person GUID."));

        var person = await _dbContext.FamilyGroups
            .Where(p => p.FamilyGroupGuid == familyGuid)
            .Select(p => new { p.HeadPersonId, p.FamilyId })
            .FirstOrDefaultAsync();

        if (person == null)
            return PersonServiceResult.Ok(FailResult("Person not found."));

        var surveys = await _dbContext.PersonSurveyMasters
            .Where(s => (s.PersonId == person.HeadPersonId || s.FamilyId == person.FamilyId)
                        && s.SurveyType == "HouseHold")
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        // Analysis: parents Id/Name only; options without IsActive filter (matches MVC)
        var parentCategoryIds = await _dbContext.GeneralSetups
            .Where(u => u.ParentId == 0 && u.Type == "HouseHold")
            .Select(u => u.Id)
            .ToListAsync();

        var generalSetupItems = await _dbContext.GeneralSetups
            .Where(x => parentCategoryIds.Contains(x.ParentId))
            .ToListAsync();

        var parentCategories = await _dbContext.GeneralSetups
            .Where(x => parentCategoryIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name })
            .ToListAsync();

        var surveyQuestions = parentCategories.Select(parent => new SurveyQuestionDto
        {
            ParentId = parent.Id,
            QuestionText = parent.Name,
            Options = generalSetupItems
                .Where(x => x.ParentId == parent.Id)
                .Select(x => new SurveyOptionDto
                {
                    OptionId = x.Id,
                    Name = x.Name
                }).ToList()
        }).ToList();

        var surveyData = await BuildSurveyDataAsync(surveys);

        return PersonServiceResult.Ok(OkResult("OK", new HouseholdSurveyAnalysisDto
        {
            Questions = surveyQuestions,
            Surveys = surveyData
        }));
    }

    public async Task<PersonServiceResult> incomeexpenseload(Guid familyGuid)
        {
            try
            {
            if (familyGuid == Guid.Empty)
                return PersonServiceResult.BadRequest(FailResult("Invalid Person GUID."));

            var person = await _dbContext.FamilyGroups
                .Where(p => p.FamilyGroupGuid == familyGuid)
                .Select(p => new { p.HeadPersonId, p.FamilyId })
                .FirstOrDefaultAsync();

            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person not found."));

            var parentCategoryIds = await _dbContext.GeneralSetups
                .Where(u => u.ParentId == 0 && (u.Type == "Income" || u.Type == "Expense"))
                .Select(u => u.Id)
                .ToListAsync();

            var generalSetupItems = await _dbContext.GeneralSetups
                .Where(x => parentCategoryIds.Contains(x.ParentId) && x.IsActive)
                .ToListAsync();

            var parentCategories = await _dbContext.GeneralSetups
                .Where(x => parentCategoryIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name, x.QuestionType, x.ShortCode })
                .ToListAsync();

            var surveyQuestions = parentCategories.Select(parent => new SurveyQuestionDto
            {
                ParentId = parent.Id,
                QuestionText = parent.Name,
                QuestionType = parent.QuestionType,
                ShortCode = parent.ShortCode,
                Options = generalSetupItems
                    .Where(x => x.ParentId == parent.Id)
                    .Select(x => new SurveyOptionDto
                    {
                        OptionId = x.Id,
                        Name = x.Name,
                        ShortCode = x.ShortCode
                    }).ToList()
            }).ToList();

            return PersonServiceResult.Ok(OkResult("OK", surveyQuestions));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "incomeexpenseload failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> SaveIncomeExpense(SurveyRequestDto model, Guid familyGuid)
        {
            try
            {
            if (model == null)
                return PersonServiceResult.BadRequest(FailResult("Invalid request data."));

            if (familyGuid == Guid.Empty)
                return PersonServiceResult.BadRequest(FailResult("Invalid Person GUID."));

            var person = await _dbContext.FamilyGroups
                .Where(p => p.FamilyGroupGuid == familyGuid)
                .Select(p => new { p.HeadPersonId, p.FamilyId })
                .FirstOrDefaultAsync();

            if (person == null)
                return PersonServiceResult.BadRequest(FailResult("Person not found."));

            await InactivateSurveysAsync(person.FamilyId, person.HeadPersonId, "IncomeExpense");

            var personSurveyMaster = new PersonSurveyMaster
            {
                PersonId = person.HeadPersonId,
                FamilyId = person.FamilyId,
                CreatedBy = CurrentUserName,
                IsActive = true,
                SurveyType = "IncomeExpense",
                CreatedAt = DateTime.Now,
                SurveyGuid = Guid.NewGuid()
            };

            _dbContext.PersonSurveyMasters.Add(personSurveyMaster);
            await _dbContext.SaveChangesAsync();

            foreach (var response in model.Responses)
            {
                _dbContext.PersonHouseHoldResponses.Add(new PersonHouseHoldResponse
                {
                    PersonSurveyMasterId = personSurveyMaster.PersonSurveyMasterId,
                    PersonId = person.HeadPersonId,
                    FamilyId = person.FamilyId,
                    ParentId = response.ParentId,
                    OptionId = response.OptionId,
                    IsChecked = true,
                    AnswerText = response.AnswerText,
                    CreatedBy = CurrentUserName,
                    InsertDate = DateTime.Now
                });
            }

            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Income & Expense saved successfully!"));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SaveIncomeExpense failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> IncomeExpenseSurveyAnalysis(Guid familyGuid)
        {
            try
            {
            if (familyGuid == Guid.Empty)
                return PersonServiceResult.BadRequest(FailResult("Invalid Person GUID."));

            var person = await _dbContext.FamilyGroups
                .Where(p => p.FamilyGroupGuid == familyGuid)
                .Select(p => new { p.HeadPersonId, p.FamilyId })
                .FirstOrDefaultAsync();

            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person not found."));

            var surveys = await _dbContext.PersonSurveyMasters
                .Where(s => (s.PersonId == person.HeadPersonId || s.FamilyId == person.FamilyId) && s.SurveyType == "IncomeExpense")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            int incomeParentId = await _dbContext.GeneralSetups
                .Where(u => u.ParentId == 0 && u.Type == "Income")
                .Select(u => u.Id)
                .FirstAsync();

            int expenseParentId = await _dbContext.GeneralSetups
                .Where(u => u.ParentId == 0 && u.Type == "Expense")
                .Select(u => u.Id)
                .FirstAsync();

            var activeExpenseHeadId = await _dbContext.GeneralSetups
                .Where(x => x.IsActive && x.Type == "Expense" && x.ParentId != 0)
                .Select(x => x.Id)
                .ToListAsync();

            var activIncomeHeadId = await _dbContext.GeneralSetups
                .Where(x => x.IsActive && x.Type == "Income" && x.ParentId != 0)
                .Select(x => x.Id)
                .ToListAsync();

            var parentCategoryIds = await _dbContext.GeneralSetups
                .Where(u => u.ParentId == 0 && (u.Type == "Income" || u.Type == "Expense"))
                .Select(u => u.Id)
                .ToListAsync();

            var generalSetupItems = await _dbContext.GeneralSetups
                .Where(x => parentCategoryIds.Contains(x.ParentId) && x.IsActive)
                .ToListAsync();

            var parentCategories = await _dbContext.GeneralSetups
                .Where(x => parentCategoryIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name })
                .ToListAsync();

            var surveyQuestions = parentCategories.Select(parent => new SurveyQuestionDto
            {
                ParentId = parent.Id,
                QuestionText = parent.Name,
                Options = generalSetupItems
                    .Where(x => x.ParentId == parent.Id)
                    .Select(x => new SurveyOptionDto { OptionId = x.Id, Name = x.Name })
                    .ToList()
            }).ToList();

            var surveyData = new List<object>();
            decimal totalIncome = 0;
            decimal totalExpense = 0;

            foreach (var survey in surveys)
            {
                var responses = await _dbContext.PersonHouseHoldResponses
                    .Where(r => r.PersonSurveyMasterId == survey.PersonSurveyMasterId)
                    .ToListAsync();

                decimal totalSurveyIncome = responses
                    .Where(x => x.ParentId == incomeParentId && activIncomeHeadId.Contains(x.OptionId))
                    .Sum(x => decimal.TryParse(x.AnswerText, out var value) ? value : 0);

                decimal totalSurveyExpense = responses
                    .Where(x => x.ParentId == expenseParentId && activeExpenseHeadId.Contains(x.OptionId))
                    .Sum(x => decimal.TryParse(x.AnswerText, out var value) ? value : 0);

                surveyData.Add(new
                {
                    survey.CreatedBy,
                    SurveyDate = survey.CreatedAt,
                    TotalExpense = totalSurveyExpense,
                    TotalIncome = totalSurveyIncome,
                    Responses = responses.Select(r => new
                    {
                        r.OptionId,
                        r.IsChecked,
                        r.AnswerText
                    }).ToList()
                });

                foreach (var response in responses)
                {
                    if (response.AnswerText != null && decimal.TryParse(response.AnswerText, out var value))
                    {
                        var category = generalSetupItems.FirstOrDefault(x => x.Id == response.OptionId);
                        if (category != null)
                        {
                            if (category.ParentId == incomeParentId) totalIncome += value;
                            else if (category.ParentId == expenseParentId) totalExpense += value;
                        }
                    }
                }
            }

            return PersonServiceResult.Ok(OkResult("OK", new
            {
                Questions = surveyQuestions,
                Surveys = surveyData,
                TotalIncome = totalIncome,
                TotalExpense = totalExpense
            }));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "IncomeExpenseSurveyAnalysis failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> AddtionalSupportsurveyload(Guid familyGuid)
    {
        if (familyGuid == Guid.Empty)
            return PersonServiceResult.BadRequest(FailResult("Invalid Family GUID."));

        var person = await _dbContext.FamilyGroups
            .Where(p => p.FamilyGroupGuid == familyGuid)
            .Select(p => new { p.HeadPersonId, p.FamilyId })
            .FirstOrDefaultAsync();

        if (person == null)
            return PersonServiceResult.Ok(FailResult("Person not found."));

        // Parent categories (Type == SupportSurvey, ParentId == 0) — matches MVC AddtionalSupportsurveyload
        var parentCategoryIds = await _dbContext.GeneralSetups
            .Where(u => u.ParentId == 0 && u.Type == "SupportSurvey")
            .Select(u => u.Id)
            .ToListAsync();

        var generalSetupItems = await _dbContext.GeneralSetups
            .Where(x => parentCategoryIds.Contains(x.ParentId) && x.IsActive)
            .ToListAsync();

        var parentCategories = await _dbContext.GeneralSetups
            .Where(x => parentCategoryIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.QuestionType })
            .ToListAsync();

        // Shape matches MVC SurveyQuestionModel: ParentId, QuestionText, QuestionType, Options[{OptionId, Name}]
        var surveyQuestions = parentCategories.Select(parent => new
        {
            ParentId = parent.Id,
            QuestionText = parent.Name,
            QuestionType = parent.QuestionType,
            Options = generalSetupItems
                .Where(x => x.ParentId == parent.Id)
                .Select(x => new
                {
                    OptionId = x.Id,
                    Name = x.Name
                }).ToList()
        }).ToList();

        return PersonServiceResult.Ok(OkResult("OK", surveyQuestions));
    }

    public async Task<PersonServiceResult> SaveSupportSurveyResponse(SurveyRequestDto model, Guid familyGuid)
        {
            try
            {
            if (model == null)
                return PersonServiceResult.BadRequest(FailResult("Invalid request data."));

            if (familyGuid == Guid.Empty)
                return PersonServiceResult.BadRequest(FailResult("Invalid Person GUID."));

            var person = await _dbContext.FamilyGroups
                .Where(p => p.FamilyGroupGuid == familyGuid)
                .Select(p => new { p.HeadPersonId, p.FamilyId })
                .FirstOrDefaultAsync();

            if (person == null)
                return PersonServiceResult.BadRequest(FailResult("Person not found."));

            await InactivateSurveysAsync(person.FamilyId, person.HeadPersonId, "SupportSurvey");

            var personSurveyMaster = new PersonSurveyMaster
            {
                PersonId = person.HeadPersonId,
                FamilyId = person.FamilyId,
                CreatedBy = CurrentUserName,
                IsActive = true,
                SurveyType = "SupportSurvey",
                CreatedAt = DateTime.Now,
                SurveyGuid = Guid.NewGuid()
            };

            _dbContext.PersonSurveyMasters.Add(personSurveyMaster);
            await _dbContext.SaveChangesAsync();

            foreach (var response in model.Responses)
            {
                _dbContext.PersonHouseHoldResponses.Add(new PersonHouseHoldResponse
                {
                    PersonSurveyMasterId = personSurveyMaster.PersonSurveyMasterId,
                    PersonId = person.HeadPersonId,
                    FamilyId = person.FamilyId,
                    ParentId = response.ParentId,
                    OptionId = response.OptionId,
                    IsChecked = response.IsChecked,
                    AnswerText = response.AnswerText,
                    Name = response.Name,
                    CreatedBy = CurrentUserName,
                    InsertDate = DateTime.Now
                });
            }

            await _dbContext.SaveChangesAsync();
            return PersonServiceResult.Ok(OkResult("Support survey saved successfully!"));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "SaveSupportSurveyResponse failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }

    public async Task<PersonServiceResult> AddtionalSupportSurveyAnalysis(Guid FamilyGUID)
        {
            try
            {
            if (FamilyGUID == Guid.Empty)
                return PersonServiceResult.BadRequest(FailResult("Invalid Person GUID."));

            var person = await _dbContext.FamilyGroups
                .Where(p => p.FamilyGroupGuid == FamilyGUID)
                .Select(p => new { p.HeadPersonId, p.FamilyId })
                .FirstOrDefaultAsync();

            if (person == null)
                return PersonServiceResult.Ok(FailResult("Person not found."));

            var surveys = await _dbContext.PersonSurveyMasters
                .Where(s => (s.PersonId == person.HeadPersonId || s.FamilyId == person.FamilyId) && s.SurveyType == "SupportSurvey")
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            var surveyQuestions = await LoadSurveyQuestionsAsync("SupportSurvey", activeOptionsOnly: false);
            var surveyData = new List<object>();

            foreach (var survey in surveys)
            {
                var responses = await _dbContext.PersonHouseHoldResponses
                    .Where(r => r.PersonSurveyMasterId == survey.PersonSurveyMasterId)
                    .ToListAsync();

                surveyData.Add(new
                {
                    survey.CreatedBy,
                    SurveyDate = survey.CreatedAt,
                    Responses = responses.Select(r => new
                    {
                        r.OptionId,
                        r.IsChecked,
                        r.AnswerText,
                        r.Name
                    }).ToList()
                });
            }

            return PersonServiceResult.Ok(OkResult("OK", new { Questions = surveyQuestions, Surveys = surveyData }));

            }
            catch (Exception ex)
            {
                LogAndPersistError(ex, "AddtionalSupportSurveyAnalysis failed");
                return PersonServiceResult.Error(FailResult("An unexpected error occurred. Please try again."));
            }
    }


    private async Task InactivateSurveysAsync(int? familyId, int? headPersonId, string surveyType)
    {
        var previous = await _dbContext.PersonSurveyMasters
            .Where(s => (s.FamilyId == familyId || s.PersonId == headPersonId) && s.IsActive && s.SurveyType == surveyType)
            .ToListAsync();

        foreach (var s in previous)
            s.IsActive = false;

        if (previous.Count > 0)
            await _dbContext.SaveChangesAsync();
    }

    private async Task<List<SurveyQuestionDto>> LoadSurveyQuestionsAsync(string type, bool activeOptionsOnly = true)
    {
        var parentCategoryIds = await _dbContext.GeneralSetups
            .Where(u => u.ParentId == 0 && u.Type == type)
            .Select(u => u.Id)
            .ToListAsync();

        var optionsQuery = _dbContext.GeneralSetups.Where(x => parentCategoryIds.Contains(x.ParentId));
        if (activeOptionsOnly)
            optionsQuery = optionsQuery.Where(x => x.IsActive);

        var generalSetupItems = await optionsQuery.ToListAsync();

        var parentCategories = await _dbContext.GeneralSetups
            .Where(x => parentCategoryIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.QuestionType, x.ShortCode })
            .ToListAsync();

        return parentCategories.Select(parent => new SurveyQuestionDto
        {
            ParentId = parent.Id,
            QuestionText = parent.Name,
            QuestionType = parent.QuestionType,
            ShortCode = parent.ShortCode,
            Options = generalSetupItems
                .Where(x => x.ParentId == parent.Id)
                .Select(x => new SurveyOptionDto
                {
                    OptionId = x.Id,
                    Name = x.Name,
                    ShortCode = x.ShortCode
                }).ToList()
        }).ToList();
    }

    private async Task<List<SurveyAnalysisEntryDto>> BuildSurveyDataAsync(List<PersonSurveyMaster> surveys)
    {
        var surveyData = new List<SurveyAnalysisEntryDto>();
        foreach (var survey in surveys)
        {
            var responses = await _dbContext.PersonHouseHoldResponses
                .Where(r => r.PersonSurveyMasterId == survey.PersonSurveyMasterId)
                .ToListAsync();

            surveyData.Add(new SurveyAnalysisEntryDto
            {
                CreatedBy = survey.CreatedBy,
                SurveyDate = survey.CreatedAt,
                Responses = responses.Select(r => new SurveyAnalysisResponseItemDto
                {
                    OptionId = r.OptionId,
                    IsChecked = r.IsChecked,
                    AnswerText = r.AnswerText
                }).ToList()
            });
        }
        return surveyData;
    }
}



