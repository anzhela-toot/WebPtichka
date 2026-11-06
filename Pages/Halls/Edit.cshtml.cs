using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Halls
{
    public class EditModel : PageModel
    {
        private readonly PtichkaContext _context;

        public EditModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Hall Hall { get; set; } = new();

        public string? Error { get; set; }

        public IActionResult OnGet(int id)
        {
            var hall = _context.Halls.Find(id);
            if (hall == null)
                return RedirectToPage("/Halls/Index");

            Hall = hall;
            return Page();
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

            var existing = _context.Halls.Find(Hall.Id);
            if (existing == null)
                return RedirectToPage("/Halls/Index");

            existing.HallName = Hall.HallName;
            existing.Capacity = Hall.Capacity;
            existing.PricePerHour = Hall.PricePerHour;
            existing.HallStatus = Hall.HallStatus;
            existing.Description = Hall.Description;

            _context.SaveChanges();

            TempData["Success"] = $"Зал «{Hall.HallName}» обновлён";
            return RedirectToPage("/Halls/Index");
        }
    }
}