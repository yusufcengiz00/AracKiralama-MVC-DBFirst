using System;
using System.Collections.Generic;

namespace CarRentalManagementSystem_DBFirst.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
