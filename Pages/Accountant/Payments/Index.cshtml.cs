using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Accountant.Payments
{
    [Authorize(Roles = "accountant")]
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        public List<Payment> Payments { get; set; } = new();
        public int TotalCount { get; set; }

        // Параметры поиска и фильтров
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Method { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateTo { get; set; }

        // Для выпадающих списков
        public List<string> MethodsList { get; set; } = new();
        public List<string> StatusesList { get; set; } = new();

        public void OnGet()
        {
            var query = _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Contract)
                        .ThenInclude(c => c.Client)
                .AsQueryable();

            // Поиск по номеру счёта / ФИО клиента
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(p =>
                    (p.Invoice != null && p.Invoice.InvoiceNumber.Contains(s)) ||
                    (p.Invoice != null && p.Invoice.Contract != null &&
                     p.Invoice.Contract.Client != null &&
                     p.Invoice.Contract.Client.FullName.Contains(s)));
            }

            // Фильтр по способу оплаты
            if (!string.IsNullOrWhiteSpace(Method))
                query = query.Where(p => p.PaymentMethod == Method);

            // Фильтр по статусу
            if (!string.IsNullOrWhiteSpace(Status))
                query = query.Where(p => p.PaymentStatus == Status);

            // Фильтр по дате
            if (DateFrom.HasValue)
                query = query.Where(p => p.PaymentDate >= DateFrom.Value);

            if (DateTo.HasValue)
                query = query.Where(p => p.PaymentDate <= DateTo.Value);

            TotalCount = _context.Payments.Count();
            Payments = query.OrderByDescending(p => p.PaymentDate).ToList();

            MethodsList = _context.Payments
                .Select(p => p.PaymentMethod)
                .Distinct()
                .OrderBy(m => m)
                .ToList();

            StatusesList = _context.Payments
                .Select(p => p.PaymentStatus)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var payment = _context.Payments.Find(id);
            if (payment == null) return NotFound();

            _context.Payments.Remove(payment);
            _context.SaveChanges();
            TempData["Success"] = "Платёж удалён";

            return RedirectToPage(new
            {
                search = Search,
                method = Method,
                status = Status,
                dateFrom = DateFrom?.ToString("yyyy-MM-dd"),
                dateTo = DateTo?.ToString("yyyy-MM-dd")
            });
        }
    }
}