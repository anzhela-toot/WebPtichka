using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebPtichka.Data;
using WebPtichka.Models;

namespace WebPtichka.Pages.Clients
{
    public class EditModel : PageModel
    {
        private readonly PtichkaContext _context;

        public EditModel(PtichkaContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Client Client { get; set; } = new();

        public IActionResult OnGet(int id)
        {
            var client = _context.Clients.Find(id);
            if (client == null) return RedirectToPage("/Clients/Clients");

            Client = client;
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Client.FullName) || string.IsNullOrWhiteSpace(Client.Phone))
            {
                return Page();
            }

            var existing = _context.Clients.Find(Client.Id);
            if (existing == null) return RedirectToPage("/Clients/Clients");

            existing.FullName = Client.FullName;
            existing.Phone = Client.Phone;
            existing.Email = Client.Email;

            _context.SaveChanges();

            return RedirectToPage("/Clients/Clients");
        }
    }
}