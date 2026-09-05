using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NCMIS.Models
{
    [Index(nameof(Email), IsUnique = true)]  // Ensures unique email
    [Index(nameof(UserName), IsUnique = true)]  // Prevents duplicate usernames
    [Index(nameof(IsActive))]  // Optimizes filtering by active users
    public class UserLogin
    {
        [Key]
        [Column("UserID")]
        public int UserID { get; set; }

        [Column("UserGUID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserGuid { get; set; }

        [Column("UserName")]
        [StringLength(200)]
        [Required]
        public string UserName { get; set; }

        [Column("Email")]
        [StringLength(500)]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Column("FullName")]
        [StringLength(200)]
        public string FullName { get; set; }

        [Column("Password")]
        [StringLength(200)]
        [Required]
        public string Password { get; set; }


        [Required]
        public int RoleId { get; set; } // Foreign key to Roles

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("IsViewer")]
        public bool IsViewer { get; set; }



        [Column("Datetime")]
        public DateTime InsertDate { get; set; }

        [Column("LastLogin")]
        public DateTime? LastLogin { get; set; }

        [Column("Remarks")]
        public string? Remarks { get; set; }

        [Column("OtherMetaData")]
        public string? OtherMetaData { get; set; }


        [Column("UserTypes")]
      
        [Required]
        public int UserTypes { get; set; }

        [Column("CreatedBy")]
        [Required]
        public string CreatedBy { get; set; }

        [Column("UpdatedBy")]
        public string? UpdatedBy { get; set; }

        [Column("UpdatedDate")]
        public DateTime? UpdatedDate { get; set; }



        public DateTime? ExpiryDate { get; set; }


        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockoutEndTime { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; } // Optional
    }
}
