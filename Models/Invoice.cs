using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class Invoice
{
    public int Id { get; set; }

    public int ContractId { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public DateOnly InvoiceDate { get; set; }

    public decimal Amount { get; set; }

    public string InvoiceStatus { get; set; } = null!;

    public virtual Contract Contract { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
