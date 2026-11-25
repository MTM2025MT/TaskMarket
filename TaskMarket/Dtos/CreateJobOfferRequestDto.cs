#nullable enable
using System;
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos;

public class CreateJobOfferRequestDto
{
    [Range(1, int.MaxValue)]
    public int TaskId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MinPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal MaxPrice { get; set; }

    public DateTime Deadline { get; set; }

    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Range(-180, 180)]
    public double? Longitude { get; set; }
}