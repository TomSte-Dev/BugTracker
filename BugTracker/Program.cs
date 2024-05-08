using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Authorization;
using BugTracker.Utility;
using BugTracker.Repositories;
namespace BugTracker;

public class Program
{
    public static void Main(string[] args)
    {
        // Create a new WebApplication builder instance
        var builder = WebApplication.CreateBuilder(args);

        // Retrieve connection string from app settings
        var connectionString = builder.Configuration.GetConnectionString("BugTrackerDbContextConnection")
            ?? throw new InvalidOperationException("Connection string 'BugTrackerDbContextConnection' not found.");

        // Add database context to the service collection
        builder.Services.AddDbContext<BugTrackerDbContext>(options => options.UseSqlServer(connectionString));

        // Add default Identity services
        builder.Services.AddDefaultIdentity<BugTrackerUser>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<BugTrackerDbContext>();

        // Register repositories for dependency injection
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<ITicketRepository, TicketRepository>();

        // Add MVC services to the service collection
        builder.Services.AddControllersWithViews(
            // Configure global filter to require authorization for every controller action
            o => o.Filters.Add(new AuthorizeFilter()));

        // Add Razor Pages services to the service collection
        builder.Services.AddRazorPages();

        // Add authentication services to the service collection
        builder.Services.AddAuthentication();

        // Build the application
        var app = builder.Build();

        // Configure the HTTP request pipeline
        if (!app.Environment.IsDevelopment())
        {
            // Configure exception handling middleware
            app.UseExceptionHandler("/Home/Error");

            // Configure HTTP Strict Transport Security (HSTS)
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseStaticFiles();

        app.UseRouting();

        // Configure authentication middleware
        app.UseAuthentication();

        // Configure authorization middleware
        app.UseAuthorization();

        // Configure MVC controller routing
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Projects}/{action=Index}/{id?}");

        // Configure Razor Pages routing
        app.MapRazorPages();

        // Seed initial data for appointments and users into the database if empty
        DbInitializer.Seed(app);

        // Run the application
        app.Run();
    }
}

