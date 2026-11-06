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
    public class EditModel : PageModel
    {
        private readonly PtichkaContext _context;

        public EditModel(PtichkaContext context)
        {
            _context = context;
        }

        public SelectList ContractsList { get; set; } = default!;

        [BindProperty] public int Id { get; set; }
        [BindProperty] public int ContractId { get; set; }
        [BindProperty] public decimal Amount { get; set; }
        [BindProperty] public DateOnly InvoiceDate { get; set; }
        [BindProperty] public string InvoiceStatus { get; set; } = "";

        public string? Error { get; set; }

        public IActionResult OnGet(int id)
        {
            var invoice = _context.Invoices.Find(id);
            if (invoice == null)
                return RedirectToPage("/Accountant/Invoices/Index");

            Id = invoice.Id;
            ContractId = invoice.ContractId;
            Amount = invoice.Amount;
            InvoiceDate = invoice.InvoiceDate;
            InvoiceStatus = invoice.InvoiceStatus;

            LoadContracts();
            return Page();
        }

        public IActionResult OnPost()
        {
            LoadContracts();

            if (ContractId <= 0) { Error = "Выберите договор."; return Page(); }
            if (Amount <= 0) { Error = "Сумма должна быть больше нуля."; return Page(); }

            var invoice = _context.Invoices.Find(Id);
            if (invoice == null)
                return RedirectToPage("/Accountant/Invoices/Index");

            invoice.ContractId = ContractId;
            invoice.Amount = Amount;
            invoice.InvoiceDate = InvoiceDate;
            invoice.InvoiceStatus = InvoiceStatus;

            _context.SaveChanges();

            TempData["Success"] = "Счёт обновлён";
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