﻿namespace Backpacking.API.Models.DTO;

public class LocationDTO
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public DateTimeOffset ArriveDate { get; set; }
    public DateTimeOffset DepartDate { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset LastModifiedDate { get; set; }

    public LocationDTO(Location location)
    {
        Id = location.Id;
        Name = location.Name;
        Longitude = location.Longitude;
        Latitude = location.Latitude;
        ArriveDate = location.ArriveDate;
        DepartDate = location.DepartDate;
        CreatedDate = location.CreatedDate;
        LastModifiedDate = location.LastModifiedDate;
    }
}
