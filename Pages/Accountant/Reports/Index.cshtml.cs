using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Accountant.Reports
{
    [Authorize(Roles = "accountant")]
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }

        public decimal TotalIncome { get; set; }
        public int PaymentsCount { get; set; }
        public decimal TotalIssued { get; set; }
        public int InvoicesCount { get; set; }
        public decimal TotalUnpaid { get; set; }
        public int UnpaidInvoicesCount { get; set; }

        public List<ClientStats> ClientStatsList { get; set; } = new();
        public List<MethodStats> MethodStatsList { get; set; } = new();
        public List<Payment> RecentPayments { get; set; } = new();
        public List<Invoice> OverdueInvoices { get; set; } = new();

        public void OnGet()
        {
            if (!DateFrom.HasValue && !DateTo.HasValue)
            {
                var today = DateTime.Today;
                DateFrom = new DateTime(today.Year, today.Month, 1);
                DateTo = today;
            }

            var from = DateOnly.FromDateTime(DateFrom ?? DateTime.MinValue);
            var to = DateOnly.FromDateTime(DateTo ?? DateTime.MaxValue);

            // Сводка за период
            var paymentsInPeriod = _context.Payments
                .Where(p => p.PaymentDate >= (DateFrom ?? DateTime.MinValue)
                         && p.PaymentDate <= (DateTo ?? DateTime.MaxValue))
                .ToList();

            TotalIncome = paymentsInPeriod.Sum(p => p.Amount);
            PaymentsCount = paymentsInPeriod.Count;

            var invoicesInPeriod = _context.Invoices
                .Where(i => i.InvoiceDate >= from && i.InvoiceDate <= to)
                .ToList();

            TotalIssued = invoicesInPeriod.Sum(i => i.Amount);
            InvoicesCount = invoicesInPeriod.Count;

            // Неоплаченные (за всё время)
            var unpaid = _context.Invoices
                .Where(i => i.InvoiceStatus != "Оплачен" && i.InvoiceStatus != "Отменён")
                .ToList();

            TotalUnpaid = unpaid.Sum(i => i.Amount);
            UnpaidInvoicesCount = unpaid.Count;

            // Разбивка по клиентам
            var clients = _context.Clients
                .Select(c => new
                {
                    c.Id,
                    c.FullName,
                    ContractsCount = c.Contracts.Count,
                    PaidAmount = c.Contracts
                        .SelectMany(ct => ct.Invoices)
                        .SelectMany(i => i.Payments)
                        .Where(p => p.PaymentDate >= (DateFrom ?? DateTime.MinValue)
                                 && p.PaymentDate <= (DateTo ?? DateTime.MaxValue))
                        .Sum(p => (decimal?)p.Amount) ?? 0,
                    DebtAmount = c.Contracts
                        .SelectMany(ct => ct.Invoices)
                        .Where(i => i.InvoiceStatus != "Оплачен" && i.InvoiceStatus != "Отменён")
                        .Sum(i => (decimal?)i.Amount) ?? 0
                })
                .ToList();

            ClientStatsList = clients
                .Where(c => c.PaidAmount > 0 || c.DebtAmount > 0)
                .OrderByDescending(c => c.PaidAmount)
                .Select(c => new ClientStats
                {
                    ClientId = c.Id,
                    FullName = c.FullName,
                    ContractsCount = c.ContractsCount,
                    PaidAmount = c.PaidAmount,
                    DebtAmount = c.DebtAmount
                })
                .ToList();

            // Разбивка по способам оплаты
            MethodStatsList = paymentsInPeriod
                .GroupBy(p => p.PaymentMethod ?? "Не указан")
                .Select(g => new MethodStats
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(p => p.Amount)
                })
                .OrderByDescending(m => m.Amount)
                .ToList();

            // Последние 10 платежей
            RecentPayments = _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Contract)
                        .ThenInclude(c => c.Client)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.Id)
                .Take(10)
                .ToList();

            // Просроченные счета (старше 30 дней)
            var overdueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            OverdueInvoices = _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Client)
                .Where(i => i.InvoiceStatus != "Оплачен"
                         && i.InvoiceStatus != "Отменён"
                         && i.InvoiceDate < overdueDate)
                .OrderBy(i => i.InvoiceDate)
                .ToList();
        }

        public class ClientStats
        {
            public int ClientId { get; set; }
            public string FullName { get; set; } = "";
            public int ContractsCount { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal DebtAmount { get; set; }
        }

        public class MethodStats
        {
            public string Method { get; set; } = "";
            public int Count { get; set; }
            public decimal Amount { get; set; }
        }
    }
}