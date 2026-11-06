using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Accountant.Invoices
{
    [Authorize(Roles = "accountant")]
    public class CreateModel : PageModel
    {
        private readonly PtichkaContext _context;

        public CreateModel(PtichkaContext context)
        {
            _context = context;
        }

        public SelectList ContractsList { get; set; } = default!;

        [BindProperty] public int ContractId { get; set; }
        [BindProperty] public decimal Amount { get; set; }
        [BindProperty] public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [BindProperty] public string InvoiceStatus { get; set; } = "Выставлен";

        public string? Error { get; set; }

        public void OnGet()
        {
            LoadContracts();
        }

        public IActionResult OnPost()
        {
            LoadContracts();

            if (ContractId <= 0) { Error = "Выберите договор."; return Page(); }
            if (Amount <= 0) { Error = "Сумма должна быть больше нуля."; return Page(); }

            var contract = _context.Contracts.Find(ContractId);
            if (contract == null) { Error = "Договор не найден."; return Page(); }

            // Автогенерация номера счёта: СЧ-2026-0001
            var year = DateTime.Now.Year;
            var countThisYear = _context.Invoices.Count(i => i.InvoiceNumber.StartsWith($"СЧ-{year}-"));
            var invoiceNumber = $"СЧ-{year}-{(countThisYear + 1):D4}";

            var invoice = new Invoice
            {
                ContractId = ContractId,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = InvoiceDate,
                Amount = Amount,
                InvoiceStatus = InvoiceStatus
            };

            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            TempData["Success"] = $"Счёт {invoiceNumber} создан";
            return RedirectToPage("/Accountant/Invoices/Index");
        }

        private void LoadContracts()
        {
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .OrderByDescending(c => c.Id)
                .ToList();

            ContractsList = new SelectList(
                contracts.Select(c => new
                {
                    c.Id,
                    Display = $"{c.ContractNumber} — {c.Client?.FullName} — {c.TotalAmount:N2} ₽"
                }),
                "Id", "Display");
        }
    }
}