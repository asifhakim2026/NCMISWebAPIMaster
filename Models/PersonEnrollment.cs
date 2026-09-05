using System.ComponentModel.DataAnnotations;

namespace NCMIS.Models
{
    public class PersonEnrollment
    {
        [Key]
        public int EnrollmemntId { get; set; }

        public int PersonId { get; set; }

        public int ProjectId { get; set; }

        public int ReferenceID {get;set;}

        public string Module { get; set; }

        public string Remarks { get; set; }

        public string CreatedBy { get; set; }

        public DateTime InsertDate { get; set; }

    }
}
