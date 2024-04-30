using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Areas.Identity.Data;
namespace BugTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("BugTrackerDbContextConnection") ?? throw new InvalidOperationException("Connection string 'BugTrackerDbContextConnection' not found.");

            builder.Services.AddDbContext<BugTrackerDbContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<BugTrackerUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<BugTrackerDbContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddRazorPages();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}
