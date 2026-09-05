using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NCMIS.Models
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(255)]
        public string DepartmentName { get; set; }

        // ✅ Simple nullable integer column (No Foreign Key Constraint)
        public int? ParentID { get; set; }

      

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("Datetime")]
        public DateTime InsertDate { get; set; }

        [Column("CreatedBy")]
        [StringLength(100)]
        public string CreatedBy { get; set; }

        [Column("Remarks")]
        [StringLength(1000)]
        public string? Remarks { get; set; }
    }



}
