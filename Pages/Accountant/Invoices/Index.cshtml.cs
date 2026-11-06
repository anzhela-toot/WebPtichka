using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Accountant.Invoices
{
    [Authorize(Roles = "accountant")]
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        public List<Invoice> Invoices { get; set; } = new();
        public int TotalCount { get; set; }

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public string? Status { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateFrom { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? DateTo { get; set; }

        public List<string> StatusesList { get; set; } = new();

        public void OnGet()
        {
            var query = _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Client)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(i =>
                    i.InvoiceNumber.Contains(s) ||
                    (i.Contract != null && i.Contract.Client != null &&
                     i.Contract.Client.FullName.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(Status))
                query = query.Where(i => i.InvoiceStatus == Status);

            if (DateFrom.HasValue)
                query = query.Where(i => i.InvoiceDate >= DateOnly.FromDateTime(DateFrom.Value));

            if (DateTo.HasValue)
                query = query.Where(i => i.InvoiceDate <= DateOnly.FromDateTime(DateTo.Value));

            TotalCount = _context.Invoices.Count();
            Invoices = query.OrderByDescending(i => i.InvoiceDate).ToList();

            StatusesList = _context.Invoices
                .Select(i => i.InvoiceStatus)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null) return NotFound();

            if (invoice.Payments.Any())
            {
                TempData["Error"] = $"Нельзя удалить счёт {invoice.InvoiceNumber} — по нему есть платежи.";
                return RedirectToPage();
            }

            _context.Invoices.Remove(invoice);
            _context.SaveChanges();
            TempData["Success"] = "Счёт удалён";
            return RedirectToPage();
        }

        public IActionResult OnPostAcceptPayment(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Contract)
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null) return NotFound();

            // Сколько уже оплачено
            var alreadyPaid = _context.Payments
                .Where(p => p.InvoiceId == id && p.PaymentStatus == "Оплачено")
                .Sum(p => (decimal?)p.Amount) ?? 0;

            var remaining = invoice.Amount - alreadyPaid;

            if (remaining <= 0)
            {
                TempData["Error"] = $"Счёт {invoice.InvoiceNumber} уже полностью оплачен.";
                return RedirectToPage();
            }

            // Создаём платёж на остаток
            var payment = new Payment
            {
                InvoiceId = id,
                Amount = remaining,
                PaymentDate = DateTime.Today,
                PaymentMethod = "Перевод",
                PaymentStatus = "Оплачено"
            };

            _context.Payments.Add(payment);

            // Обновляем статус счёта
            invoice.InvoiceStatus = "Оплачен";

            _context.SaveChanges();

            TempData["Success"] = $"Принята оплата {remaining:N2} ₽ по счёту {invoice.InvoiceNumber}.";
            return RedirectToPage();
        }
    }
}