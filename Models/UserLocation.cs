using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class UserLocation
    {
        [Key]
        public int UserLocationID { get; set; }

        [Required]
       
        public int LocationID { get; set; }

    

        [Required]
      
        public int UserID { get; set; }


        [StringLength(100)]
        public string Type { get; set; } //region school etc



        public bool IsActive { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
