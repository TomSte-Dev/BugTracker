using BugTracker.Controllers;
using BugTracker.Models;
using BugTrackerTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BugTrackerTests.Controllers;

public class ProjectsControllerTests
{
    [Fact]
    public async Task Index_ReturnsProjectsForUser()
    {
        // Arrange
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();

        // Mock User.Identity.Name
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "user1@email.com") // Replace with your sample user identity
        }));

        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var controller = new ProjectsController(mockProjectRepository.Object)
        {
            ControllerContext = controllerContext
        };

        // Act
        var result = await controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Project>>(viewResult.ViewData.Model);
        Assert.NotNull(model);

        // Add specific assertions to verify user-project connection
        var userProjects = model.ToList(); // Convert the model to a list for easier handling
        Assert.Single(userProjects); // Check that only one project is returned for the user
        var userProject = userProjects.First(); // Get the first project
        Assert.Equal(1, userProject.ProjectId); // Check that the project ID is 1
    }
}

