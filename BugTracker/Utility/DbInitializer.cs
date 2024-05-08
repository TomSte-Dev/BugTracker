using BugTracker.Areas.Identity.Data;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Xml.Linq;

namespace BugTracker.Utility;

public class DbInitializer
{


    // Method to seed the database with sample data if it is empty
    public static async void Seed(IApplicationBuilder applicationBuilder)
    {
        // Retrieve the AppointmentDbContext from the service provider
        BugTrackerDbContext DbContext = applicationBuilder.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<BugTrackerDbContext>();

        await SeedAccounts(applicationBuilder, DbContext);
        await SeedRoles(DbContext);
        await SeedProjects(DbContext);

        await SeedStatuses(DbContext);
        await SeedTickets(DbContext);
    }

    private static async Task SeedRoles(BugTrackerDbContext context)
    {
        string[] Roles = { "Admin", "User" };

        if (!await context.Roles.AnyAsync())
        {
            foreach (var role in Roles)
            {
                await context.Roles.AddAsync(new Role { Title = role });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedStatuses(BugTrackerDbContext context)
    {
        string[] StatusNames = { "To Do", "In Progress", "Done" };

        if (!await context.Statuses.AnyAsync())
        {
            foreach(var status in StatusNames)
            {
                await context.Statuses.AddAsync(new Status {Name = status });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAccounts(IApplicationBuilder applicationBuilder, BugTrackerDbContext context)
    {
        if (!await context.Users.AnyAsync())
        {
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

                        await userManager.CreateAsync(newUser, user.Password);
                    }
                }

                // Save changes after all users have been processed
                await context.SaveChangesAsync();
            }
        }
    }

    private class UserSeedData
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
    }

    private static async Task SeedProjects(BugTrackerDbContext context)
    {
        if (!await context.Projects.AnyAsync())
        {
            // Create project
            Project project = new Project()
            {
                Title = "Bug Tracking Application",
                Description = "A bug tracking application created for internal use within a company"
            };

            // Add project to Db and save
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();

            // Populate project with all users
            var newProject = await context.Projects.FirstOrDefaultAsync();
            var users = await context.Users.ToListAsync();
            if (users != null)
            {
                foreach (var user in users)
                {
                    var newProjectUser = new ProjectUser()
                    {
                        ProjectId = newProject.ProjectId,
                        RoleId = await context.Roles
                        .Where(r => r.Title == "Admin")
                        .Select(s => s.RoleId)
                        .FirstOrDefaultAsync(),
                        UserEmail = user.Email
                    };
                    await context.ProjectUsers.AddAsync(newProjectUser);
                }
                await context.SaveChangesAsync();
            }
        }
    }


    private static async Task SeedTickets(BugTrackerDbContext context)
    {
        if (!await context.Tickets.AnyAsync())
        {
            var project = await context.Projects.FirstOrDefaultAsync();
            List<Ticket> tickets = new List<Ticket>()
            {
                new Ticket()
                {
                    ProjectId= project.ProjectId,
                    Title = "Identity",
                    Description = "Scaffold the identity framework to allow for user accounts with login, register, and management.",
                    StatusId = await context.Statuses
                        .Where(s => s.Name == "To Do")
                        .Select(s => s.StatusId)
                        .FirstOrDefaultAsync(),
                    AssigneeEmail = "user2@email.com",
                    ReporterEmail = "user1@email.com",
                    DateCreated = DateTime.Parse("2024-05-08 09:00:00"), // Insert appropriate date and time
                    LastUpdateTime = DateTime.Parse("2024-05-08 09:00:00"), // Insert appropriate date and time
                    Comments = new List<Comment>()

                }
            };
            await context.Tickets.AddRangeAsync(tickets);
            await context.SaveChangesAsync();

            var ticket = await context.Tickets.FirstOrDefaultAsync();

            List<Comment> Comments = new List<Comment>()
            {
                new Comment()
                {
                    TicketId = ticket.TicketId,
                    CommentText = "This is a comment regarding the Identity ticket.",
                    CommentedBy = "user1@email.com",
                    CommentDate = DateTime.Parse("2024-05-08 09:00:00") // Insert appropriate date and time
                },
                new Comment()
                {
                    TicketId = ticket.TicketId,
                    CommentText = "We need to make sure to integrate the identity framework securely.",
                    CommentedBy = "user2@email.com",
                    CommentDate = DateTime.Parse("2024-05-09 10:30:00") // Insert appropriate date and time
                },
                new Comment()
                {
                    TicketId = ticket.TicketId,
                    CommentText = "Have we considered multi-factor authentication?",
                    CommentedBy = "user3@email.com",
                    CommentDate = DateTime.Parse("2024-05-10 14:45:00") // Insert appropriate date and time
                }
            };

            await context.Comment.AddRangeAsync(Comments);
            ticket.Comments.AddRange(Comments);
            context.Tickets.Update(ticket);
            await context.SaveChangesAsync();

        }
    }
}

