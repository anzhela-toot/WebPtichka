using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;
using ScheduleModel = WebPtichka.Models.Schedule;

namespace WebPtichka.Pages.Schedule
{
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        public List<ScheduleModel> SchedulesList { get; set; } = new();
        public int TotalCount { get; set; }

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public int? HallId { get; set; }
        [BindProperty(SupportsGet = true)] public string? Status { get; set; }
        [BindProperty(SupportsGet = true)] public DateOnly? DateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateOnly? DateTo { get; set; }

        [BindProperty(SupportsGet = true)] public DateOnly? WeekStart { get; set; }
        public DateOnly CurrentWeekStart { get; set; }
        public List<ScheduleModel> WeekSchedules { get; set; } = new();

        public SelectList HallsList { get; set; } = default!;
        public List<string> StatusesList { get; set; } = new();

        public void OnGet()
        {
            // 1. Логика для списка
            var query = _context.Schedules
                .Include(s => s.Hall)
                .Include(s => s.Contract)
                    .ThenInclude(c => c.Client)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(x =>
                    (x.Contract != null && x.Contract.ContractNumber.Contains(s)) ||
                    (x.Contract != null && x.Contract.Client != null &&
                     x.Contract.Client.FullName.Contains(s)));
            }

            if (HallId.HasValue && HallId.Value > 0)
                query = query.Where(s => s.HallId == HallId.Value);

            if (!string.IsNullOrWhiteSpace(Status))
                query = query.Where(s => s.ScheduleStatus == Status);

            if (DateFrom.HasValue)
                query = query.Where(s => s.BookingDate >= DateFrom.Value);

            if (DateTo.HasValue)
                query = query.Where(s => s.BookingDate <= DateTo.Value);

            TotalCount = _context.Schedules.Count();
            SchedulesList = query.OrderBy(s => s.BookingDate).ThenBy(s => s.StartTime).ToList();

            HallsList = new SelectList(_context.Halls.OrderBy(h => h.HallName).ToList(), "Id", "HallName", HallId);
            StatusesList = _context.Schedules.Select(s => s.ScheduleStatus).Distinct().OrderBy(s => s).ToList();

            // 2. Логика для календаря
            if (!WeekStart.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                WeekStart = today.AddDays(-1 * diff);
            }

            CurrentWeekStart = WeekStart.Value;
            var weekEnd = CurrentWeekStart.AddDays(7);

            var weekQuery = _context.Schedules
                .Include(s => s.Hall)
                .Include(s => s.Contract)
                    .ThenInclude(c => c.Client)
                .Where(s => s.BookingDate >= CurrentWeekStart && s.BookingDate < weekEnd);

            if (HallId.HasValue && HallId.Value > 0)
                weekQuery = weekQuery.Where(s => s.HallId == HallId.Value);

            WeekSchedules = weekQuery.OrderBy(s => s.StartTime).ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var schedule = _context.Schedules.Find(id);
            if (schedule == null) return NotFound();

            _context.Schedules.Remove(schedule);
            _context.SaveChanges();
            TempData["Success"] = "Бронирование удалено";

            return RedirectToPage(new
            {
                search = Search,
                hallId = HallId,
                status = Status,
                dateFrom = DateFrom?.ToString("yyyy-MM-dd"),
                dateTo = DateTo?.ToString("yyyy-MM-dd"),
                weekStart = WeekStart?.ToString("yyyy-MM-dd")
            });
        }

        // Класс для приема данных от JavaScript (Drag-and-Drop)
        public class UpdateScheduleRequest
        {
            public int Id { get; set; }
            public string NewDate { get; set; } = "";
            public int NewHour { get; set; }
        }

        // Метод обновления при перетаскивании
        public IActionResult OnPostUpdateSchedule([FromBody] UpdateScheduleRequest request)
        {
            var schedule = _context.Schedules.Find(request.Id);
            if (schedule == null) return NotFound();

            if (DateOnly.TryParse(request.NewDate, out var newDate))
            {
                schedule.BookingDate = newDate;
            }

            var duration = schedule.EndTime - schedule.StartTime;
            schedule.StartTime = new TimeOnly(request.NewHour, 0);
            schedule.EndTime = schedule.StartTime.Add(duration);

            var conflict = _context.Schedules.Any(s =>
                s.Id != schedule.Id &&
                s.HallId == schedule.HallId &&
                s.BookingDate == schedule.BookingDate &&
                s.StartTime < schedule.EndTime &&
                s.EndTime > schedule.StartTime &&
                s.ScheduleStatus != "Отменено");

            if (conflict)
            {
                return BadRequest("Это время уже занято другим бронированием!");
            }

            _context.SaveChanges();
            return new JsonResult(new { success = true });
        }
    }
}