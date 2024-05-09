using BugTracker.Controllers;
using BugTracker.Models;
using BugTrackerTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
        // Needed as its apart of the index method
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, "user1@email.com") 
        }));
        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Create the controller using the mock repo
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
        var userProjects = model.ToList(); // Convert the model to a list
        Assert.Single(userProjects); // Check that only one project is returned for the user
        var userProject = userProjects.First(); // Get the first project
        Assert.Equal(1, userProject.ProjectId); // Check that the project ID is 1
    }

    [Fact]
    public async Task CreateProject_AddsProjectToRepository_RedirectsToIndex()
    {
        // Arrange
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
        new Claim(ClaimTypes.Name, "user1@email.com")
        }));

        var controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        var controller = new ProjectsController(mockProjectRepository.Object)
        {
            ControllerContext = controllerContext
        };

        var project = new Project
        {
            ProjectId = 2,
            Title = "New Project",
            Description = "New Project Description"
        };

        // Act
        var result = await controller.CreateProject(project);

        // Assert
        var createdProject = (await mockProjectRepository.Object.GetProjectsByUser(user.Identity.Name))
            .Where(p => p.ProjectId == project.ProjectId)
            .FirstOrDefault();
        Assert.NotNull(createdProject);
        Assert.Equal(project.Title, createdProject.Title);
        Assert.Equal(project.Description, createdProject.Description);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }

}

