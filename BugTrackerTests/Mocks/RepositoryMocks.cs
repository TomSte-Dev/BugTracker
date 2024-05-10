using BugTracker.Models;
using BugTracker.Repositories;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BugTrackerTests.Mocks
{
    public class RepositoryMocks
    {
        public static Mock<IProjectRepository> GetProjectRepository()
        {
            // Create mock data for reteival from the mock repository
            var roles = new List<Role>
            {
                new Role()
                {
                    RoleId = 1,
                    Title = "Admin"
                },

                new Role()
                {
                    RoleId = 2,
                    Title = "User"
                }
            };
            var projects = new List<BugTracker.Models.Project>
            {
                new BugTracker.Models.Project()
                {
                    ProjectId = 1,
                    Title = "Sample Project",
                    Description = "Sample Project Description"
                }
            };
            var users = new List<ProjectUser>
            {
                new ProjectUser()
                {
                    ProjectUserId = 1,
                    ProjectId = 1,
                    RoleId = 1,
                    UserEmail = "user1@email.com"
                }
            };

            // Initialise repository 
            var mockProjectRepository = new Mock<IProjectRepository>();

            // Setup attributes
            mockProjectRepository.Setup(repo => repo.AllRoles).Returns(roles);
            mockProjectRepository.Setup(repo => repo.AllProjects).Returns(projects);

            //Setup methods
            // Return a project given an id
            mockProjectRepository.Setup(repo => repo.GetProjectById(It.IsAny<int>()))
                .ReturnsAsync((int id) => projects.FirstOrDefault(p => p.ProjectId == id));

            // Checks user is assigned to project
            mockProjectRepository.Setup(repo => repo.IsUserAssignedToProject(It.IsAny<int?>(), It.IsAny<string?>()))
                .ReturnsAsync((int? projectId, string? user) =>
                {
                    // Simulate the behavior of querying the database to check if any project user matches the conditions
                    bool isAssigned = users.Any(project => project.ProjectId == projectId && project.UserEmail == user);
                    return(isAssigned);
                });

            // Setup the AddProject method to actually add projects to the list in the mock repository
            mockProjectRepository.Setup(repo => repo.AddProject(It.IsAny<BugTracker.Models.Project>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<BugTracker.Models.Project, string>((project, userEmail) =>
                {
                    // Add the project to the list
                    projects.Add(project);
                    var newProjectUser = new ProjectUser()
                    {
                        ProjectUserId = 2,
                        ProjectId = project.ProjectId,
                        RoleId = 1,
                        UserEmail = userEmail
                    };
                    users.Add(newProjectUser);
                });

            // Provide a list of projects given a user email
            mockProjectRepository.Setup(repo => repo.GetProjectsByUser(It.IsAny<string>()))
                .Returns((string user) =>
                {
                    // Retrieve project IDs associated with the specified user
                    var projectIds = users
                        .Where(pu => pu.UserEmail == user)
                        .Select(p => p.ProjectId)
                        .ToList();

                    // Filter projects based on the project IDs
                    var userProjects = projects
                        .Where(p => projectIds.Contains(p.ProjectId))
                        .ToList();

                    return Task.FromResult<IEnumerable<BugTracker.Models.Project>>(userProjects);
                });

            // Gets a list pf user emails by a given projects id
            mockProjectRepository.Setup(repo => repo.GetUserEmailsByProjectId(It.IsAny<int>()))
                .ReturnsAsync((int? id) =>
                {
                    // filters for a loist of distinct user emails
                    var userEmails = users
                        .Where(u => u.ProjectId == id)
                        .Select(u => u.UserEmail)
                        .Distinct()
                        .ToList();

                    return userEmails;
                });

            // Updates a project user by overwriting the previously stored index
            mockProjectRepository.Setup(repo => repo.UpdateProjectUser(It.IsAny<ProjectUser>()))
                .Returns(Task.CompletedTask)
                .Callback<ProjectUser>((newProjectUser) =>
                {
                    users.Add(newProjectUser);

                });

            // Delete project user
            // No call back and deletion from mock repo as it is not needed for testing
            // We are just checking that it validates properly and redirects
            mockProjectRepository.Setup(repo => repo.DeleteProjectUser(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Add project user
            // No call back and addition from mock repo as it is not needed for testing
            // We are just checking that it validates properly
            mockProjectRepository.Setup(repo => repo.AddProjectUser(It.IsAny<ProjectUser>()))
                .Returns(Task.CompletedTask);

            // Update project
            // Does not alter mock repository
            // We are just checking that it validates properly
            mockProjectRepository.Setup(repo => repo.UpdateProject(It.IsAny<BugTracker.Models.Project>()))
                .Returns(Task.CompletedTask);

            // Delete project
            // Does not alter mock repository
            // We are just checking that it validates properly
            mockProjectRepository.Setup(repo => repo.DeleteProjectById(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            return mockProjectRepository;
        }

        public static Mock<ITicketRepository> GetTicketRepository()
        {
            // Create mock data for reteival from the mock repository
            var statuses = new List<Status>
            {
                new Status()
                {
                    StatusId = 1,
                    Name = "To Do"
                },

                new Status()
                {
                    StatusId = 2,
                    Name = "In Progress"
                },

                new Status()
                {
                    StatusId = 3,
                    Name = "Done"
                }
            };
            var comments = new List<Comment>
            {
                new Comment()
                {
                    TicketId = 1,
                    CommentId = 1,
                    CommentText = "Sample comment",
                    CommentedBy = "user1@email.com",
                    CommentDate = DateTime.Parse("2024-05-09 09:30:00"),

                }
            };
            var tickets = new List<Ticket>
            {
                new Ticket()
                {
                    TicketId = 1,
                    ProjectId = 1,
                    Title = "Identity",
                    Description = "Scaffold the identity framework to allow for user accounts with login, register, and management.",
                    StatusId = 1,
                    AssigneeEmail = "user2@email.com",
                    ReporterEmail = "user1@email.com",
                    DateCreated = DateTime.Parse("2024-05-08 09:00:00"),
                    LastUpdateTime = DateTime.Parse("2024-05-09 09:30:00"),
                    Comments = comments
                }
            };

            // Initialise the mock ticket repository
            var mockTicketRepository = new Mock<ITicketRepository>();

            // Setup attributes
            mockTicketRepository.Setup(repo => repo.AllStatuses).Returns(statuses);
            mockTicketRepository.Setup(repo => repo.AllTickets).Returns(tickets);

            // Setup methods
            // Gets a ticket by a given id
            mockTicketRepository.Setup(repo => repo.GetTicketById(It.IsAny<int>()))
                .ReturnsAsync((int id) => tickets.FirstOrDefault(t => t.TicketId == id));

            // Adds a ticket by overwriting the previously stored index
            mockTicketRepository.Setup(repo => repo.AddTicket(It.IsAny<Ticket>()))
                .Returns(Task.CompletedTask)
                .Callback<Ticket>((ticket) =>
                {
                    // Add the project to the list
                    tickets.Add(ticket);
                    if (ticket.Comments != null) // Ensures comments are added if provided
                        comments.AddRange(ticket.Comments);
                });

            // Update Ticket
            // Does not alter mock repository
            // We are just checking that it validates properly
            mockTicketRepository.Setup(repo => repo.UpdateTicket(It.IsAny<Ticket>()))
                .Returns(Task.CompletedTask);

            // Update Ticket
            // Does not alter mock repository
            // We are just checking that it validates properly
            mockTicketRepository.Setup(repo => repo.DeleteTicketById(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Add comment to ticket
            // Does not alter mock repository
            // We are just checking that it validates properly
            mockTicketRepository.Setup(repo => repo.AddCommentToTicket(It.IsAny<Ticket>(), It.IsAny<Comment>()))
                .Returns(Task.CompletedTask);

            return mockTicketRepository;
        }
    }
}
