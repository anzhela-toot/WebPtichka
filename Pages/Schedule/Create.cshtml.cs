using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPtichka.Data;

namespace WebPtichka.Pages.Schedule
{
    public class CreateModel : PageModel
    {
        private readonly PtichkaContext _context;

        public CreateModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public WebPtichka.Models.Schedule Schedule { get; set; } = new();

        public SelectList HallsList { get; set; } = default!;

        public SelectList ContractsList { get; set; } = default!;

        public void OnGet()
        {
            LoadLists();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                LoadLists();
                return Page();
            }

            if (Schedule.EndTime <= Schedule.StartTime)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Время окончания должно быть позже времени начала.");

                LoadLists();
                return Page();
            }

            // Проверка занятости зала
            var conflict = _context.Schedules.Any(s =>
                s.HallId == Schedule.HallId &&
                s.BookingDate == Schedule.BookingDate &&
                s.StartTime < Schedule.EndTime &&
                s.EndTime > Schedule.StartTime &&
                s.ScheduleStatus != "Отменено");

            if (conflict)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "На это время зал уже занят.");

                LoadLists();
                return Page();
            }

            _context.Schedules.Add(Schedule);
            _context.SaveChanges();

            return RedirectToPage("Index");
        }

        private void LoadLists()
        {
            HallsList = new SelectList(
                _context.Halls.OrderBy(h => h.HallName).ToList(),
                "Id",
                "HallName");

            ContractsList = new SelectList(
                _context.Contracts.OrderBy(c => c.ContractNumber).ToList(),
                "Id",
                "ContractNumber");
        }
    }
}