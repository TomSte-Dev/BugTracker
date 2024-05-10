using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTracker.Models;
using BugTracker.Repositories;
using BugTracker.Utility;
using Microsoft.CodeAnalysis;
using System.Data;

namespace BugTracker.Controllers;

public class TicketsController : Controller
{
    // Declare private ticket and project repositories
    private readonly ITicketRepository _ticketRepository;
    private readonly IProjectRepository _projectRepository;

    // Constructor with dependency injection for ITicketRepository and IProjectRepository
    // This constructor is used by the ASP.NET Core framework to inject instances of the required repositories.
    public TicketsController(ITicketRepository ticketRepository, IProjectRepository projectRepository)
    {
        // Assign injected instances to private fields
        _ticketRepository = ticketRepository;
        _projectRepository = projectRepository;
    }

    // GET: Tickets/Index
    // Displays the tickets for the specified project
    public async Task<IActionResult> Index(int? projectId)
    {
        // Check if projectId is null
        if (projectId == null)
        {
            return NotFound();
        }

        // Get the project by id from the repository
        var project = await _projectRepository.GetProjectById(projectId);

        // Check if the project is null
        if (project == null)
        {
            return NotFound();
        }

        // Set the current project in the singleton
        CurrentProjectSingleton.Instance.CurrentProject = project;

        // Check if the current user is assigned to the project
        bool isUserAssigned = await _projectRepository.IsUserAssignedToProject(projectId, User.Identity.Name);
        if (!isUserAssigned)
        {
            return Unauthorized();
        } 
        else
        {
            // Set the current user role in the singleton
            CurrentProjectSingleton.CurrentUserRole = 
                await _projectRepository.GetProjectUserRole
                (
                User.Identity.Name,
                CurrentProjectSingleton.Instance.CurrentProject.ProjectId
                );
        }

        // Get tickets for the specified project
        var tickets = _ticketRepository.AllTickets
            .Where(ticket => ticket.ProjectId == projectId)
            .OrderBy(ticket => ticket.DateCreated);

        // Get statuses from the repository and create a dictionary
        var statuses = _ticketRepository.AllStatuses.ToDictionary(status => status.StatusId, status => status.Name);

        // Pass the statuses dictionary to the view
        // This allows for the user to select an id by a display name
        ViewBag.StatusesDictionary = statuses;

        // Return the view with the list of tickets
        return View(tickets);
    }

    // GET: Tickets/Create
    // Displays the form for creating a new ticket
    public async Task<IActionResult> CreateTicket()
    {
        // Get the project id from the current project in the singleton
        int projectId = CurrentProjectSingleton.Instance.CurrentProject.ProjectId;

        // Get all statuses from the repository
        var statuses = _ticketRepository.AllStatuses;

        // Get user emails associated with the project from the repository
        var userEmails = await _projectRepository.GetUserEmailsByProjectId(projectId);

        // Allow the user to select from emails and status instead of entering a email or Id
        // Pass the list of user emails to the view using ViewBag
        ViewBag.UserEmails = new SelectList(userEmails);
        // Pass the list of statuses to the view using ViewBag
        ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name");

        // Return the view for creating a new ticket
        return View();
    }

    // POST: Tickets/Create
    // Handles the creation of a new ticket
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTicket([Bind("TicketId,ProjectId,Title,Description,StatusId,AssigneeEmail,ReporterEmail,DateCreated,LastUpdateTime,Comments")] Ticket ticket)
    {
        // Check if the model state is a valid ticket
        if (ModelState.IsValid)
        {
            // Create an empty list of comments and assign it to the ticket
            List<Comment> comment = new List<Comment>();
            ticket.Comments = comment;

            // Add the ticket to the repository
            await _ticketRepository.AddTicket(ticket);

            // Redirect to the ticket index page with the projectId
            return RedirectToAction("Index", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
        } else
        {
            // If the model state is not valid, return the view with the ticket
            return View(ticket);
        }
    }

    // GET: Tickets/EditTicket/5
    // Displays the form for editing a ticket with the specified id
    public async Task<IActionResult> EditTicket(int? id)
    {
        // Check if the id is null
        if (id == null)
        {
            return NotFound();
        }

        // Get the ticket by id from the repository
        var ticket = await _ticketRepository.GetTicketById(id);
        // Check if the ticket is null
        if (ticket == null)
        {
            return NotFound();
        }
        // Pass the ticket id to the view using ViewBag
        ViewBag.TicketId = ticket.TicketId;

        // Get the project id from the current project in the singleton
        int projectId = CurrentProjectSingleton.Instance.CurrentProject.ProjectId;

        // Get all statuses from the repository
        var statuses = _ticketRepository.AllStatuses;

        // Get user emails associated with the project from the repository
        var userEmails = await _projectRepository.GetUserEmailsByProjectId(projectId); // Task<IEnumerable<string>>

        // Allow the user to select from emails and status instead of entering a email or Id
        // Pass the list of user emails to the view using ViewBag
        ViewBag.UserEmails = new SelectList(userEmails);
        // Pass the list of statuses to the view using ViewBag
        ViewBag.Statuses = new SelectList(statuses, "StatusId", "Name");

        // Return the view for editing the ticket
        return View(ticket);
    }

    // POST: Tickets/EditTicket/5
    // Handles the editing of a ticket with the specified id
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTicket(int id, [Bind("TicketId,ProjectId,Title,Description,StatusId,AssigneeEmail,ReporterEmail,DateCreated,LastUpdateTime,Comments")] Ticket ticket)
    {
        // Check if the id in the URL matches the ticket's id
        if (id != ticket.TicketId)
        {
            return NotFound();
        }

        // Check if the ModelState is valid
        if (ModelState.IsValid)
        {
            try
            {
                // Attempt to update the ticket using the ticket repository
                await _ticketRepository.UpdateTicket(ticket);
            } catch (DbUpdateConcurrencyException)
            {
                // Check that the ticket to be updated exisits 
                if (!TicketExists(ticket.TicketId))
                {
                    // If the ticket doesn't exist, return NotFound
                    return NotFound();
                } else
                {
                    // If there's a conflict, throw exception
                    throw;
                }
            }
            // Redirect to the EditTicket action with the same ticket ID
            return RedirectToAction("EditTicket", "Tickets", new { id });
        }
        // If ModelState is not valid, return the view with the ticket data
        return View(ticket);
    }

    // Checks if a ticket with the specified ID exists in the repository
    private bool TicketExists(int id)
    {
        return _ticketRepository.AllTickets.Any(e => e.TicketId == id);
    }

    // POST: Tickets/Delete/5
    // Handles the deletion of a ticket with the specified id
    [HttpPost, ActionName("DeleteTicket")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        // Validate if id is null
        if (id == null)
        {
            return NotFound();
        }

        // Get the ticket by id from the repository
        var ticket = await _ticketRepository.GetTicketById(id);

        // Check if the ticket exists
        if (ticket == null)
        {
            return NotFound();
        }

        // Delete the ticket from the repository
        await _ticketRepository.DeleteTicketById(id);

        // Redirect to the ticket index page for the current project
        return RedirectToAction("Index", "Tickets", new { CurrentProjectSingleton.Instance.CurrentProject.ProjectId });
    }

    // POST: Tickets/AddComment/5
    // Handles the addition of a comment to a ticket
    [HttpPost]
    public async Task<IActionResult> AddComment(int? ticketId, [Bind("CommentId,TicketId,CommentText,CommentedBy,CommentDate")] Comment comment)
    {
        // Retrieve the ticket associated with the comment
        Ticket ticket = await _ticketRepository.GetTicketById(ticketId);

        // Check if the ticket is not found
        if (ticket == null)
        {
            return NotFound();
        }

        // Add the comment to the ticket in the repository
        await _ticketRepository.AddCommentToTicket(ticket, comment);

        // Redirect back to the ticket details page with the same ticket id
        return RedirectToAction("EditTicket", "Tickets", new { Id = ticketId });
    }
}
