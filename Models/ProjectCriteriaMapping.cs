using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class ProjectCriteriaMapping
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ProjectID { get; set; }


        [NotMapped]
        public virtual Project Project { get; set; }

        [Required]
        public int ValueID { get; set; }


        [NotMapped]
        public virtual CriteriaValue CriteriaValue { get; set; }


        public bool IsActive { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
