using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class UserSetupProjectWiseRoleMapping
    {
        [Key]
        public int UserSetupProjectWiseRoleMappingID { get; set; }
        public int SetupProjectWiseRoleId { get; set; }

        public int UserId { get; set; } 

        public bool IsActive { get; set; }

    
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }


        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
