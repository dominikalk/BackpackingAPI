using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO;

public class LogCurrentLocationDTO
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; } = string.Empty;

    [Required]
    public required float Longitude { get; set; }

    [Required]
    public required float Latitude { get; set; }
}
