using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class ProjectProjectType
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int ProjectID { get; set; }

       

        [Required]
        public int ProjectTypeID { get; set; }

    
    }

}
