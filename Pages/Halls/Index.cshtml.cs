using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Halls
{
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        public List<Hall> Halls { get; set; } = new();
        public int TotalCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CapacityMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? CapacityMax { get; set; }

        public List<string> StatusesList { get; set; } = new();

        public void OnGet()
        {
            var query = _context.Halls.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(h =>
                    h.HallName.Contains(s) ||
                    (h.Description != null && h.Description.Contains(s)));
            }

            if (!string.IsNullOrWhiteSpace(Status))
                query = query.Where(h => h.HallStatus == Status);

            if (CapacityMin.HasValue)
                query = query.Where(h => h.Capacity >= CapacityMin.Value);

            if (CapacityMax.HasValue)
                query = query.Where(h => h.Capacity <= CapacityMax.Value);

            TotalCount = _context.Halls.Count();
            Halls = query.OrderBy(h => h.Id).ToList();

            StatusesList = _context.Halls
                .Select(h => h.HallStatus)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var hall = _context.Halls.Find(id);
            if (hall == null) return NotFound();

            // Проверяем, есть ли договоры с этим залом
            var hasContracts = _context.Contracts.Any(c => c.HallId == id);
            if (hasContracts)
            {
                TempData["Error"] = $"Нельзя удалить зал «{hall.HallName}» — с ним связаны договоры.";
                return RedirectToPage();
            }

            _context.Halls.Remove(hall);
            _context.SaveChanges();
            TempData["Success"] = "Зал удалён";
            return RedirectToPage();
        }
    }
}