using System.ComponentModel.DataAnnotations;

namespace Backpacking.API.Models.DTO.LocationDTOs;

public class UpdatePlannedLocationDTO
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; } = string.Empty;

    [Required]
    public required float Longitude { get; set; }

    [Required]
    public required float Latitude { get; set; }

    [Required]
    public required DateTimeOffset ArriveDate { get; set; }

    public DateTimeOffset? DepartDate { get; set; } = DateTimeOffset.MaxValue;
}
