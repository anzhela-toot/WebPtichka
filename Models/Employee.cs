using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class Employee
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Position { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateOnly HireDate { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
