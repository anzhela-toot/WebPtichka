using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPtichka.Data;
using WebPtichka.Models;
using ScheduleModel = WebPtichka.Models.Schedule;

namespace WebPtichka.Pages.Contracts
{
    public class EditModel : PageModel
    {
        private readonly PtichkaContext _context;

        public EditModel(PtichkaContext context)
        {
            _context = context;
        }

        public SelectList ClientsList { get; set; } = default!;
        public SelectList EmployeesList { get; set; } = default!;
        public SelectList HallsList { get; set; } = default!;
        public List<Service> ServicesList { get; set; } = new();

        [BindProperty] public int Id { get; set; }
        [BindProperty] public int ClientId { get; set; }
        [BindProperty] public int EmployeeId { get; set; }
        [BindProperty] public int HallId { get; set; }
        [BindProperty] public DateOnly EventDate { get; set; }
        [BindProperty] public TimeOnly StartTime { get; set; }
        [BindProperty] public TimeOnly EndTime { get; set; }
        [BindProperty] public string ContractStatus { get; set; } = "";
        [BindProperty] public List<int> ServiceIds { get; set; } = new();

        public string? Error { get; set; }

        public IActionResult OnGet(int id)
        {
            var contract = _context.Contracts
                .Include(c => c.ContractServices)
                .FirstOrDefault(c => c.Id == id);

            if (contract == null)
                return RedirectToPage("/Contracts/Index");

            // Заполняем поля формы
            Id = contract.Id;
            ClientId = contract.ClientId;
            EmployeeId = contract.EmployeeId;
            HallId = contract.HallId;
            EventDate = contract.EventDate;
            StartTime = contract.StartTime;
            EndTime = contract.EndTime;
            ContractStatus = contract.ContractStatus;
            ServiceIds = contract.ContractServices
                .Select(cs => cs.ServiceId)
                .ToList();

            LoadLists();
            return Page();
        }

        public IActionResult OnPost()
        {
            LoadLists();

            if (ClientId <= 0) { Error = "Выберите клиента."; return Page(); }
            if (EmployeeId <= 0) { Error = "Выберите сотрудника."; return Page(); }
            if (HallId <= 0) { Error = "Выберите зал."; return Page(); }
            if (EndTime <= StartTime) { Error = "Время окончания должно быть позже времени начала."; return Page(); }

            var contract = _context.Contracts
                .Include(c => c.ContractServices)
                .FirstOrDefault(c => c.Id == Id);

            if (contract == null)
                return RedirectToPage("/Contracts/Index");

            var hall = _context.Halls.Find(HallId);
            if (hall == null) { Error = "Зал не найден."; return Page(); }

            // Проверка занятости зала (исключая текущий договор)
            var conflict = _context.Schedules.Any(s =>
                s.HallId == HallId &&
                s.ContractId != contract.Id &&
                s.BookingDate == EventDate &&
                s.StartTime < EndTime &&
                s.EndTime > StartTime &&
                s.ScheduleStatus != "Отменено");

            if (conflict)
            {
                Error = $"Зал «{hall.HallName}» уже занят на {EventDate:dd.MM.yyyy} в это время.";
                return Page();
            }

            // Пересчёт суммы
            var hours = (decimal)(EndTime - StartTime).TotalHours;
            var hallAmount = hall.PricePerHour * hours;

            var selectedServices = _context.Services
                .Where(s => ServiceIds.Contains(s.Id))
                .ToList();
            var servicesAmount = selectedServices.Sum(s => s.ServicePrice);

            var totalAmount = hallAmount + servicesAmount;

            // Обновляем договор
            contract.ClientId = ClientId;
            contract.EmployeeId = EmployeeId;
            contract.HallId = HallId;
            contract.EventDate = EventDate;
            contract.StartTime = StartTime;
            contract.EndTime = EndTime;
            contract.ContractStatus = ContractStatus;
            contract.TotalAmount = totalAmount;

            // Удаляем старые услуги, добавляем новые
            _context.ContractServices.RemoveRange(contract.ContractServices);
            foreach (var service in selectedServices)
            {
                _context.ContractServices.Add(new ContractService
                {
                    ContractId = contract.Id,
                    ServiceId = service.Id,
                    Quantity = 1,
                    Price = service.ServicePrice
                });
            }

            // Обновляем запись в расписании
            var schedule = _context.Schedules.FirstOrDefault(s => s.ContractId == contract.Id);
            if (schedule != null)
            {
                schedule.HallId = HallId;
                schedule.BookingDate = EventDate;
                schedule.StartTime = StartTime;
                schedule.EndTime = EndTime;
            }

            _context.SaveChanges();

            return RedirectToPage("/Contracts/Index");
        }

        private void LoadLists()
        {
            ClientsList = new SelectList(
                _context.Clients.OrderBy(c => c.FullName).ToList(),
                "Id", "FullName");

            EmployeesList = new SelectList(
                _context.Employees.OrderBy(e => e.FullName).ToList(),
                "Id", "FullName");

            HallsList = new SelectList(
                _context.Halls.OrderBy(h => h.HallName).ToList(),
                "Id", "HallName");

            ServicesList = _context.Services.OrderBy(s => s.ServiceName).ToList();
        }
    }
}