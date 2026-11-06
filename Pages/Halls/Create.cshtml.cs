using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Halls
{
    public class CreateModel : PageModel
    {
        private readonly PtichkaContext _context;

        public CreateModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Hall Hall { get; set; } = new();

        public string? Error { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Hall.HallName))
            {
                Error = "Введите название зала.";
                return Page();
            }

            if (Hall.Capacity <= 0)
            {
                Error = "Вместимость должна быть больше нуля.";
                return Page();
            }

            if (Hall.PricePerHour <= 0)
            {
                Error = "Цена за час должна быть больше нуля.";
                return Page();
            }

            _context.Halls.Add(Hall);
            _context.SaveChanges();

            TempData["Success"] = $"Зал «{Hall.HallName}» добавлен";
            return RedirectToPage("/Halls/Index");
        }
    }
}