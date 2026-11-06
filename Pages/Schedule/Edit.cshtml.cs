using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPtichka.Data;
using ScheduleModel = WebPtichka.Models.Schedule;

namespace WebPtichka.Pages.Schedule
{
    public class EditModel : PageModel
    {
        private readonly PtichkaContext _context;

        public EditModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ScheduleModel Schedule { get; set; } = new();

        public SelectList HallsList { get; set; } = default!;
        public SelectList ContractsList { get; set; } = default!;

        public string? Error { get; set; }

        public IActionResult OnGet(int id)
        {
            var schedule = _context.Schedules.Find(id);
            if (schedule == null)
                return RedirectToPage("/Schedule/Index");

            Schedule = schedule;
            LoadLists();
            return Page();
        }

        public IActionResult OnPost()
        {
            LoadLists();

            if (Schedule.EndTime <= Schedule.StartTime)
            {
                Error = "Время окончания должно быть позже времени начала.";
                return Page();
            }

            var existing = _context.Schedules.Find(Schedule.Id);
            if (existing == null)
                return RedirectToPage("/Schedule/Index");

            // Проверка занятости зала (исключая эту запись)
            var conflict = _context.Schedules.Any(s =>
                s.Id != Schedule.Id &&
                s.HallId == Schedule.HallId &&
                s.BookingDate == Schedule.BookingDate &&
                s.StartTime < Schedule.EndTime &&
                s.EndTime > Schedule.StartTime &&
                s.ScheduleStatus != "Отменено");

            if (conflict)
            {
                Error = "На это время зал уже занят.";
                return Page();
            }

            existing.BookingDate = Schedule.BookingDate;
            existing.StartTime = Schedule.StartTime;
            existing.EndTime = Schedule.EndTime;
            existing.HallId = Schedule.HallId;
            existing.ContractId = Schedule.ContractId;
            existing.ScheduleStatus = Schedule.ScheduleStatus;

            _context.SaveChanges();

            TempData["Success"] = "Бронирование обновлено";
            return RedirectToPage("/Schedule/Index");
        }

        private void LoadLists()
        {
            HallsList = new SelectList(
                _context.Halls.OrderBy(h => h.HallName).ToList(),
                "Id", "HallName");

            ContractsList = new SelectList(
                _context.Contracts.OrderBy(c => c.ContractNumber).ToList(),
                "Id", "ContractNumber");
        }
    }
}