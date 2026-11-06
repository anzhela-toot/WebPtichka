using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;
using ScheduleModel = WebPtichka.Models.Schedule;

namespace WebPtichka.Pages.Accountant
{
    [Authorize(Roles = "accountant")]
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        // Виджеты
        public decimal MonthIncome { get; set; }
        public int MonthPaymentsCount { get; set; }
        public decimal TotalUnpaid { get; set; }
        public int UnpaidInvoicesCount { get; set; }
        public int MonthInvoicesCount { get; set; }
        public int OverdueCount { get; set; }

        // Таблицы
        public List<Payment> RecentPayments { get; set; } = new();
        public List<ScheduleModel> UpcomingBookings { get; set; } = new();

        public void OnGet()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var monthStartDate = DateOnly.FromDateTime(monthStart);

            // Доход за текущий месяц
            var monthPayments = _context.Payments
                .Where(p => p.PaymentDate >= monthStart && p.PaymentDate <= today)
                .ToList();

            MonthIncome = monthPayments.Sum(p => p.Amount);
            MonthPaymentsCount = monthPayments.Count;

            // Выставлено счетов за месяц
            MonthInvoicesCount = _context.Invoices
                .Count(i => i.InvoiceDate >= monthStartDate);

            // Ожидает оплаты (все неоплаченные)
            var unpaid = _context.Invoices
                .Where(i => i.InvoiceStatus != "Оплачен" && i.InvoiceStatus != "Отменён")
                .ToList();

            TotalUnpaid = unpaid.Sum(i => i.Amount);
            UnpaidInvoicesCount = unpaid.Count;

            // Просроченные (старше 30 дней)
            var overdueDate = DateOnly.FromDateTime(today.AddDays(-30));
            OverdueCount = _context.Invoices
                .Count(i => i.InvoiceStatus != "Оплачен"
                         && i.InvoiceStatus != "Отменён"
                         && i.InvoiceDate < overdueDate);

            // Последние 5 платежей
            RecentPayments = _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Contract)
                        .ThenInclude(c => c.Client)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.Id)
                .Take(5)
                .ToList();

            // Ближайшие 7 дней — брони
            var todayDate = DateOnly.FromDateTime(today);
            var weekLater = todayDate.AddDays(7);

            UpcomingBookings = _context.Schedules
                .Include(s => s.Hall)
                .Where(s => s.BookingDate >= todayDate
                         && s.BookingDate <= weekLater
                         && s.ScheduleStatus != "Отменено")
                .OrderBy(s => s.BookingDate)
                .ThenBy(s => s.StartTime)
                .Take(5)
                .ToList();
        }
    }
}