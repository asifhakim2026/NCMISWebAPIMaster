using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Identity.Client;

namespace NCMIS.Models
{
    public class Location
    {
        [Key]
        public int LocationID { get; set; }

        [Required]
        [StringLength(255)]
        public string LocationName { get; set; }


        [Required]
        [StringLength(255)]
        public string ShortCode { get; set; }

        public int? ParentID { get; set; }


       


        public int LocationTypeId { get; set; }//fetching from general setup


      


       

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("Datetime")]
        public DateTime InsertDate { get; set; }

        [Column("CreatedBy")]
        [StringLength(100)] // Added limit for efficiency
        public string CreatedBy { get; set; }

        [Column("Remarks")]
        [StringLength(1000)] // Define reasonable max length
        public string? Remarks { get; set; }

      
    }
}
