using BugTracker.Models;
using BugTracker.Repositories;
using Moq;

namespace BugTrackerTests.Mocks
{
    public class RepositoryMocks
    {
        public static Mock<IProjectRepository> GetProjectRepository()
        {
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

            var projects = new List<Project>
            {
                new Project()
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



            var mockProjectRepository = new Mock<IProjectRepository>();
            mockProjectRepository.Setup(repo => repo.AllRoles).Returns(roles);
            mockProjectRepository.Setup(repo => repo.AllProjects).Returns(projects);

            // Setup the AddProject method to actually add projects to the list
            mockProjectRepository.Setup(repo => repo.AddProject(It.IsAny<Project>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask)
                .Callback<Project, string>((project, userEmail) =>
                {
                    // Add the project to the list
                    projects.Add(project);
                    var newProjectUser = new ProjectUser()
                    {
                        ProjectUserId = 2, // Doesnt matter for testing purposes but as we have a default user at 1 2 should do
                        ProjectId = project.ProjectId,
                        RoleId = 1,
                        UserEmail = userEmail
                    };
                    users.Add(newProjectUser);
                });

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

                    return Task.FromResult<IEnumerable<Project>>(userProjects);
                });


            return mockProjectRepository;
        }

        public static Mock<ITicketRepository> GetTicketRepository()
        {
            var status = new List<Status>
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

            var mockTicketRepository = new Mock<ITicketRepository>();
            mockTicketRepository.Setup(repo => repo.AllStatuses).Returns(status);
            mockTicketRepository.Setup(repo => repo.AllTickets).Returns(tickets);

            return mockTicketRepository;
        }
    }
}
