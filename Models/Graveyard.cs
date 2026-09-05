using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class Graveyard
    {

        [key]
        public int Id { get; set; }


        public int LocationId { get; set; }

        [StringLength(300)]
        public string Name { get; set; }


        [StringLength(50)]
        public string? Sharedwithother { get; set; }

        [StringLength(300)]
        public string? Capacity { get; set; }


        [StringLength(300)]
        public string? LocationDescription { get; set; } // optional

       
        [StringLength(50)]
        public string? Type { get; set; }
     
        public bool IsActive { get; set; }

    }
}
