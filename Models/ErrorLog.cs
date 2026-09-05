using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCMIS.Models;

[Table("ErrorLogs")]
public class ErrorLog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ErrorLogId { get; set; }

    /// <summary>Error | Warning | Information</summary>
    [Required]
    [StringLength(50)]
    public string Type { get; set; } = "Error";

    [StringLength(200)]
    public string? ControllerName { get; set; }

    [StringLength(200)]
    public string? ClassName { get; set; }

    [StringLength(200)]
    public string? MethodName { get; set; }

    [Required]
    public string ErrorDescription { get; set; } = string.Empty;

    public int? LineNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [StringLength(300)]
    public string? UserName { get; set; }

    [StringLength(200)]
    public string? ModuleName { get; set; }

    [StringLength(500)]
    public string? ExceptionType { get; set; }

    [StringLength(4000)]
    public string? StackTrace { get; set; }

    [StringLength(2000)]
    public string? InnerException { get; set; }

    /// <summary>Stores SourceFile (caller path or export context).</summary>
    [StringLength(500)]
    public string? FileName { get; set; }

    [StringLength(1000)]
    public string? RequestPath { get; set; }

    [StringLength(200)]
    public string? MachineName { get; set; }

    /// <summary>Optional JSON notes (filters, template, etc.)</summary>
    public string? AdditionalData { get; set; }
}
