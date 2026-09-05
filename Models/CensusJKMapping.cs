using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class CensusJKMapping
    {
        [Key]
        [Column("CensusUserJKID")]
        public int CensusUserJKID { get; set; }

        public int CensusUserID { get; set; }

        public bool IsActive { get; set; }
    }
}
