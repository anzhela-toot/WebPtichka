using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Contracts
{
    public class IndexModel : PageModel
    {
        private readonly PtichkaContext _context;

        public IndexModel(PtichkaContext context)
        {
            _context = context;
        }

        public List<Contract> Contracts { get; set; } = new();
        public int TotalCount { get; set; }

        // Параметры поиска и фильтров (из URL)
        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? HallId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateOnly? DateTo { get; set; }

        // Для выпадающих списков
        public SelectList HallsList { get; set; } = default!;
        public List<string> StatusesList { get; set; } = new();

        public void OnGet()
        {
            var query = _context.Contracts
                .Include(c => c.Client)
                .Include(c => c.Hall)
                .Include(c => c.Employee)
                .AsQueryable();

            // Поиск по номеру договора + ФИО клиента
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(c =>
                    c.ContractNumber.Contains(s) ||
                    (c.Client != null && c.Client.FullName.Contains(s)));
            }

            // Фильтр по залу
            if (HallId.HasValue && HallId.Value > 0)
                query = query.Where(c => c.HallId == HallId.Value);

            // Фильтр по статусу
            if (!string.IsNullOrWhiteSpace(Status))
                query = query.Where(c => c.ContractStatus == Status);

            // Фильтр по дате мероприятия
            if (DateFrom.HasValue)
                query = query.Where(c => c.EventDate >= DateFrom.Value);

            if (DateTo.HasValue)
                query = query.Where(c => c.EventDate <= DateTo.Value);

            TotalCount = _context.Contracts.Count();

            Contracts = query.OrderByDescending(c => c.EventDate).ToList();

            // Списки для фильтров
            HallsList = new SelectList(
                _context.Halls.OrderBy(h => h.HallName).ToList(),
                "Id", "HallName", HallId);

            StatusesList = _context.Contracts
                .Select(c => c.ContractStatus)
                .Distinct()
                .OrderBy(s => s)
                .ToList();
        }

        public IActionResult OnPostDelete(int id)
        {
            var contract = _context.Contracts.Find(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
                _context.SaveChanges();
            }
            return RedirectToPage(new
            {
                search = Search,
                hallId = HallId,
                status = Status,
                dateFrom = DateFrom?.ToString("yyyy-MM-dd"),
                dateTo = DateTo?.ToString("yyyy-MM-dd")
            });
        }
    }
}