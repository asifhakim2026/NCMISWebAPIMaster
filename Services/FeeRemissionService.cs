using Microsoft.EntityFrameworkCore;
using NCMISAPI.Data;
using NCMISAPI.DTOs;

namespace NCMISAPI.Services;

public class FeeRemissionService : IFeeRemissionService
{
    private const int PageSize = 500;
    private readonly NcmisDbContext _dbContext;
    private readonly ILogger<FeeRemissionService> _logger;

    public FeeRemissionService(NcmisDbContext dbContext, ILogger<FeeRemissionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<FeeRemissionListResponseDto> GetFeeRemissionListAsync(
        int userId,
        FeeRemissionListRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;

        var assignedLocationIds = await _dbContext.UserLocations
            .AsNoTracking()
            .Where(x =>
                x.UserID == userId &&
                x.IsActive &&
                x.Type == "region")
            .Select(x => x.LocationID)
            .ToListAsync(cancellationToken);

        var stepFlagsDict = await _dbContext.ProjectWorkflowSteps
            .AsNoTracking()
            .Where(s => s.IsActive)
            .ToDictionaryAsync(
                s => s.StepID,
                s => new
                {
                    s.IsSurveyor,
                    s.IsSeen,
                    s.RequestGeneratorURL,
                    s.IsRequestCreator
                },
                cancellationToken);

        var query =
            from fee in _dbContext.FeesRemisssions.AsNoTracking()
            join person in _dbContext.PersonalInfos.AsNoTracking()
                on fee.PersonId equals person.PersonId
            join fg in _dbContext.FamilyGroups.AsNoTracking()
                on person.FamilyId equals fg.FamilyId
            join pr in _dbContext.Projects.AsNoTracking()
                on fee.ProjectId equals pr.ProjectID
            join wt in _dbContext.WorkflowTrackers.AsNoTracking()
                on fee.FeeRemissionId equals wt.ApplicationId
            where wt.ModuleName == "FeesRemission"
                  && assignedLocationIds.Contains(fee.JKID)
            select new { fee, person, fg, pr, wt };

        if (request.IsActionFilter && request.StepId.HasValue)
        {
            query = query.Where(u =>
                u.wt.CurrentStepId == request.StepId.Value &&
                u.fee.IsCurrentStepActive == true &&
                u.wt.CurrentAssignToUserID == userId);
        }

        if (request.SchoolId.HasValue)
            query = query.Where(x => x.fee.SchoooID == request.SchoolId.Value);

        if (!string.IsNullOrWhiteSpace(request.CaseStatus))
        {
            var caseStatus = request.CaseStatus.Trim().ToLower();
            query = query.Where(x =>
                (x.fee.CaseApprovalStatus ?? "").ToLower().Trim() == caseStatus);
        }

        if (!string.IsNullOrWhiteSpace(request.StudentEnrollmentNumber))
        {
            var enrollment = request.StudentEnrollmentNumber.Trim();
            query = query.Where(x =>
                x.fee.SchoolEnrollmentNumber.Contains(enrollment));
        }

        if (!string.IsNullOrWhiteSpace(request.KeywordFilter))
        {
            var keyword = request.KeywordFilter.Trim();
            query = query.Where(x =>
                (x.fee.CaseNumber ?? "").Contains(keyword) ||
                (x.fee.StudentFirstName ?? "").Contains(keyword) ||
                (x.fee.StudentLastName ?? "").Contains(keyword) ||
                (x.fee.StudentCNIC ?? "").Contains(keyword) ||
                (x.fee.FatherName ?? "").Contains(keyword) ||
                (x.fee.PhoneNumber ?? "").Contains(keyword) ||
                (x.fee.SchoolEnrollmentNumber ?? "").Contains(keyword));
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.fee.FeeRemissionId)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(x => new FeeRemissionListItemDto
            {
                FamilyId = x.person.FamilyId,
                PersonId = x.fee.PersonId,
                FeeRemissionId = x.fee.FeeRemissionId,
                FeeRemissionGUID = x.fee.FeeRemissionGUID,
                CaseNumber = x.fee.CaseNumber,
                SurveyConsent = x.fee.SurveyConsent,
                PreferredSurveyTime = x.fee.PreferredSurveyTime,
                VisitorName = x.fee.VisitorName,
                Relation = x.fee.Relation,
                CaseType = x.fee.CaseType,
                CreatedBy = x.fee.CreatedBy,
                InsertDate = x.fee.InsertDate,
                UpdatedBy = x.fee.UpdatedBy,
                UpdatedDate = x.fee.UpdatedDate,
                StudyingClass = x.fee.StudyingClass,
                StudyingSection = x.fee.Section,
                DOB = x.fee.DateOfBirth,
                StudentFirstName = x.fee.StudentFirstName,
                StudentLastName = x.fee.StudentLastName,
                FatherName = x.fee.FatherName,
                StudentCNIC = x.fee.StudentCNIC,
                PhoneNumber = x.fee.PhoneNumber,
                EmailAddress = x.fee.EmailAddress,
                CompleteAddress = x.fee.CompleteAddress,
                SchoolEnrollmentNumber = x.fee.SchoolEnrollmentNumber,
                SchoooID = x.fee.SchoooID,
                StepId = x.wt.CurrentStepId,
                IsCurrentStepActive = x.fee.IsCurrentStepActive,
                NetFeeRate = x.fee.NetFeeRate,
                HostelFee = x.fee.HostelFee,
                CurrentBalance = x.fee.CurrentBalance,
                CurrentFA_Percentage = x.fee.CurrentFA_Percentage,
                CurrentHostelFA_Percentage = x.fee.CurrentHostelFA_Percentage,
                Remarks = x.fee.Remarks,
                JKID = x.fee.JKID,
                CurrentStatus = x.fee.CurrentStatus,
                FatherCNIC = x.fee.FatherCNIC,
                MotherName = x.fee.MotherName,
                MotherCNIC = x.fee.MotherCNIC,
                Gender = x.fee.Gender,
                CaseApprovalStatus = x.fee.CaseApprovalStatus,
                ClientAcceptanceStatus = x.fee.ClientAcceptanceStatus,
                CurrentAssignTo = x.fee.CurrentAssignTo,
                CurrentStep = x.fee.CurrentStep,
                IsManual = x.fee.IsManual,
                VoucherNumber = x.fee.VoucherNumber,
                PersonGuid = x.person.PersonalGuid,
                FamilyGUID = x.fg.FamilyGroupGuid,
                ProjectGUID = x.pr.ProjectGUID,
                ProjectId = x.pr.ProjectID
            })
            .ToListAsync(cancellationToken);

        var stepId = request.StepId ?? 0;
        stepFlagsDict.TryGetValue(stepId, out var stepFlags);

        _logger.LogInformation(
            "FeeRemissionList userId={UserId} page={Page} total={Total} returned={Count}",
            userId,
            page,
            totalRecords,
            items.Count);

        return new FeeRemissionListResponseDto
        {
            Success = true,
            Message = "OK",
            CurrentPage = page,
            PageSize = PageSize,
            TotalRecords = totalRecords,
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize),
            IsSurveyor = stepFlags?.IsSurveyor ?? false,
            IsSeen = stepFlags?.IsSeen ?? false,
            RequestCreatorURL = stepFlags?.RequestGeneratorURL ?? string.Empty,
            IsRequestCreator = stepFlags?.IsRequestCreator ?? false,
            Items = items
        };
    }
}
