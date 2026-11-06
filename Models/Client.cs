using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class Client
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public DateOnly RegistrationDate { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
}
