using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Data;
using BugTracker.Models;
using BugTracker.Repositories;
using BugTracker.Utility;
using Microsoft.CodeAnalysis;
using System.Data;

namespace BugTracker.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IProjectRepository _projectRepository;

        public TicketsController(ITicketRepository ticketRepository, IProjectRepository projectRepository)
        {
            _ticketRepository = ticketRepository;
            _projectRepository = projectRepository;
        }

        // GET: Tickets
        public async Task<IActionResult> Index(int? projectId)
        {
            if (projectId == null)
            {
                return NotFound();
            }

            var project = await _projectRepository.GetProjectById(projectId);
            if (project == null)
            {
                return NotFound();
            }

            CurrentProjectSingleton.Instance.CurrentProject = project;

            // Should check that current user is a project user.
            bool isUserAssigned = await _projectRepository.IsUserAssignedToProject(projectId, User.Identity.Name);
            if (!isUserAssigned)
            {
                return Unauthorized();
            } 
            else
            {
                CurrentProjectSingleton.CurrentUserRole = await _projectRepository.GetProjectUserRole(
                    User.Identity.Name,
                    CurrentProjectSingleton.Instance.CurrentProject.ProjectId
                    );
            }


            var tickets = _ticketRepository.AllTickets
                .Where(ticket => ticket.ProjectId == projectId)
                .OrderBy(ticket => ticket.DateCreated);


            var statuses = _ticketRepository.AllStatuses.ToDictionary(status => status.StatusId, status => status.Name);
            ViewBag.StatusesDictionary = statuses;

            return View(tickets);
        }

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

        public async Task<IActionResult> AddPeople([Bind("ProjectId,UserEmail,RoleId")] ProjectUser projectUser)
        {
            if(CurrentProjectSingleton.CurrentUserRole != "Admin")
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
                return RedirectToAction("TeamMembers", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
            }

            return View();
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {

            int projectId = CurrentProjectSingleton.Instance.CurrentProject.ProjectId;
            var statuses = _ticketRepository.AllStatuses;

            var userEmails = await _projectRepository.GetUserEmailsByProjectId(projectId);

            // Pass the list of user emails to the view
            ViewBag.UserEmails = new SelectList(userEmails);

            // Pass the list of statuses to the view
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name");



            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TicketId,ProjectId,Title,Description,StatusId,AssigneeEmail,ReporterEmail,DateCreated,LastUpdateTime")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                await _ticketRepository.AddTicket(ticket);
                // Redirect with the projectId
                return RedirectToAction("Index", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });

            }
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _ticketRepository.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }

            int projectId = CurrentProjectSingleton.Instance.CurrentProject.ProjectId;
            var statuses = _ticketRepository.AllStatuses;

            var userEmails = await _projectRepository.GetUserEmailsByProjectId(projectId); // Task<IEnumerable<string>>

            // Pass the list of user emails to the view
            ViewBag.UserEmails = new SelectList(userEmails);

            // Pass the list of statuses to the view
            ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name");


            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TicketId,ProjectId,Title,Description,StatusId,AssigneeEmail,ReporterEmail,DateCreated,LastUpdateTime")] Ticket ticket)
        {
            if (id != ticket.TicketId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _ticketRepository.UpdateTicket(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.TicketId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                // Redirect with the projectId
                return RedirectToAction("Index", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
            }
            return View(ticket);
        }


        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            // Validate Id
            if (id == null)
            {
                return NotFound();
            }

            // Validate ticket
            var ticket = await _ticketRepository.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }



            await _ticketRepository.DeleteTicketById(id);
            // Redirect with the projectId
            return RedirectToAction("Index", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
        }

        private bool TicketExists(int id)
        {
            return _ticketRepository.AllTickets.Any(e => e.TicketId == id);
        }
    }
}
