using BugTracker.Models;

namespace BugTracker.Repositories;

public interface IProjectRepository
{
    #region Properties

    // Get all projects from the repository.
    IEnumerable<Project> AllProjects { get; }

    // Get all roles from the repository.
    IEnumerable<Role> AllRoles { get; }

    #endregion

    #region CRUD Operations

    // Add a new project to the repository.
    Task AddProject(Project project, string currentUser);

    // Retrieve a project by its ID from the repository.
    Task<Project?> GetProjectById(int? id);

    // Update an existing project in the repository.
    Task UpdateProject(Project project);

    // Delete a project by its ID from the repository.
    Task DeleteProjectById(int? id);

    #endregion

    #region Project users

    // Retrieve user emails associated with a project by its ID.
    Task<IEnumerable<string>> GetUserEmailsByProjectId(int? id);

    // Retrieve users associated with a project by its ID.
    Task<IEnumerable<ProjectUser>> GetUsersByProjectId(int? id);

    // Retrieve projects associated with a user.
    Task<IEnumerable<Project>> GetProjectsByUser(string user);

    // Check if a user is assigned to a project.
    Task<bool> IsUserAssignedToProject(int? projectId, string? user);

    // Add a user to a project.
    Task AddProjectUser(ProjectUser projectUser);

    // Update a user's association with a project.
    Task UpdateProjectUser(ProjectUser projectUser);

    // Delete a user's association with a project by the user's ID.
    Task DeleteProjectUser(int? projectUserId);

    // Retrieve the role of a user in a project by user email and project ID.
    Task<string?> GetProjectUserRole(string userEmail, int? projectId);
    #endregion
}