using NCMISAPI.DTOs.Person;

namespace NCMISAPI.Services;

public interface IPersonEducationService
{
    Task<PersonServiceResult> EducationFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SaveEducation(
            int personId,
            string institutionName,
            string boardType,
            string board,
            string group,
            string degreeType,
            string fieldOfStudy,
            string? courseDuration,
            string? startDate,
            string? endDate,
            string? passingDate,
            int? totalMarks,
            int? obtainedMarks,
            string? remarks,
            string? fundingJson,
            bool isOngoing = false,
            bool notknown = false);

    Task<PersonServiceResult> MarkEducationAsInactive(int educationId, string reason, string? description);

    Task<PersonServiceResult> YouthEducationFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SaveYouthEducation(
            int personId,
            string className,
            string centerName,
            string completionStatus,
            string? completionDate);

    Task<PersonServiceResult> MarkAsInactive(int educationId, string reason, string? description);

    Task<PersonServiceResult> LifeSkillsFamilyGUID(Guid FamilyGUID);

    Task<PersonServiceResult> SaveSkillWithDetails(
            int PersonId,
            int SkillId,
            bool IsCertified,
            string? Proficiency,
            bool IsChecked);

    Task<PersonServiceResult> AddNewSkill(string SkillName, string Category);

}
