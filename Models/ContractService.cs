using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class ContractService
{
    public int Id { get; set; }

    public int ContractId { get; set; }

    public int ServiceId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
