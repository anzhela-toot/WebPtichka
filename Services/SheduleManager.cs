using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Services;

public class ScheduleManager
{
    private readonly PtichkaContext _context;

    public ScheduleManager(PtichkaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Проверка: свободен ли зал в указанную дату и время.
    /// </summary>
    public bool IsHallFree(int hallId, DateOnly date, TimeOnly start, TimeOnly end)
    {
        return !_context.Schedules.Any(s =>
            s.HallId == hallId &&
            s.BookingDate == date &&
            s.ScheduleStatus != "Cancelled" &&
            s.StartTime < end &&
            s.EndTime > start);
    }

    /// <summary>
    /// Проверка: свободен ли сотрудник (через Contract.EmployeeId).
    /// </summary>
    public bool IsEmployeeFree(int employeeId, DateOnly date, TimeOnly start, TimeOnly end)
    {
        return !_context.Schedules
            .Include(s => s.Contract)
            .Any(s =>
                s.Contract.EmployeeId == employeeId &&
                s.BookingDate == date &&
                s.ScheduleStatus != "Cancelled" &&
                s.StartTime < end &&
                s.EndTime > start);
    }

    /// <summary>
    /// Создаёт бронь. Возвращает (успех, сообщение).
    /// </summary>
    public (bool ok, string message) Book(int contractId, int hallId,
                                          DateOnly date, TimeOnly start, TimeOnly end)
    {
        if (end <= start)
            return (false, "Время окончания должно быть позже начала.");

        if (!IsHallFree(hallId, date, start, end))
            return (false, "Зал занят на это время.");

        var schedule = new Schedule
        {
            ContractId = contractId,
            HallId = hallId,
            BookingDate = date,
            StartTime = start,
            EndTime = end,
            ScheduleStatus = "Booked"
        };

        _context.Schedules.Add(schedule);
        _context.SaveChanges();

        return (true, "Бронь создана.");
    }

    /// <summary>
    /// Отменяет бронь (мягкое удаление — меняем статус).
    /// </summary>
    public bool Cancel(int scheduleId)
    {
        var s = _context.Schedules.Find(scheduleId);
        if (s == null) return false;

        s.ScheduleStatus = "Cancelled";
        _context.SaveChanges();
        return true;
    }

    /// <summary>
    /// Съёмки на сегодня (с подгрузкой зала, договора и клиента).
    /// </summary>
    public List<Schedule> GetToday()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        return _context.Schedules
            .Include(s => s.Hall)
            .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
            .Include(s => s.Contract)
                .ThenInclude(c => c.Employee)
            .Where(s => s.BookingDate == today &&
                        s.ScheduleStatus != "Cancelled")
            .OrderBy(s => s.StartTime)
            .ToList();
    }

    /// <summary>
    /// Съёмки конкретного сотрудника за период.
    /// </summary>
    public List<Schedule> GetForEmployee(int employeeId, DateOnly from, DateOnly to)
    {
        return _context.Schedules
            .Include(s => s.Hall)
            .Include(s => s.Contract)
                .ThenInclude(c => c.Client)
            .Where(s => s.Contract.EmployeeId == employeeId &&
                        s.BookingDate >= from &&
                        s.BookingDate <= to &&
                        s.ScheduleStatus != "Cancelled")
            .OrderBy(s => s.BookingDate)
            .ThenBy(s => s.StartTime)
            .ToList();
    }
}