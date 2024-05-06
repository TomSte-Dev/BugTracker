using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly BugTrackerDbContext _context;

    public ProjectRepository(BugTrackerDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Project> AllProjects
    {
        get { return _context.Projects.ToList().OrderBy(t => t.Title); }
    }

    public async Task AddProject(Project project, string currentUser)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        var firstUser = new ProjectUser
        {
            ProjectId = project.ProjectId,
            UserEmail = currentUser,
            RoleId = _context.Roles
                .Where(r => r.Title == "Admin")
                .Select(i => i.RoleId)
                .FirstOrDefault()
        };

        if (firstUser != null)
            _context.ProjectUsers.Add(firstUser);

        await _context.SaveChangesAsync();
    }

    public async Task<Project?> GetProjectById(int? id)
    {
        return await _context.Projects.FindAsync(id);
    }

    public async Task UpdateProject(Project project)
    {
        _context.Update(project);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProjectById(int? id)
    {
        var project = _context.Projects.Find(id);
        if (project != null)
        {
            var users = _context.ProjectUsers.Where(p => p.ProjectId == id).ToList();
            _context.ProjectUsers.RemoveRange(users);
            _context.Projects.Remove(project);
        }

        await _context.SaveChangesAsync();
    }

    public IEnumerable<ProjectUser> GetProjectUsersByProjectId(int? id)
    {
        return _context.ProjectUsers.Where(u => u.ProjectId == id).ToList();
    }

}
