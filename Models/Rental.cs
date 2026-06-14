using System;
using System.Collections.Generic;

namespace CarRentalManagementSystem_DBFirst.Models;

public partial class Rental
{
    public int RentalId { get; set; }

    public int? CustomerId { get; set; }

    public int? VehicleId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public decimal? TotalPrice { get; set; }

    public string? RentalStatus { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Vehicle? Vehicle { get; set; }
}
