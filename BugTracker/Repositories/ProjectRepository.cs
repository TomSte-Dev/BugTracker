using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly BugTrackerDbContext _context;

    // Constructor to initialize the repository with a BugTrackerDbContext instance
    public ProjectRepository(BugTrackerDbContext context)
    {
        _context = context;
    }

    // Retrieves all projects from the database and orders them by title
    public IEnumerable<Project> AllProjects
    {
        get { return _context.Projects.ToList().OrderBy(t => t.Title); }
    }

    // Retrieves all roles from the database
    public IEnumerable<Role> AllRoles
    {
        get { return _context.Roles.ToList(); }
    }

    // Adds a new project to the database
    public async Task AddProject(Project project, string currentUser)
    {
        // Add the project to the context
        _context.Projects.Add(project);
        // Save changes to the database
        await _context.SaveChangesAsync();

        // Create a project user for the current user with 'Admin' role
        var firstUser = new ProjectUser
        {
            ProjectId = project.ProjectId,
            UserEmail = currentUser,
            // Get the role ID of the 'Admin' role from the database
            RoleId = _context.Roles
                .Where(r => r.Title == "Admin")
                .Select(i => i.RoleId)
                .FirstOrDefault()
        };

        // Add the project user to the context
        if (firstUser != null)
            _context.ProjectUsers.Add(firstUser);

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task<Project?> GetProjectById(int? id)
    {
        // Retrieve a project from the database by its ID
        return await _context.Projects.FindAsync(id);
    }

    public async Task UpdateProject(Project project)
    {
        // Update the project entity in the context
        _context.Update(project);
        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProjectById(int? id)
    {
        // Find the project in the database by its ID
        var project = await _context.Projects.FindAsync(id);

        // If the project exists
        if (project != null)
        {
            // Find all users associated with the project and remove them
            var users = _context.ProjectUsers.Where(p => p.ProjectId == id).ToList();
            _context.ProjectUsers.RemoveRange(users);

            // Remove the project itself
            _context.Projects.Remove(project);
        }

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    // Gets a list of user email strings 
    public async Task<IEnumerable<string>> GetUserEmailsByProjectId(int? id)
    {
        // Retrieves distinct user emails associated with the specified project ID
        var userEmails = await _context.ProjectUsers
            .Where(u => u.ProjectId == id) // Filter by project ID
            .Select(u => u.UserEmail) // Select only the UserEmail property
            .Distinct() // Get distinct user emails
            .ToListAsync();

        return userEmails;
    }

    // Boolean check for conditional checks within the controller
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

    // Retrieves projects associated with a specific user.
    public async Task<IEnumerable<Project>> GetProjectsByUser(string user)
    {
        // Retrieve project IDs associated with the specified user
        var projectIds = await _context.ProjectUsers
            .Where(pu => pu.UserEmail == user) // Filter by user email
            .Select(pu => pu.ProjectId) // Select only the ProjectId property
            .ToListAsync();

        // Retrieve projects based on the project IDs
        var projects = await _context.Projects
            .Where(p => projectIds.Contains(p.ProjectId)) // Filter projects based on projectIds
            .ToListAsync();

        return projects;
    }

    // Retrieves users associated with a specific project.
    public async Task<IEnumerable<ProjectUser>> GetUsersByProjectId(int? id)
    {
        // Retrieve users associated with the specified project ID
        var users = await _context.ProjectUsers
            .Where(u => u.ProjectId == id) // Filter by project ID
            .OrderBy(u => u.UserEmail) // Order users by email
            .ToListAsync();

        return users;
    }

    // Adds a new project user to the database.
    public async Task AddProjectUser(ProjectUser projectUser)
    {
        // Add project user asynchronously
        await _context.ProjectUsers.AddAsync(projectUser);
        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    // Updates an existing project user in the database.
    public async Task UpdateProjectUser(ProjectUser projectUser)
    {
        // Update project user
        _context.ProjectUsers.Update(projectUser);
        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    // Deletes a project user from the database by its ID.
    public async Task DeleteProjectUser(int? projectUserId)
    {
        // Find the project user by its ID
        var user = await _context.ProjectUsers.FindAsync(projectUserId);
        // If the project user exists
        if (user != null)
        {
            // Remove user from project users
            // Keeps any tickets they have made or used 
            _context.ProjectUsers.Remove(user);
            // Save changes to the database
            await _context.SaveChangesAsync();
        }
    }

    // Retrieves the role of a user in a specific project.
    // Used to validate that they are an admin before displaying certain information such as the admin panel options
    public async Task<string?> GetProjectUserRole(string userEmail, int? projectId)
    {
        // Find the ProjectUser record matching the userEmail and projectId
        var projectUser = await _context.ProjectUsers
            .FirstOrDefaultAsync(pu => pu.UserEmail == userEmail && pu.ProjectId == projectId);

        // If the project user exists
        if (projectUser != null)
        {
            // Find the role associated with the project user
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == projectUser.RoleId);

            // If the role exists, return its title
            if (role != null)
            {
                return role.Title;
            }
        }

        // Return null if project user or role does not exist
        return null;
    }
}
