using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class Contract
{
    public int Id { get; set; }

    public string ContractNumber { get; set; } = null!;

    public int ClientId { get; set; }

    public int EmployeeId { get; set; }

    public int HallId { get; set; }

    public DateOnly EventDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public decimal TotalAmount { get; set; }

    public string ContractStatus { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual ICollection<ContractService> ContractServices { get; set; } = new List<ContractService>();

    public virtual Employee Employee { get; set; } = null!;

    public virtual Hall Hall { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
