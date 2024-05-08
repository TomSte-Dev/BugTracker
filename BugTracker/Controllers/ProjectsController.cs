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

namespace BugTracker.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly IProjectRepository _projectRepository;
        public ProjectsController(IProjectRepository projectRepository)
        {
            _projectRepository = projectRepository;

        }

        #region Project Creation

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            //Should only show projects this user has access to
            var projects = await _projectRepository.GetProjectsByUser(User.Identity.Name);
            return View(projects);
        }


        // GET: Projects/Create
        public IActionResult CreateProject()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject([Bind("ProjectId,Title,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                await _projectRepository.AddProject(project, User.Identity.Name);

                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> EditProject(int? id)
        {
            if (CurrentProjectSingleton.CurrentUserRole != "Admin")
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectRepository.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProject(int id, [Bind("ProjectId,Title,Description")] Project project)
        {
            if (CurrentProjectSingleton.CurrentUserRole != "Admin")
            {
                return Unauthorized();
            }

            if (id != project.ProjectId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _projectRepository.UpdateProject(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("DeleteProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (CurrentProjectSingleton.CurrentUserRole != "Admin")
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectRepository.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            await _projectRepository.DeleteProjectById(id);
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _projectRepository.AllProjects.Any(e => e.ProjectId == id);
        }

        #endregion

        #region Project Dashboard
        public async Task<IActionResult> TeamMembers(int? projectId)
        {
            if (projectId == null)
            {
                return NotFound();
            }

            // Should check that current user is a project user.
            bool isUserAssigned = await _projectRepository.IsUserAssignedToProject(projectId, User.Identity.Name);
            if (!isUserAssigned || CurrentProjectSingleton.CurrentUserRole != "Admin")
            {
                return Unauthorized();
            }

            var users = await _projectRepository.GetUsersByProjectId(projectId);

            // Fetch roles from the database
            var roles = _projectRepository.AllRoles;
            // Pass roles to the ViewBag
            ViewBag.Roles = roles;

            // Pass the list of statuses to the view
            ViewBag.RoleList = new SelectList(roles, "RoleId", "Title");

            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeamMember(int? id, [Bind("ProjectUserId,ProjectId,UserEmail,RoleId")] ProjectUser projectUser)
        {
            if (CurrentProjectSingleton.CurrentUserRole != "Admin")
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return NotFound();
            }

            await _projectRepository.UpdateProjectUser(projectUser);

            return RedirectToAction("TeamMembers", "Projects", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveTeamMember(int? id)
        {
            if (CurrentProjectSingleton.CurrentUserRole != "Admin")
            {
                return Unauthorized();
            }

            if (id == null)
            {
                return NotFound();
            }

            await _projectRepository.DeleteProjectUser(id);

            return RedirectToAction("TeamMembers", "Projects", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
        }

        public async Task<IActionResult> AddPeople([Bind("ProjectId,UserEmail,RoleId")] ProjectUser projectUser)
        {
            if (CurrentProjectSingleton.CurrentUserRole != "Admin")
            {
                return Unauthorized();
            }

            if (ModelState.IsValid)
            {
                // Check that user doesn't currently exist
                var userEmails = await _projectRepository.GetUserEmailsByProjectId(CurrentProjectSingleton.Instance.CurrentProject.ProjectId);
                if (!userEmails.Contains(projectUser.UserEmail))
                {
                    await _projectRepository.AddProjectUser(projectUser);
                }

                // Redirect to manage team
                return RedirectToAction("TeamMembers", "Projects", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
            }

            return View();
        }
        #endregion
    }
}
