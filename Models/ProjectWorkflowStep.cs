using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class ProjectWorkflowStep
    {
        [Key]
        public int StepID { get; set; } // Unique Step ID

        [Required]
        public int ProjectID { get; set; } // Link to Project

      

        [Required]
        [StringLength(255)]
        public string StepName { get; set; } // e.g., "School Review", "Regional Office Review"


        [StringLength(255)]
        public string? Description { get; set; } // e.g., "School Review", "Regional Office Review"

        public int StepOrder { get; set; } // Defines the order of steps in the workflow

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string ActionButtonLabel { get; set; }

        public bool IsRequestCreator { get; set; }


        public bool IsAutoUserAssigned { get; set; }
        public bool IsSurveyor { get; set; }
        public bool CanSkipStep { get; set; }

        public bool IsDecisionMaker { get; set; }

        public bool CanGiveRecommendation { get; set; }

        public bool IsSeen { get; set; }


        [StringLength(50)]
        public string CSSClass { get; set; }

        [StringLength(300)]
        public string? URL { get; set; }

        public string? RequestGeneratorURL { get; set; }

        // Link to Form Template
     /*   public int? FormTemplateID { get; set; } */ // New property to link with FormTemplate
       
        /*public virtual FormTemplate FormTemplate { get; set; } */ // Navigation property
    }
}
