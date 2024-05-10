using BugTracker.Controllers;
using BugTracker.Models;
using BugTracker.Repositories;
using BugTracker.Utility;
using BugTrackerTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using System.Xml.Linq;

namespace BugTrackerTests.Controllers;

public class TicketsControllerTests
{
    [Fact]
    public async Task Index_ReturnsTicketsForProject()
    {
        // Arrange
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var mockTicketRepository = RepositoryMocks.GetTicketRepository();

        var projectId = 1; // Sample project ID

        // Mock User.Identity.Name
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.Name, "user1@email.com") // Replace with your sample user identity
        }));

        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var controller = new TicketsController(mockTicketRepository.Object, mockProjectRepository.Object)
        {
            ControllerContext = controllerContext
        };

        // Act
        var result = await controller.Index(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Ticket>>(viewResult.Model);
        Assert.NotNull(model);

        // Verify that the tickets returned belong to the specified project
        Assert.All(model, t => Assert.Equal(projectId, t.ProjectId));
    }

    [Fact]
    public async Task CreateTicket_ReturnsRedirectToAction()
    {
        // Arrange
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var mockTicketRepository = RepositoryMocks.GetTicketRepository();

        var projectId = 1; // Set up the project ID

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };

        var controller = new TicketsController(mockTicketRepository.Object, mockProjectRepository.Object);

        var comments = new List<Comment>()
        {
            new Comment
            {
                TicketId = 2,
                CommentId = 2,
                CommentText = "Sample comment",
                CommentedBy = "user1@email.com",
                CommentDate = DateTime.Parse("2024-05-09 09:31:00"),
            }
        };

        var ticket = new Ticket()
        {
            TicketId = 2,
            ProjectId = 1,
            Title = "test",
            Description = "test ticket",
            StatusId = 1,
            AssigneeEmail = "user2@email.com",
            ReporterEmail = "user1@email.com",
            DateCreated = DateTime.Parse("2024-05-08 09:00:00"),
            LastUpdateTime = DateTime.Parse("2024-05-09 09:30:00"),
            Comments = comments // Assign comments here
        };

        // Act
        var result = await controller.CreateTicket(ticket);

        // Assert
        var createdTicket = (await mockTicketRepository.Object.GetTicketById(ticket.TicketId));

        Assert.NotNull(createdTicket);
        Assert.Equal(ticket, createdTicket);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }


}
