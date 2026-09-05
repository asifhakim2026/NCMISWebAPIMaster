using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class SetupProjectWiseRole
    {
        [Key]
        public int SetupProjectWiseRoleId { get; set; }
      

        public int ProjectId { get; set; }

        [StringLength(500)]
        public string SectionKey { get; set; }

        [StringLength(500)]
        public string SectionDescription { get; set; }

        public bool IsActive { get; set; }

    }
}
