using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class Service
{
    public int Id { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal ServicePrice { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<ContractService> ContractServices { get; set; } = new List<ContractService>();
}
