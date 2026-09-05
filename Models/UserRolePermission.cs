using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class UserRolePermission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required]
        public int UserId { get; set; }

   

        [Required]
        public int MenuId { get; set; }

     

    }
}
