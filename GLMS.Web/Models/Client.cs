using System.ComponentModel.DataAnnotations;

namespace GLMS.Web.Models;

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string ContactDetails { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Region { get; set; } = string.Empty;

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
