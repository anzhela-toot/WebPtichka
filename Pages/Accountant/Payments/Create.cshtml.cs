using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Accountant.Payments
{
    [Authorize(Roles = "accountant")]
    public class CreateModel : PageModel
    {
        private readonly PtichkaContext _context;

        public CreateModel(PtichkaContext context)
        {
            _context = context;
        }

        public SelectList InvoicesList { get; set; } = default!;

        [BindProperty] public int InvoiceId { get; set; }
        [BindProperty] public decimal Amount { get; set; }
        [BindProperty] public DateTime PaymentDate { get; set; } = DateTime.Today;
        [BindProperty] public string PaymentMethod { get; set; } = "Перевод";
        [BindProperty] public string PaymentStatus { get; set; } = "Оплачено";

        public string? Error { get; set; }

        public void OnGet()
        {
            LoadInvoices();
        }

        public IActionResult OnPost()
        {
            LoadInvoices();

            if (InvoiceId <= 0) { Error = "Выберите счёт."; return Page(); }
            if (Amount <= 0) { Error = "Сумма должна быть больше нуля."; return Page(); }

            var invoice = _context.Invoices.Find(InvoiceId);
            if (invoice == null) { Error = "Счёт не найден."; return Page(); }

            var payment = new Payment
            {
                InvoiceId = InvoiceId,
                Amount = Amount,
                PaymentDate = PaymentDate,
                PaymentMethod = PaymentMethod,
                PaymentStatus = PaymentStatus
            };

            _context.Payments.Add(payment);

            // Обновляем статус счёта, если платёж "Оплачено"
            if (PaymentStatus == "Оплачено")
            {
                var totalPaid = _context.Payments
                    .Where(p => p.InvoiceId == InvoiceId && p.PaymentStatus == "Оплачено")
                    .Sum(p => (decimal?)p.Amount) ?? 0;

                if (totalPaid + Amount >= invoice.Amount)
                    invoice.InvoiceStatus = "Оплачен";
                else
                    invoice.InvoiceStatus = "Частично оплачен";
            }

            _context.SaveChanges();

            TempData["Success"] = "Платёж добавлен";
            return RedirectToPage("/Accountant/Payments/Index");
        }

        private void LoadInvoices()
        {
            var invoices = _context.Invoices
                .Include(i => i.Contract)
                    .ThenInclude(c => c.Client)
                .OrderByDescending(i => i.Id)
                .ToList();

            InvoicesList = new SelectList(
                invoices.Select(i => new
                {
                    i.Id,
                    Display = $"{i.InvoiceNumber} — {i.Contract?.Client?.FullName} — {i.Amount:N2} ₽"
                }),
                "Id", "Display");
        }
    }
}