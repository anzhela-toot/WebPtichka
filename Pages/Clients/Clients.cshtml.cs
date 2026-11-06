using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Clients
{
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        public List<Client> Clients { get; set; } = new();
        public int TotalCount { get; set; }

        // Параметры поиска (привязываются из URL)
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly? DateTo { get; set; }

        public void OnGet()
        {
            var query = _context.Clients.AsQueryable();

            // Поиск по ФИО / телефону / email
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(c =>
                    c.FullName.Contains(s) ||
                    c.Phone.Contains(s) ||
                    (c.Email != null && c.Email.Contains(s)));
            }

            // Фильтр по дате регистрации
            if (DateFrom.HasValue)
                query = query.Where(c => c.RegistrationDate >= DateFrom.Value);

            if (DateTo.HasValue)
                query = query.Where(c => c.RegistrationDate <= DateTo.Value);

            TotalCount = _context.Clients.Count();

            Clients = query.OrderBy(c => c.Id).ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var client = _context.Clients.Find(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                _context.SaveChanges();
            }
            return RedirectToPage(new
            {
                search = Search,
                dateFrom = DateFrom?.ToString("yyyy-MM-dd"),
                dateTo = DateTo?.ToString("yyyy-MM-dd")
            });
        }
    }
}