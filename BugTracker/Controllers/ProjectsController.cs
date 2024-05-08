using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using BugTracker.Areas.Identity.Data;
using System.Security.Claims;
using Microsoft.DotNet.Scaffolding.Shared.ProjectModel;
using BugTracker.Repositories;
using BugTracker.Utility;

namespace BugTracker.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectRepository _projectRepository;

    // Constructor to inject project repository
    public ProjectsController(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    #region Project Creation

    // GET: Projects/Index
    // Displays projects accessible to the current user
    public async Task<IActionResult> Index()
    {
        var projects = await _projectRepository.GetProjectsByUser(User.Identity.Name);
        return View(projects);
    }

    // GET: Projects/CreateProject
    // Displays the view to create a new project
    public IActionResult CreateProject()
    {
        return View();
    }

    // POST: Projects/CreateProject
    // Handles the creation of a new project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProject([Bind("ProjectId,Title,Description")] Project project)
    {
        // Check if the model state is valid
        if (ModelState.IsValid)
        {
            // Add the project using the repository
            await _projectRepository.AddProject(project, User.Identity.Name);

            // Redirect to the project index page
            return RedirectToAction(nameof(Index));
        }

        // If model state is not valid, return the view with the invalid project
        return View(project);
    }

    // GET: Projects/EditProject/5
    // Displays the view to edit a project with the given id
    public async Task<IActionResult> EditProject(int? id)
    {
        // Check if the user is not an admin
        if (CurrentProjectSingleton.CurrentUserRole != "Admin")
        {
            return Unauthorized();
        }

        // Check if the id is null
        if (id == null)
        {
            return NotFound();
        }

        // Get the project by id from the repository
        var project = await _projectRepository.GetProjectById(id);

        // Check if the project is null
        if (project == null)
        {
            return NotFound();
        }

        // Return the view with the project data
        return View(project);
    }

    // POST: Projects/EditProject/5
    // Handles the editing of a project with the given id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProject(int id, [Bind("ProjectId,Title,Description")] Project project)
    {
        // Check if the user is not an admin
        if (CurrentProjectSingleton.CurrentUserRole != "Admin")
        {
            return Unauthorized();
        }

        // Check if the id in the route does not match the project id in the model
        if (id != project.ProjectId)
        {
            return NotFound();
        }

        if(await _projectRepository.GetProjectById(project.ProjectId) != null && ModelState.IsValid) 
        {
            // Update the project using the repository
            await _projectRepository.UpdateProject(project);
            // Redirect to the index page of tickets for the current project
            return RedirectToAction("Index", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
        }

        // If model state is not valid, return the view with the invalid project
        return View(project);
    }

    // POST: Projects/DeleteProject/5
    // Handles the deletion of a project with the given id
    [HttpPost, ActionName("DeleteProject")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        // Check if the user is not an admin
        if (CurrentProjectSingleton.CurrentUserRole != "Admin")
        {
            return Unauthorized();
        }

        // Check if the id is null
        if (id == null)
        {
            return NotFound();
        }

        // Get the project by id from the repository
        var project = await _projectRepository.GetProjectById(id);

        // Check if the project is null
        if (project == null)
        {
            return NotFound();
        }

        // Delete the project using the repository
        await _projectRepository.DeleteProjectById(id);

        // Redirect to the index page of projects
        return RedirectToAction(nameof(Index));
    }
    #endregion

    #region Project Dashboard
    // GET: Projects/TeamMembers
    // Displays the team members for a project with the given projectId
    public async Task<IActionResult> TeamMembers(int? projectId)
    {
        // Check if projectId is null
        if (projectId == null)
        {
            return NotFound();
        }

        // Check if the current user is assigned to the project or is an admin
        bool isUserAssigned = await _projectRepository.IsUserAssignedToProject(projectId, User.Identity.Name);
        if (!isUserAssigned || CurrentProjectSingleton.CurrentUserRole != "Admin")
        {
            return Unauthorized();
        }

        // Get users assigned to the project
        var users = await _projectRepository.GetUsersByProjectId(projectId);

        // Fetch roles from the database
        var roles = _projectRepository.AllRoles;

        // Pass roles to the ViewBag
        ViewBag.Roles = roles;

        // Pass the list of roles to the view
        // Used within _AddPeople and _EditPeople to choose a role id by displaying the roles title for selection
        ViewBag.RoleList = new SelectList(roles, "RoleId", "Title");

        // Return the view with the list of users
        return View(users);
    }


    // POST: Projects/EditTeamMember
    // Handles the editing of a team member
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTeamMember(int? id, [Bind("ProjectUserId,ProjectId,UserEmail,RoleId")] ProjectUser projectUser)
    {
        // Check if the user is not an admin
        if (CurrentProjectSingleton.CurrentUserRole != "Admin")
        {
            return Unauthorized();
        }

        // Check if the id is null
        if (id == null)
        {
            return NotFound();
        }

        // Update the project user using the repository
        await _projectRepository.UpdateProjectUser(projectUser);

        // Redirect to the TeamMember page
        return RedirectToAction("TeamMembers", "Projects", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
    }

    // POST: Projects/RemoveTeamMember
    // Handles the removal of a team member
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTeamMember(int? id)
    {
        // Check if the user is not an admin
        if (CurrentProjectSingleton.CurrentUserRole != "Admin")
        {
            return Unauthorized();
        }

        // Check if the id is null
        if (id == null)
        {
            return NotFound();
        }

        // Delete the project user using the repository
        await _projectRepository.DeleteProjectUser(id);

        // Redirect to the TeamMembers page
        return RedirectToAction("TeamMembers", "Projects", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
    }


    // POST: Projects/AddPeople
    // Handles the addition of people (team members) to the project
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPeople([Bind("ProjectId,UserEmail,RoleId")] ProjectUser projectUser)
    {
        // Check if the user is not an admin
        if (CurrentProjectSingleton.CurrentUserRole != "Admin")
        {
            return Unauthorized();
        }

        // Check if the model state is valid
        if (ModelState.IsValid)
        {
            // Check if the user email is not already added to the project
            var userEmails = await _projectRepository.GetUserEmailsByProjectId(CurrentProjectSingleton.Instance.CurrentProject.ProjectId);
            if (!userEmails.Contains(projectUser.UserEmail))
            {
                // Add the project user using the repository
                await _projectRepository.AddProjectUser(projectUser);
            }

            // Redirect to the TeamMembers pagee
            return RedirectToAction("TeamMembers", "Projects", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
        }

        // If model state is not valid, return the view
        return View();
    }
    #endregion
}
