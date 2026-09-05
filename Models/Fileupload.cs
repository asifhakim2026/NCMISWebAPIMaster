using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NCMIS.Models
{
    public class Fileupload
    {
        [Key]
        [Column("Fileuploadid")]

        [DisplayName("File Upload ID")]
        [Required(ErrorMessage = "File Upload ID is required")]
        public int FileUploadID { get; set; }

        [Column("FileUploadName")]
        [StringLength(100)]
        [DisplayName("File Upload Name")]
        [Required(ErrorMessage = "File Upload Name is required")]
        [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only characters are allowed for File Upload Name")]
        public string FileUploadName { get; set; }

        [Column("FileUploadShortName")]
        [StringLength(100)]
        [DisplayName("File Upload Short Name")]
        [Required(ErrorMessage = "File Upload Short Name is required")]
        [RegularExpression("^[a-zA-Z ]*$", ErrorMessage = "Only characters are allowed for File Upload Short Name")]
        public string FileUploadShortName { get; set; }

        public string GroupBy { get; set; } // personal, academic, professional,Legal ,Financial 


        //[Column("FileUploadType")]
        //[StringLength(50)]
        //[DisplayName("File Upload Type")]
        //[Required(ErrorMessage = "File Upload Type is required")]
        //public string FileUploadType { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

        public bool IsMandatory { get; set; }

        public int SortOrder { get; set; }
        public DateOnly InsertDate { get; set; }
    }

}
