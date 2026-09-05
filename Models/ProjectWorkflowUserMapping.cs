using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace NCMIS.Models
{
    public class ProjectWorkflowUserMapping
    {
        [Key]
        public int AssignmentID { get; set; } // Unique ID for mapping

        [Required]
        public int StepID { get; set; } // Link to Workflow Step

       

        [Required]
        public int AssignedUserID { get; set; } // Assigned User


     

        public bool IsActive { get; set; } = true; // ✅ Active or Inactive
    }

       
    
}
