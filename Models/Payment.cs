using System;
using System.Collections.Generic;

namespace WebPtichka.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string PaymentStatus { get; set; } = null!;

    public virtual Invoice Invoice { get; set; } = null!;
}
