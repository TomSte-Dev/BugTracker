using BugTracker.Data;
using BugTracker.Models;

namespace BugTracker.Utility;

public class DbInitializer
{
    // Role constants
    public static readonly string[] Roles = { "Admin", "User" };

    // Status constants
    public static readonly string[] StatusNames = { "To Do", "In Progress", "Done" };


    // Method to seed the database with sample data if it is empty
    public static async void Seed(IApplicationBuilder applicationBuilder)
    {
        // Retrieve the AppointmentDbContext from the service provider
        BugTrackerDbContext DbContext = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<BugTrackerDbContext>();


        SeedRoles(DbContext);
        SeedStatuses(DbContext);
    }

    private static void SeedRoles(BugTrackerDbContext context)
    {
        if (!context.Roles.Any())
        {
            foreach (var role in Roles)
            {
                context.Roles.Add(new Role { Title = role });
            }
            context.SaveChanges();
        }
    }

    private static void SeedStatuses(BugTrackerDbContext context)
    {
        if (!context.Statuses.Any())
        {
            foreach (var statusName in StatusNames)
            {
                context.Statuses.Add(new Status { Name = statusName });
            }
            context.SaveChanges();
        }
    }
}
