using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebPtichka.Data;
using WebPtichka.Models;
using ScheduleModel = WebPtichka.Models.Schedule;

namespace WebPtichka.Pages.Contracts
{
    public class CreateModel : PageModel
    {
        private readonly PtichkaContext _context;

        public CreateModel(PtichkaContext context)
        {
            _context = context;
        }

        // Списки для выпадающих меню
        public SelectList ClientsList { get; set; } = default!;
        public SelectList EmployeesList { get; set; } = default!;
        public SelectList HallsList { get; set; } = default!;
        public List<Service> ServicesList { get; set; } = new();

        // Поля формы (привязываются из POST)
        [BindProperty] public int ClientId { get; set; }
        [BindProperty] public int EmployeeId { get; set; }
        [BindProperty] public int HallId { get; set; }
        [BindProperty] public DateOnly EventDate { get; set; }
        [BindProperty] public TimeOnly StartTime { get; set; }
        [BindProperty] public TimeOnly EndTime { get; set; }
        [BindProperty] public string ContractStatus { get; set; } = "Активен";
        [BindProperty] public List<int> ServiceIds { get; set; } = new();

        public string? Error { get; set; }

        public void OnGet()
        {
            LoadLists();
            EventDate = DateOnly.FromDateTime(DateTime.Today);
            StartTime = new TimeOnly(10, 0);
            EndTime = new TimeOnly(12, 0);
        }

        public IActionResult OnPost()
        {
            LoadLists();

            // Валидация
            if (ClientId <= 0) { Error = "Выберите клиента."; return Page(); }
            if (EmployeeId <= 0) { Error = "Выберите сотрудника."; return Page(); }
            if (HallId <= 0) { Error = "Выберите зал."; return Page(); }
            if (EndTime <= StartTime) { Error = "Время окончания должно быть позже времени начала."; return Page(); }

            // Зал и его стоимость
            var hall = _context.Halls.Find(HallId);
            if (hall == null) { Error = "Зал не найден."; return Page(); }

            // Проверка занятости зала
            var conflict = _context.Schedules.Any(s =>
                s.HallId == HallId &&
                s.BookingDate == EventDate &&
                s.StartTime < EndTime &&
                s.EndTime > StartTime &&
                s.ScheduleStatus != "Отменено");

            if (conflict)
            {
                Error = $"Зал «{hall.HallName}» уже занят на {EventDate:dd.MM.yyyy} в это время.";
                return Page();
            }

            // Расчёт стоимости
            var hours = (decimal)(EndTime - StartTime).TotalHours;
            var hallAmount = hall.PricePerHour * hours;

            var selectedServices = _context.Services
                .Where(s => ServiceIds.Contains(s.Id))
                .ToList();
            var servicesAmount = selectedServices.Sum(s => s.ServicePrice);

            var totalAmount = hallAmount + servicesAmount;

            // Генерация номера договора: Д-2026-0001
            var year = DateTime.Now.Year;
            var countThisYear = _context.Contracts
                .Count(c => c.ContractNumber.StartsWith($"Д-{year}-"));
            var contractNumber = $"Д-{year}-{(countThisYear + 1):D4}";

            // Создание договора
            var contract = new Contract
            {
                ContractNumber = contractNumber,
                ClientId = ClientId,
                EmployeeId = EmployeeId,
                HallId = HallId,
                EventDate = EventDate,
                StartTime = StartTime,
                EndTime = EndTime,
                TotalAmount = totalAmount,
                ContractStatus = ContractStatus,
                CreatedDate = DateTime.Now
            };

            _context.Contracts.Add(contract);
            _context.SaveChanges(); // чтобы получить contract.Id

            // Добавляем услуги
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

            // Добавляем в расписание
            _context.Schedules.Add(new WebPtichka.Models.Schedule
            {
                ContractId = contract.Id,
                HallId = HallId,
                BookingDate = EventDate,
                StartTime = StartTime,
                EndTime = EndTime,
                ScheduleStatus = "Забронировано"
            });

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