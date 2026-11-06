using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebPtichka.Data;       // ← для PtichkaContext
using WebPtichka.Models;     // ← ОБЯЗАТЕЛЬНО для класса Client

namespace WebPtichka.Pages.Clients
{
    public class CreateModel : PageModel
    {
        private readonly PtichkaContext _context;

        public CreateModel(PtichkaContext context)
        {
            _context = context;
        }

        // Вот это свойство ищет Razor, когда вы пишете Client.FullName
        [BindProperty]
        public Client Client { get; set; } = new();

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Client.FullName) || string.IsNullOrWhiteSpace(Client.Phone))
            {
                ModelState.AddModelError(string.Empty, "ФИО и Телефон обязательны для заполнения.");
                return Page();
            }

            Client.RegistrationDate = DateOnly.FromDateTime(DateTime.Today);

            _context.Clients.Add(Client);
            _context.SaveChanges();

            return RedirectToPage("/Clients/Clients");
        }
    }
}