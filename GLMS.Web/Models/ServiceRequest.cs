using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GLMS.Web.Models;

public enum ServiceRequestStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled
}

public class ServiceRequest
{
    public int Id { get; set; }

    [Required]
    public int ContractId { get; set; }

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal CostUSD { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CostZAR { get; set; }

    [Required]
    public ServiceRequestStatus Status { get; set; }

    public Contract Contract { get; set; } = null!;
}
