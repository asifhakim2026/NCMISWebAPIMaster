using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class ApplicationStageAssignee
    {
        public int ApplicationStageAssigneeId { get; set; }

        public int ApplicationStageStatusId { get; set; } // FK from ApplicationStageStatus

        [NotMapped]
        public virtual ApplicationStageStatus ApplicationStageStatus { get; set; } // Navigation Property

        public int UserId { get; set; } // Assigned User

        [NotMapped]
        public virtual UserLogin User { get; set; } // Navigation Property to User (if you have a User table)
    }

}
