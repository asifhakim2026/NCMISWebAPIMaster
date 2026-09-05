using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonBankAccount
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PersonId { get; set; }

        [Required]
        [StringLength(300)]
        public string AccountTitle { get; set; }

        [Required]
        [StringLength(50)]
        public string AccountNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string BankType { get; set; }  // e.g., CommercialBank, IslamicBank, etc.

        [Required]
        [StringLength(500)]
        public string BankName { get; set; }

        [StringLength(500)]
        public string Branch { get; set; }

        [StringLength(200)]
        public string AreaOrCity { get; set; }

        [Required]
        public bool IsItOwnAccount { get; set; }

        [StringLength(50)]
        public string? RelationshipWithAccountHolder { get; set; }  // Only if not own account

        [StringLength(250)]
        public string? ReasonForUsingOthersAccount { get; set; }    // Only if not own account

        [Required]
        public bool IsActive { get; set; } = true;



        [StringLength(100)]
        public string? ReasonForInActive { get; set; }
        [StringLength(500)]
        public string? DescriptionForInActive { get; set; }

        [StringLength(500)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
    }
}
