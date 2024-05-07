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
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectId,Title,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                await _projectRepository.AddProject(project, User.Identity.Name);

                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit(int id, [Bind("ProjectId,Title,Description")] Project project)
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
        [HttpPost, ActionName("Delete")]
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

            return RedirectToAction("TeamMembers", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
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

            return RedirectToAction("TeamMembers", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
        }

        #endregion
    }
}
