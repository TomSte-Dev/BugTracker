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
    public IEnumerable<Role> AllRoles 
    { 
        get { return _context.Roles.ToList(); } 
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

    public async Task<IEnumerable<string>> GetUserEmailsByProjectId(int? id)
    {
        var userEmails = await _context.ProjectUsers
            .Where(u => u.ProjectId == id)
            .Select(u => u.UserEmail) // Select only the UserEmail property
            .Distinct() // Get distinct user emails
            .ToListAsync();

        return userEmails;
    }

    public async Task<bool> IsUserAssignedToProject(int? projectId, string? user)
    {
        // Check if projectId has a value
        if (!projectId.HasValue)
        {
            return false;
        }

        // Query to check if any project user matches the conditions
        bool isAssigned = await _context.ProjectUsers
            .AnyAsync(project => project.ProjectId == projectId && project.UserEmail == user);

        return isAssigned;
    }

    public async Task<IEnumerable<Project>> GetProjectsByUser(string user)
    {
        var projectIds = await _context.ProjectUsers
            .Where(pu => pu.UserEmail == user)
            .Select(pu=>pu.ProjectId)
            .ToListAsync();

        var projects = await _context.Projects
            .Where(p => projectIds.Contains(p.ProjectId)) // Filter projects based on projectIds
            .ToListAsync();

        return projects;
    }

    public async Task<IEnumerable<ProjectUser>> GetUsersByProjectId(int? id)
    {
        var users = await _context.ProjectUsers
            .Where(u => u.ProjectId == id)
            .ToListAsync();

        return users;
    }
}
