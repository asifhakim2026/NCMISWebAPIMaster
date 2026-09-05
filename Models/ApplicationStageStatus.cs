using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class ApplicationStageStatus
    {

        public int ApplicationStageStatusId { get; set; }

        public int WorkFlowID { get; set; }
        public int ApplicationId { get; set; }

     

        public int ProjectID { get; set; }

        [StringLength(50)]
        public string Status { get; set; }  // Current status like Pending, Passed, Failed, etc.


        public bool? IsApproved { get; set; }

        public int? Rating { get; set; }

        public DateTime Date { get; set; }  // Timestamp for when this status was set

        public bool IsActive { get; set; }  // Indicates if the status is still active

        public string Description { get; set; }

        public string UserName { get; set; }

        public int UserId { get; set; }

      
    }
}
