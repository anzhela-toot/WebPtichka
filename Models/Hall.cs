using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class Hall
{
    public int Id { get; set; }

    public string HallName { get; set; } = null!;

    public int Capacity { get; set; }

    public decimal PricePerHour { get; set; }

    public string HallStatus { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
