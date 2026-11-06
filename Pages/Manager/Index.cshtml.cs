using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;
using ScheduleModel = WebPtichka.Models.Schedule;

namespace WebPtichka.Pages.Manager
{
    [Authorize(Roles = "manager")]
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        // Виджеты
        public int TotalClients { get; set; }
        public int MonthContractsCount { get; set; }
        public int WeekBookingsCount { get; set; }
        public int ActiveContractsCount { get; set; }

        // Таблицы
        public List<ScheduleModel> UpcomingBookings { get; set; } = new();
        public List<Contract> RecentContracts { get; set; } = new();

        public void OnGet()
        {
            var today = DateTime.Today;
            var todayDate = DateOnly.FromDateTime(today);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var weekLater = todayDate.AddDays(7);

            // Всего клиентов
            TotalClients = _context.Clients.Count();

            // Договоров оформлено в этом месяце
            MonthContractsCount = _context.Contracts
                .Count(c => c.CreatedDate >= monthStart && c.CreatedDate <= today);

            // Событий на ближайшую неделю (брони)
            WeekBookingsCount = _context.Schedules
                .Count(s => s.BookingDate >= todayDate
                         && s.BookingDate <= weekLater
                         && s.ScheduleStatus != "Отменено");

            // Активных договоров
            ActiveContractsCount = _context.Contracts
                .Count(c => c.ContractStatus == "Активен");

            // Ближайшие 5 бронирований
            UpcomingBookings = _context.Schedules
                .Include(s => s.Hall)
                .Include(s => s.Contract)
                    .ThenInclude(c => c.Client)
                .Where(s => s.BookingDate >= todayDate
                         && s.ScheduleStatus != "Отменено")
                .OrderBy(s => s.BookingDate)
                .ThenBy(s => s.StartTime)
                .Take(5)
                .ToList();

            // Последние 5 договоров
            RecentContracts = _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Hall)
                .OrderByDescending(c => c.CreatedDate)
                .ThenByDescending(c => c.Id)
                .Take(5)
                .ToList();
        }
    }
}