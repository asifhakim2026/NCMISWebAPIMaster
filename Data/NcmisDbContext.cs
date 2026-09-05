using Microsoft.EntityFrameworkCore;
using NCMIS.Models;

namespace NCMISAPI.Data;

public class NcmisDbContext : DbContext
{
    public NcmisDbContext(DbContextOptions<NcmisDbContext> options)
        : base(options)
    {
    }

    #region Department
    public virtual DbSet<Department> Departments { get; set; }
    #endregion

    #region UserLogins
    public virtual DbSet<UserLogin> UserLogins { get; set; }
    public virtual DbSet<UserLocation> UserLocations { get; set; }
    public virtual DbSet<UserDepartmentMapping> UserDepartmentMappings { get; set; }
    public virtual DbSet<UserSetupProjectWiseRoleMapping> UserSetupProjectWiseRoleMappings { get; set; }
    public virtual DbSet<LoginAudit> LoginAudits { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    #endregion

    #region Fee Remission / Projects
    public virtual DbSet<FeesRemission> FeesRemisssions { get; set; }
    public virtual DbSet<Project> Projects { get; set; }
    public virtual DbSet<WorkflowTracker> WorkflowTrackers { get; set; }
    public virtual DbSet<ProjectWorkflowStep> ProjectWorkflowSteps { get; set; }
    #endregion

    #region Person / Family
    public virtual DbSet<PersonalInfo> PersonalInfos { get; set; }
    public virtual DbSet<FamilyGroup> FamilyGroups { get; set; }
    public virtual DbSet<PersonFamily> PersonFamilies { get; set; }
    public virtual DbSet<RelationshipType> RelationshipTypes { get; set; }
    public virtual DbSet<Location> Locations { get; set; }
    public virtual DbSet<PersonAddress> PersonAddress { get; set; }
    public virtual DbSet<PersonEnrollment> PersonEnrollments { get; set; }
    public virtual DbSet<PersonSurveyMaster> PersonSurveyMasters { get; set; }
    public virtual DbSet<PersonHouseHoldResponse> PersonHouseHoldResponses { get; set; }
    public virtual DbSet<GeneralSetup> GeneralSetups { get; set; }
    public virtual DbSet<PersonLoan> PersonLoans { get; set; }
    public virtual DbSet<PersonBankAccount> PersonBankAccounts { get; set; }
    public virtual DbSet<PersonInvestment> PersonInvestments { get; set; }
    public virtual DbSet<PersonSeniorCitizen> PersonSeniorCitizens { get; set; }
    public virtual DbSet<PersonEducationDetail> PersonEducationDetails { get; set; }
    public virtual DbSet<PersonEducationFundingSource> PersonEducationFundingSources { get; set; }
    public virtual DbSet<PersonWorkExperience> PersonWorkExperiences { get; set; }
    public virtual DbSet<PersonWorkIncomeComponent> PersonWorkIncomeComponents { get; set; }
    public virtual DbSet<PersonAttachment> PersonAttachments { get; set; }
    public virtual DbSet<PersonYouthEducation> PersonYouthEducations { get; set; }
    public virtual DbSet<PersonLifeSkill> PersonLifeSkills { get; set; }
    public virtual DbSet<LifeSkillsMaster> LifeSkillsMasters { get; set; }
    public virtual DbSet<SurveyorFamilyNote> SurveyorFamilyNotes { get; set; }
    public virtual DbSet<FamilyVerificationRecord> FamilyVerificationRecords { get; set; }
    public virtual DbSet<PersonDeceasedInfo> PersonDeceasedInfos { get; set; }
    public virtual DbSet<PersonHealthCondition> PersonHealthConditions { get; set; }
    public virtual DbSet<SetupHealthCondition> SetupHealthConditions { get; set; }
    public virtual DbSet<SetupCauseOfDeathType> SetupCauseOfDeathTypes { get; set; }
    public virtual DbSet<Graveyard> Graveyards { get; set; }
    public virtual DbSet<TempHouseholdRaw> TempHouseholdRaws { get; set; }
    #endregion

    #region HouseHold Support
    public virtual DbSet<SetupHouseHoldCategory> SetupHouseHoldCategories { get; set; }
    #endregion

    #region Error Logs
    public virtual DbSet<ErrorLog> ErrorLogs { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<PersonAddress>().ToTable("PersonAddress");
        modelBuilder.Entity<PersonFamily>().ToTable("PersonFamilies");
        modelBuilder.Entity<RelationshipType>().ToTable("RelationshipTypes");

        modelBuilder.Entity<ErrorLog>(entity =>
        {
            entity.ToTable("ErrorLogs");
            entity.HasKey(e => e.ErrorLogId);
            entity.Property(e => e.Type).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ControllerName).HasMaxLength(200);
            entity.Property(e => e.ClassName).HasMaxLength(200);
            entity.Property(e => e.MethodName).HasMaxLength(200);
            entity.Property(e => e.ErrorDescription).IsRequired();
            entity.Property(e => e.UserName).HasMaxLength(300);
            entity.Property(e => e.ModuleName).HasMaxLength(200);
            entity.Property(e => e.ExceptionType).HasMaxLength(500);
            entity.Property(e => e.StackTrace).HasMaxLength(4000);
            entity.Property(e => e.InnerException).HasMaxLength(2000);
            entity.Property(e => e.FileName).HasMaxLength(500);
            entity.Property(e => e.RequestPath).HasMaxLength(1000);
            entity.Property(e => e.MachineName).HasMaxLength(200);
        });
    }
}


