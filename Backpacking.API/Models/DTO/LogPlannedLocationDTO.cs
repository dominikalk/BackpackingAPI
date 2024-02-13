using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO;

public class LogPlannedLocationDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public float Longitude { get; set; }

    [Required]
    public float Latitude { get; set; }

    [Required]
    public DateTimeOffset ArriveDate { get; set; }

    public DateTimeOffset? DepartDate { get; set; }
}
