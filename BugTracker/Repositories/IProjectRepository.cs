using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Repositories;

public interface IProjectRepository
{

    IEnumerable<Project> AllProjects { get; }
    IEnumerable<Role> AllRoles { get; }

    //CRUD
    Task AddProject(Project project, string currentUser);
    Task<Project?> GetProjectById(int? id);
    Task UpdateProject(Project project);
    Task DeleteProjectById(int? id);

    // Project users
    public Task<IEnumerable<string>> GetUserEmailsByProjectId(int? id);
    public Task<IEnumerable<ProjectUser>> GetUsersByProjectId(int? id);
    public Task<IEnumerable<Project>> GetProjectsByUser(string user);

    public Task<bool> IsUserAssignedToProject(int? projectId, string? user);

    public Task AddProjectUser(ProjectUser projectUser);

    public Task UpdateProjectUser(ProjectUser projectUser);

    public Task DeleteProjectUser(int? projectUserId);

    public Task<string?> GetProjectUserRole(string userEmail, int? projectId);
}
