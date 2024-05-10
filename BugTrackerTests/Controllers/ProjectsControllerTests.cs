using BugTracker.Controllers;
using BugTracker.Models;
using BugTracker.Utility;
using BugTrackerTests.Mocks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using System.Security.Claims;
using Project = BugTracker.Models.Project;

namespace BugTrackerTests.Controllers;

public class ProjectsControllerTests
{
    // Test method to ensure that Index action returns projects for the user
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

    // Test method to ensure that CreateProject action adds a project to the repository and redirects to Index
    [Fact]
    public async Task CreateProject_AddsProjectToRepository_RedirectsToIndex()
    {
        // Arrange
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();

        // Mock User.Identity.Name
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

        // Create sample project to pas into createProject
        var project = new Project
        {
            ProjectId = 2,
            Title = "New Project",
            Description = "New Project Description"
        };

        // Act
        var result = await controller.CreateProject(project);

        // Assert
        // Retreive the created project from the mock repo
        var createdProject = (await mockProjectRepository.Object.GetProjectsByUser(user.Identity.Name))
            .Where(p => p.ProjectId == project.ProjectId)
            .FirstOrDefault();

        // Check that its not null and that the passed in project is the same as the retieved
        Assert.NotNull(createdProject);
        Assert.Equal(project.Title, createdProject.Title);
        Assert.Equal(project.Description, createdProject.Description);

        // Redirection confirms success of the operation
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }

    // Test method to ensure that TeamMembers action returns Unauthorized for non-assigned user
    [Fact]
    public async Task TeamMembers_Returns_Unauthorized_For_Non_Assigned_User()
    {
        // Arrange
        var projectId = 1;
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();

        // Mock User.Identity.Name
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
        // Check for unauthorized status code
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    // Test method to ensure that TeamMembers action returns ViewResult for assigned user
    [Fact]
    public async Task TeamMembers_Returns_ViewResult_For_TeamMembers()
    {
        // Arrange
        var projectId = 1; 
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();

        // Mock User.Identity.Name
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
        // Checks for a view result with a valid model that is of type IEnumerable<ProjectUser>
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
        Assert.IsAssignableFrom<IEnumerable<ProjectUser>>(viewResult.Model);
    }

    // Test method to ensure that EditTeamMember action returns Unauthorized for non-admin user
    [Fact]
    public async Task EditTeamMember_Returns_Unauthorized_For_Non_Admin_User()
    {
        // Arrange
        var id = 1; 
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "User";

        // Create sample project user
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
        // Check for unauthorized status code
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    // Test method to ensure that EditTeamMember action returns NotFound for null ID
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
        // Check for 404 status code
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    // Test method to ensure that EditTeamMember action redirects to TeamMembers action for valid input
    [Fact]
    public async Task EditTeamMember_Returns_RedirectToAction_For_Valid_Input()
    {
        // Arrange
        var id = 1;
        var projectId = 1;

        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        // Create sample project user
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
        // Check that we are redirected to the right page
        // From this we can assume the operation was successful
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TeamMembers", redirectToActionResult.ActionName);
        Assert.Equal("Projects", redirectToActionResult.ControllerName);
    }

    // Test method to ensure that RemoveTeamMember action returns Unauthorized for non-admin user
    [Fact]
    public async Task RemoveTeamMember_Returns_Unauthorized_For_Non_Admin_User()
    {
        // Arrange
        var id = 1;
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();
        var controller = new ProjectsController(mockProjectRepository.Object);

        CurrentProjectSingleton.CurrentUserRole = "User"; // Set user role

        // Act
        var result = await controller.RemoveTeamMember(id);

        // Assert
        // Check for unauthorized status code
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    // Test method to ensure that RemoveTeamMember action redirects to TeamMembers action for valid input
    [Fact]
    public async Task RemoveTeamMember_Returns_RedirectToAction_For_Valid_Input()
    {
        // Arrange
        var id = 1;
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
        // By redirecting we can assume the operation was successful
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TeamMembers", redirectToActionResult.ActionName);
        Assert.Equal("Projects", redirectToActionResult.ControllerName);
    }

    // Test method to ensure that AddPeople action returns Unauthorized for non-admin user
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

        // Create sample user
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
        // Check for unauthorized status code
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    // Test method to ensure that AddPeople action redirects to TeamMembers action for admin user
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

        // Create sampple project user
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
        // By redirecting we can assume the operation was successful
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("TeamMembers", redirectToActionResult.ActionName);
        Assert.Equal("Projects", redirectToActionResult.ControllerName);
    }

    // Test method to ensure that EditProject action returns Unauthorized for non-admin user
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
        // Check for unauthorized status code
        var unauthorizedResult = Assert.IsType<UnauthorizedResult>(result);
        Assert.Equal(401, unauthorizedResult.StatusCode);
    }

    // Test method to ensure that DeleteConfirmed action redirects to Index action
    [Fact]
    public async Task DeleteConfirmed_Redirects_To_Index()
    {
        // Arrange
        var projectId = 1; 
        var mockProjectRepository = RepositoryMocks.GetProjectRepository();

        var controller = new ProjectsController(mockProjectRepository.Object);

        // Set the CurrentProject property directly
        CurrentProjectSingleton.Instance.CurrentProject = new Project { ProjectId = projectId };
        // Set the CurrentUserRole property directly
        CurrentProjectSingleton.CurrentUserRole = "Admin";

        // Act
        var result = await controller.DeleteConfirmed(projectId);

        // Assert
        // By redirecting we can assume the operation was successful
        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
    }
}

