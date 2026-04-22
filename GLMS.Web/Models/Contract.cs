using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public enum ContractStatus
{
    Draft,
    Active,
    Expired,
    OnHold
}

public class Contract
{
    public int Id { get; set; }

    [Required]
    public int ClientId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Required]
    public ContractStatus Status { get; set; }

    [Required]
    [StringLength(200)]
    public string ServiceLevel { get; set; } = string.Empty;

    public string? SignedAgreementPath { get; set; }

    public Client Client { get; set; } = null!;

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
