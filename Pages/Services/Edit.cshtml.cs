using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Services
{
    public class EditModel : PageModel
    {
        private readonly PtichkaContext _context;

        public EditModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Service Service { get; set; } = new();

        public string? Error { get; set; }

        public IActionResult OnGet(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null)
                return RedirectToPage("/Services/Index");

            Service = service;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Service.ServiceName))
            {
                Error = "Введите название услуги.";
                return Page();
            }

            if (Service.ServicePrice < 0)
            {
                Error = "Цена не может быть отрицательной.";
                return Page();
            }

            var existing = _context.Services.Find(Service.Id);
            if (existing == null)
                return RedirectToPage("/Services/Index");

            existing.ServiceName = Service.ServiceName;
            existing.Description = Service.Description;
            existing.ServicePrice = Service.ServicePrice;

            _context.SaveChanges();

            TempData["Success"] = $"Услуга «{Service.ServiceName}» обновлена";
            return RedirectToPage("/Services/Index");
        }
    }
}