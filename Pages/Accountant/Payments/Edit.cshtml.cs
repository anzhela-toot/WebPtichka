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
    public class EditModel : PageModel
    {
        private readonly PtichkaContext _context;

        public EditModel(PtichkaContext context)
        {
            _context = context;
        }

        public SelectList InvoicesList { get; set; } = default!;

        [BindProperty] public int Id { get; set; }
        [BindProperty] public int InvoiceId { get; set; }
        [BindProperty] public decimal Amount { get; set; }
        [BindProperty] public DateTime PaymentDate { get; set; }
        [BindProperty] public string PaymentMethod { get; set; } = "";
        [BindProperty] public string PaymentStatus { get; set; } = "";

        public string? Error { get; set; }

        public IActionResult OnGet(int id)
        {
            var payment = _context.Payments.Find(id);
            if (payment == null)
                return RedirectToPage("/Accountant/Payments/Index");

            Id = payment.Id;
            InvoiceId = payment.InvoiceId;
            Amount = payment.Amount;
            PaymentDate = payment.PaymentDate;
            PaymentMethod = payment.PaymentMethod;
            PaymentStatus = payment.PaymentStatus;

            LoadInvoices();
            return Page();
        }

        public IActionResult OnPost()
        {
            LoadInvoices();

            if (InvoiceId <= 0) { Error = "Выберите счёт."; return Page(); }
            if (Amount <= 0) { Error = "Сумма должна быть больше нуля."; return Page(); }

            var payment = _context.Payments.Find(Id);
            if (payment == null)
                return RedirectToPage("/Accountant/Payments/Index");

            var invoice = _context.Invoices.Find(InvoiceId);
            if (invoice == null) { Error = "Счёт не найден."; return Page(); }

            payment.InvoiceId = InvoiceId;
            payment.Amount = Amount;
            payment.PaymentDate = PaymentDate;
            payment.PaymentMethod = PaymentMethod;
            payment.PaymentStatus = PaymentStatus;

            _context.SaveChanges();

            // Пересчитываем статус счёта
            RecalculateInvoiceStatus(invoice.Id);
            _context.SaveChanges();

            TempData["Success"] = "Платёж обновлён";
            return RedirectToPage("/Accountant/Payments/Index");
        }

        private void RecalculateInvoiceStatus(int invoiceId)
        {
            var invoice = _context.Invoices.Find(invoiceId);
            if (invoice == null) return;

            var totalPaid = _context.Payments
                .Where(p => p.InvoiceId == invoiceId && p.PaymentStatus == "Оплачено")
                .Sum(p => (decimal?)p.Amount) ?? 0;

            if (totalPaid == 0)
                invoice.InvoiceStatus = "Выставлен";
            else if (totalPaid >= invoice.Amount)
                invoice.InvoiceStatus = "Оплачен";
            else
                invoice.InvoiceStatus = "Частично оплачен";
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