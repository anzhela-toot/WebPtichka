using WebPtichka.Data;
using WebPtichka.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Photino.NET;
namespace WebPtichka
{
    public class Program
    {
        [STAThread]

        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages(options =>
            {
                options.Conventions.AuthorizeFolder("/Accountant", "AccountantOnly");
                options.Conventions.AuthorizeFolder("/Manager", "ManagerOnly");
                options.Conventions.AllowAnonymousToPage("/Account/Login");
            });

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("ManagerOnly", p => p.RequireRole("manager"));
                options.AddPolicy("AccountantOnly", p => p.RequireRole("accountant"));
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddDbContext<PtichkaContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("PtichkaConnection")));

            builder.Services.AddScoped<ScheduleManager>();
            builder.Services.AddScoped<ContractManager>();
            builder.Services.AddScoped<DashboardService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // app.UseHttpsRedirection();   ← ЗАКОММЕНТИРОВАНО для Photino
            app.UseStaticFiles();
            app.UseRouting();
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapRazorPages();

            // === ЗАПУСК СЕРВЕРА В ФОНЕ ===
            var serverTask = app.RunAsync("http://localhost:5000");

            // === ЖДЁМ, ПОКА ПОРТ ОТКРОЕТСЯ ===
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.Elapsed.TotalSeconds < 10)
            {
                try
                {
                    using var client = new System.Net.Sockets.TcpClient();
                    client.Connect("localhost", 5000);
                    break;
                }
                catch
                {
                    Thread.Sleep(200);
                }
            }

            // ВОТ ЗДЕСЬ добавляем паузу (после закрывающей скобки цикла)
            Thread.Sleep(3000);


            // === ОТКРЫВАЕМ ОКНО PHOTINO ===
            var window = new PhotinoWindow()
                .SetTitle("Птичка в кадре")
                .SetSize(1400, 900)
                .SetResizable(true)
                .Center()
                .Load("http://localhost:5000");

            window.WaitForClose();
            app.StopAsync().Wait();
        }
    }
}