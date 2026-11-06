using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using System.Security.Claims;

namespace WebPtichka.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _config;

        public LoginModel(IConfiguration config)
        {
            _config = config;
        }

        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        public string? Error { get; set; }

        public IActionResult OnGet()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();
                return RedirectToPage(role == "manager" ? "/Manager/Index" : "/Accountant/Index");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Error = "Введите логин и пароль.";
                return Page();
            }

            string connectionString = _config.GetConnectionString("PtichkaConnection")!;

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            const string sql = @"
                SELECT FullName, Role
                FROM Users
                WHERE Username = @Username
                  AND PasswordHash = @Password";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", Username);
            command.Parameters.AddWithValue("@Password", Password);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                Error = "Неверный логин или пароль.";
                return Page();
            }

            string fullName = reader["FullName"].ToString() ?? Username;
            string role = (reader["Role"].ToString() ?? "").ToLower();  // ← ВАЖНО: to lower

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, Username),
                new Claim(ClaimTypes.Role, role),
                new Claim("FullName", fullName)
            };

            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            return role switch
            {
                "manager" => RedirectToPage("/Manager/Index"),
                "accountant" => RedirectToPage("/Accountant/Index"),
                _ => RedirectToPage("/Index")
            };
        }
    }
}