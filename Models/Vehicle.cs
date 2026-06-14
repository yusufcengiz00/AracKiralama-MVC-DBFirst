using System;
using System.Collections.Generic;

namespace CarRentalManagementSystem_DBFirst.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public int? Year { get; set; }

    public string? PlateNumber { get; set; }

    public string? Color { get; set; }

    public double? Kilometer { get; set; }

    public decimal? DailyPrice { get; set; }

    public bool? Status { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
