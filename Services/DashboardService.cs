using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Services;

public class DashboardService
{
    private readonly PtichkaContext _context;

    public DashboardService(PtichkaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Съёмки на сегодня.
    /// </summary>
    public List<Schedule> TodaySchedules()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return _context.Schedules
            .Include(s => s.Hall)
            .Include(s => s.Contract).ThenInclude(c => c.Client)
            .Include(s => s.Contract).ThenInclude(c => c.Employee)
            .Where(s => s.BookingDate == today &&
                        s.ScheduleStatus != "Cancelled")
            .OrderBy(s => s.StartTime)
            .ToList();
    }

    /// <summary>
    /// Неоплаченные счета.
    /// </summary>
    public List<Invoice> UnpaidInvoices()
    {
        return _context.Invoices
            .Include(i => i.Contract).ThenInclude(c => c.Client)
            .Where(i => i.InvoiceStatus != "Paid")
            .OrderBy(i => i.Id)
            .ToList();
    }

    /// <summary>
    /// Свободные залы прямо сейчас.
    /// </summary>
    public List<Hall> FreeHallsNow()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var now = TimeOnly.FromDateTime(DateTime.Now);

        var busyHallIds = _context.Schedules
            .Where(s => s.BookingDate == today &&
                        s.ScheduleStatus != "Cancelled" &&
                        s.StartTime <= now && s.EndTime > now)
            .Select(s => s.HallId)
            .ToList();

        return _context.Halls
            .Where(h => !busyHallIds.Contains(h.Id))
            .ToList();
    }
}