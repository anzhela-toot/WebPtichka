using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Services
{
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        public List<Service> Services { get; set; } = new();
        public int TotalCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PriceMin { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? PriceMax { get; set; }

        public void OnGet()
        {
            var query = _context.Services.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(x =>
                    x.ServiceName.Contains(s) ||
                    (x.Description != null && x.Description.Contains(s)));
            }

            if (PriceMin.HasValue)
                query = query.Where(x => x.ServicePrice >= PriceMin.Value);

            if (PriceMax.HasValue)
                query = query.Where(x => x.ServicePrice <= PriceMax.Value);

            TotalCount = _context.Services.Count();
            Services = query.OrderBy(x => x.ServiceName).ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var service = _context.Services.Find(id);
            if (service == null) return NotFound();

            // Проверка — есть ли услуга в договорах
            var usedInContracts = _context.ContractServices.Any(cs => cs.ServiceId == id);
            if (usedInContracts)
            {
                TempData["Error"] = $"Нельзя удалить услугу «{service.ServiceName}» — она используется в договорах.";
                return RedirectToPage();
            }

            _context.Services.Remove(service);
            _context.SaveChanges();
            TempData["Success"] = "Услуга удалена";
            return RedirectToPage();
        }
    }
}