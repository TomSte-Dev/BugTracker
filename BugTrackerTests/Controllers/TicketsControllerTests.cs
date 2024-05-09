using BugTracker.Controllers;
using BugTracker.Models;
using BugTrackerTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

}
