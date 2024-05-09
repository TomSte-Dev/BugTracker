using BugTracker.Areas.Identity.Data;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Utility;

public class DbInitializer
{
    // Method to seed the database with sample data if it is empty
    public static async void Seed(IApplicationBuilder applicationBuilder)
    {
        // Retrieve the BugTrackerDbContext from the service provider
        BugTrackerDbContext dbContext = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<BugTrackerDbContext>();

        // Seed data for accounts, roles, projects, statuses, and tickets
        await SeedAccounts(applicationBuilder, dbContext);
        await SeedRoles(dbContext);
        await SeedProjects(dbContext);
        await SeedStatuses(dbContext);
        await SeedTickets(dbContext);
    }

    // Method to seed roles data into the database if it is empty
    private static async Task SeedRoles(BugTrackerDbContext context)
    {
        // Array of role titles to seed
        string[] Roles = { "Admin", "User" };

        // Check if any roles exist in the database
        if (!await context.Roles.AnyAsync())
        {
            // If no roles exist, seed the roles
            foreach (var role in Roles)
            {
                // Add each role to the database
                await context.Roles.AddAsync(new Role { Title = role });
            }
            // Save changes to the database
            await context.SaveChangesAsync();
        }
    }

    // Method to seed statuses data into the database if it is empty
    private static async Task SeedStatuses(BugTrackerDbContext context)
    {
        // Array of status names to seed
        string[] StatusNames = { "To Do", "In Progress", "Done" };

        // Check if any statuses exist in the database
        if (!await context.Statuses.AnyAsync())
        {
            // If no statuses exist, seed the statuses
            foreach (var status in StatusNames)
            {
                // Add each status to the database
                await context.Statuses.AddAsync(new Status { Name = status });
            }
            // Save changes to the database
            await context.SaveChangesAsync();
        }
    }

    // Method to seed user accounts data into the database if it is empty
    private static async Task SeedAccounts(IApplicationBuilder applicationBuilder, BugTrackerDbContext context)
    {
        // Check if any users exist in the database
        if (!await context.Users.AnyAsync())
        {
            // Begin a new scope to retrieve necessary services
            using (var scope = applicationBuilder.ApplicationServices.CreateScope())
            {
                // Get the UserManager service for managing user accounts
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<BugTrackerUser>>();

                // Define user account information to be seeded into the database
                var userData = new List<UserSeedData>()
                {
                    new UserSeedData { UserName = "user1@email.com", Email = "user1@email.com", FirstName = "Walter", LastName ="White", Password = "h493yz96x5XyxYTfAOZdey/rL0Qe2fmESwmldH9Ph9g=" },
                    new UserSeedData { UserName = "user2@email.com", Email = "user2@email.com", FirstName = "Jesse", LastName ="Pinkman", Password = "9NgIcEyeC6DRUQwjgD2NEJ4lRV6N3rkMVpndW9u0VOE=" },
                    new UserSeedData { UserName = "user3@email.com", Email = "user3@email.com", FirstName = "Saul", LastName ="Goodman", Password = "CYAW1j7zrwejgW47ldd36rgyOmUWHUJuwPRoOWvV5MM=" }
                };

                foreach (var user in userData)
                {
                    // Check if the user already exists
                    var existingUser = await userManager.FindByEmailAsync(user.Email);
                    if (existingUser == null)
                    {
                        // If user does not exist, create a new user account
                        var newUser = new BugTrackerUser
                        {
                            UserName = user.UserName,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName
                        };

                        // Create the new user account using UserManager service
                        await userManager.CreateAsync(newUser, user.Password);
                    }
                }

                // Save changes after all users have been processed
                await context.SaveChangesAsync();
            }
        }
    }

    // Class to represent user account information used for seeding
    private class UserSeedData
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
    }

    // Method to seed project data into the database if it is empty
    private static async Task SeedProjects(BugTrackerDbContext context)
    {
        // Check if any projects exist in the database
        if (!await context.Projects.AnyAsync())
        {
            // Create a new project
            Project project = new Project()
            {
                Title = "Bug Tracking Application",
                Description = "A bug tracking application created for internal use within a company"
            };

            // Add the project to the database and save changes
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            // Populate the project with all users
            var newProject = await context.Projects.FirstOrDefaultAsync();
            var users = await context.Users.ToListAsync();
            // Ensures a user was retreived
            if (users != null)
            {
                foreach (var user in users)
                {
                    var newProjectUser = new ProjectUser()
                    {
                        ProjectId = newProject.ProjectId,
                        // Assign the Admin role to each user for the new project
                        RoleId = await context.Roles
                        .Where(r => r.Title == "Admin")
                        .Select(s => s.RoleId)
                        .FirstOrDefaultAsync(),
                        UserEmail = user.Email
                    };
                    // Add the user to the project as an admin
                    await context.ProjectUsers.AddAsync(newProjectUser);
                }
                // Save changes after all users have been added to the project
                await context.SaveChangesAsync();
            }
        }
    }

    // Method to seed ticket data into the database if it is empty
    private static async Task SeedTickets(BugTrackerDbContext context)
    {
        // Check if any tickets exist in the database
        if (!await context.Tickets.AnyAsync())
        {
            // Retrieve the first project from the database
            var project = await context.Projects.FirstOrDefaultAsync();

            // Create a list of tickets to be seeded
            List<Ticket> tickets = new List<Ticket>()
            {
                new Ticket()
                {
                    ProjectId = project.ProjectId,
                    Title = "Identity",
                    Description = "Scaffold the identity framework to allow for user accounts with login, register, and management.",
                    StatusId = await context.Statuses
                        .Where(s => s.Name == "To Do")
                        .Select(s => s.StatusId)
                        .FirstOrDefaultAsync(),
                    AssigneeEmail = "user2@email.com",
                    ReporterEmail = "user1@email.com",
                    DateCreated = DateTime.Parse("2024-05-08 09:00:00"),
                    LastUpdateTime = DateTime.Parse("2024-05-08 09:00:00"),
                    Comments = new List<Comment>()
                }
            };

            // Add the tickets to the database and save changes
            await context.Tickets.AddRangeAsync(tickets);
            await context.SaveChangesAsync();

            // Retrieve the seeded ticket from the database
            var ticket = await context.Tickets.FirstOrDefaultAsync();

            // Create a list of comments to be seeded for the ticket
            List<Comment> comments = new List<Comment>()
            {
                new Comment()
                {
                    TicketId = ticket.TicketId,
                    CommentText = "This is a comment regarding the Identity ticket.",
                    CommentedBy = "user1@email.com",
                    CommentDate = DateTime.Parse("2024-05-08 09:00:00") 
                },
                new Comment()
                {
                    TicketId = ticket.TicketId,
                    CommentText = "We need to make sure to integrate the identity framework securely.",
                    CommentedBy = "user2@email.com",
                    CommentDate = DateTime.Parse("2024-05-09 10:30:00") 
                },
                new Comment()
                {
                    TicketId = ticket.TicketId,
                    CommentText = "Have we considered multi-factor authentication?",
                    CommentedBy = "user3@email.com",
                    CommentDate = DateTime.Parse("2024-05-10 14:45:00") 
                }
            };

            // Add the comments to the database
            await context.Comment.AddRangeAsync(comments);

            // Add the comments to the ticket and update the ticket
            ticket.Comments.AddRange(comments);
            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();
        }
    }
}

