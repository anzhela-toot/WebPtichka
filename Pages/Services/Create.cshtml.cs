using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Services
{
    public class CreateModel : PageModel
    {
        private readonly PtichkaContext _context;

        public CreateModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Service Service { get; set; } = new();

        public string? Error { get; set; }

        public void OnGet()
        {
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

            _context.Services.Add(Service);
            _context.SaveChanges();

            TempData["Success"] = $"Услуга «{Service.ServiceName}» добавлена";
            return RedirectToPage("/Services/Index");
        }
    }
}