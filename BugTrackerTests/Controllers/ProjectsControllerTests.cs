using BugTracker.Controllers;
using BugTracker.Models;
using BugTracker.Utility;
using BugTrackerTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Moq;
using System.Security.Claims;
using Project = BugTracker.Models.Project;

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

    [Fact]
    public async Task TeamMembers_Returns_Unauthorized_For_Non_Assigned_User()
    {
        // Arrange
        var projectId = 1; // Sample project ID
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

        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "User";

        // Act
        var result = await controller.TeamMembers(projectId);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task TeamMembers_Returns_ViewResult_For_Assigned_User()
    {
        // Arrange
        var projectId = 1; // Sample project ID
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

        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        // Act
        var result = await controller.TeamMembers(projectId);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        var model = Assert.IsAssignableFrom<IEnumerable<ProjectUser>>(viewResult.Model);
        // Add more assertions as needed
    }

    [Fact]
    public async Task EditTeamMember_Returns_Unauthorized_For_Non_Admin_User()
    {
        // Arrange
        var id = 1; // Sample project user ID
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "User";

        var projectUser = new ProjectUser()
        {
            ProjectId = id,
            ProjectUserId = 2,
            RoleId = 2,
            UserEmail = "User2@email.com"
        };

        // Act
        var result = await controller.EditTeamMember(id, projectUser);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task EditTeamMember_Returns_NotFound_For_Null_Id()
    {
        // Arrange
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        // Act
        var result = await controller.EditTeamMember(null, new ProjectUser());

        // Assert
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task EditTeamMember_Returns_RedirectToAction_For_Valid_Input()
    {
        // Arrange
        var id = 1; // Sample project user ID
        var projectId = 1;
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        var projectUser = new ProjectUser()
        {
            ProjectId = projectId,
            ProjectUserId = id,
            RoleId = 1,
            UserEmail = "User1@email.com"
        };

        // Act
        var result = await controller.EditTeamMember(id, projectUser);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TeamMembers", redirectToActionResult.ActionName);
        Assert.Equal("Projects", redirectToActionResult.ControllerName);
    }

    [Fact]
    public async Task RemoveTeamMember_Returns_Unauthorized_For_Non_Admin_User()
    {
        // Arrange
        var id = 1; // Sample project user ID
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);
        CurrentProjectSingleton.CurrentUserRole = "User"; // Set user role

        // Act
        var result = await controller.RemoveTeamMember(id);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task RemoveTeamMember_Returns_RedirectToAction_For_Valid_Input()
    {
        // Arrange
        var id = 1; // Sample project user ID
        var projectId = 1;
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        // Act
        var result = await controller.RemoveTeamMember(id);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TeamMembers", redirectToActionResult.ActionName);
        Assert.Equal("Projects", redirectToActionResult.ControllerName);
    }

    [Fact]
    public async Task AddPeople_Returns_Unauthorized_For_Non_Admin_User()
    {
        // Arrange
        var projectId = 1;
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "User";

        var projectUser = new ProjectUser()
        {
            ProjectId = projectId,
            ProjectUserId = 2,
            RoleId = 1,
            UserEmail = "User2@email.com"
        };

        // Act
        var result = await controller.AddPeople(projectUser);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task AddPeople_Returns_RedirectToAction()
    {
        // Arrange
        var projectId = 1;
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        var projectUser = new ProjectUser()
        {
            ProjectId = projectId,
            ProjectUserId = 2,
            RoleId = 1,
            UserEmail = "User2@email.com"
        };

        // Act
        var result = await controller.AddPeople(projectUser);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TeamMembers", redirectToActionResult.ActionName);
        Assert.Equal("Projects", redirectToActionResult.ControllerName);
    }

    [Fact]
    public async Task EditProject_Returns_Unauthorized_For_Non_Admin_User()
    {
        // Arrange
        var projectId = 1;
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "User";

        // Act
        var result = await controller.EditProject(1);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    [Fact]
    public async Task DeleteConfirmed_Redirects_To_Index()
    {
        // Arrange
        var projectId = 1; // Sample project ID
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();

        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        // Act
        var result = await controller.DeleteConfirmed(projectId);

        // Assert
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }
}

