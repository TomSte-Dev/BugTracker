using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.Repositories;

public interface IProjectRepository
{
    public IEnumerable<ProjectUser> GetProjectUsersByProjectId(int? id);

    IEnumerable<Project> AllProjects { get; }

    //CRUD
    Task AddProject(Project project, string currentUser);
    Project GetProjectById(int? id);
    Task UpdateProject(Project project);
    Task DeleteProjectById(int? id);


}
