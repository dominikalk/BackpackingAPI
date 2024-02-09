using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO;

public class LogCurrentLocationDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public float Longitude { get; set; }

    [Required]
    public float Latitude { get; set; }
}
