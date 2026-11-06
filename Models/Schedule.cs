using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WebPtichka.Models;

public partial class Schedule
{
    public int Id { get; set; }
    public int ContractId { get; set; }
    public int HallId { get; set; }

    public DateOnly BookingDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string ScheduleStatus { get; set; } = null!;

    [ValidateNever]
    public virtual Contract Contract { get; set; } = null!;

    [ValidateNever]
    public virtual Hall Hall { get; set; } = null!;
}
