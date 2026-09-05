using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models
{
    public class PersonHealthCondition
    {
        [key]
        public int Id { get; set; }

      
        public int PersonDeceasedInfoId { get; set; }
      

    
        public int SetupHealthConditionId { get; set; }
        
    }
}
