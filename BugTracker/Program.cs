using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Identity;
using BugTracker.Utility;
using BugTracker.Repositories;
namespace BugTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("BugTrackerDbContextConnection") ?? throw new InvalidOperationException("Connection string 'BugTrackerDbContextConnection' not found.");

            builder.Services.AddDbContext<BugTrackerDbContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<BugTrackerUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<BugTrackerDbContext>();

            // Register the Repository class to be injected whenever an IRepository dependency is requested.
            // This enables dependency injection, allowing components to easily obtain instances of AppointmentRepository
            // without needing to create them directly.
            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
            builder.Services.AddScoped<ITicketRepository, TicketRepository>();


            // Add services to the container.
            builder.Services.AddControllersWithViews(
                // Specifies a global filter ensuring that ever controller needs a logged in user
                // Secure by default scenario
                // If you want to opt out use [AlllowAnonymous] attribute
                o => o.Filters.Add(new AuthorizeFilter())
                );
            builder.Services.AddRazorPages();
            builder.Services.AddAuthentication();

            // Dependency injection containers
            // Inject into controller on constructor 


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

            // Never map after endpoints as it needs to happen before request
            app.UseAuthentication(); 
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Projects}/{action=Index}/{id?}");

            app.MapRazorPages();

            // Seed initial data for appointments and users into the database.

            // Only if the databases are empty
            DbInitializer.Seed(app);

            app.Run();
        }
    }
}
