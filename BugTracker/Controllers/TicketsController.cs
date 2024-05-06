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
            await Console.Out.WriteLineAsync(projectId.ToString());
            if (projectId == null)
            {
                return NotFound();
            }

            var project = await _projectRepository.GetProjectById(projectId);

            if (project != null)
            {
                CurrentProjectSingleton.Instance.CurrentProject = project;

                var tickets = _ticketRepository.AllTickets
                    .Where(ticket => ticket.ProjectId == projectId)
                    .OrderBy(ticket => ticket.DateCreated);

                return View(tickets);
            }
            return NotFound();
        }


        // GET: Tickets/Create
        public IActionResult Create()
        {
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
                return RedirectToAction(nameof(Index));
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

            var ticket = _ticketRepository.GetTicketById(id);
            if (ticket == null)
            {
                return NotFound();
            }
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
                return RedirectToAction(nameof(Index));
            }
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
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

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _ticketRepository.DeleteTicketById(id);
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
            return _ticketRepository.AllTickets.Any(e => e.TicketId == id);
        }
    }
}
