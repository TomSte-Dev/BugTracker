using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Repositories;

public interface IProjectRepository
{

    IEnumerable<Project> AllProjects { get; }

    //CRUD
    Task AddProject(Project project, string currentUser);
    Task<Project?> GetProjectById(int? id);
    Task UpdateProject(Project project);
    Task DeleteProjectById(int? id);

    // Project users
    public Task<IEnumerable<ProjectUser>> GetProjectUsersByProjectId(int? id);
    public Task<IEnumerable<Project>> GetProjectsByUser(string user);

    public Task<bool> IsUserAssignedToProject(int? projectId, string user);

}
