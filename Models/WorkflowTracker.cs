using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class WorkflowTracker
    {
        [Key]
        public int WorkflowTrackerId { get; set; }

        public int ProjectId { get; set; }

        public int ApplicationId { get; set; } // e.g., FeeRemissionId

        [Required]
        [StringLength(100)]
        public string ModuleName { get; set; } // e.g., "FeesRemission", "LoanRequest"

        [Required]
        public string StageJson { get; set; } // JSON log (List<StageLogEntry>)

        public int CurrentStepId { get; set; }

        [StringLength(500)]
        public string CurrentStepName { get; set; }

        [StringLength(100)]
        public string CurrentStatus { get; set; }

        [StringLength(500)]
        public string AssignedTo { get; set; }

        public int? CurrentAssignToUserID { get; set; } // ✅ NEW: Required for dashboard filtering


        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;


        [StringLength(50)]
        public string? FinalStatus { get; set; } // e.g., "Rejected", "Cancelled", or null if active

        public bool IsCancelOrReject { get; set; } = false; // ✅ If true, do not show in active dashboard


        [StringLength(50)]
        public string? ApprovalStatus { get; set; } // e.g., "Approved", "Unapproved", or null if active

        public bool IsApprovalStatus { get; set; } = false; // ✅ If true, do not show in the dashboard process


        public int DecissionRating { get; set; }
    }
}
