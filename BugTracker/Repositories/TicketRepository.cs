using BugTracker.Data;
using BugTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace BugTracker.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly BugTrackerDbContext _context;

    // Constructor to initialize the repository with a BugTrackerDbContext instance
    public TicketRepository(BugTrackerDbContext context)
    {
        _context = context;
    }

    // Property to retrieve all tickets from the database
    public IEnumerable<Ticket> AllTickets
    {
        get { return _context.Tickets.ToList(); }
    }

    // Property to retrieve all statuses from the database
    public IEnumerable<Status> AllStatuses
    {
        get { return _context.Statuses.ToList(); }
    }

    // Add ticket to the database table
    public async Task AddTicket(Ticket ticket)
    {
        // Add the ticket to the context
        _context.Add(ticket);
        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    // Delete ticket from the database table by id
    public async Task DeleteTicketById(int? id)
    {
        // Find the ticket in the database by its ID
        var ticket = await _context.Tickets.FindAsync(id);
        // If the ticket exists
        if (ticket != null)
        {
            // Remove the ticket from the context
            _context.Tickets.Remove(ticket);
        }
        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    // Retrieves a ticket class by the tickets id within the table
    public async Task<Ticket?> GetTicketById(int? id)
    {
        // Retrieve the ticket by its ID including its comments
        return await _context.Tickets
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.TicketId == id);
    }

    // Updates ticket using the provided update
    // This uses the PK the ticket id to update the details of the entry
    public async Task UpdateTicket(Ticket ticket)
    {
        // Update the ticket in the context
        _context.Update(ticket);
        // Save changes to the database
        await _context.SaveChangesAsync();
    }

    // On crating a comment an entry is added to the comments table as well 
    // as adding it to the list of comments each ticket model contains.
    // this handles the addition of the provided comment to the provided ticket
    public async Task AddCommentToTicket(Ticket ticket, Comment comment)
    {
        // Add the new comment to the ticket's comments collection
        if (ticket.Comments == null)
        {
            ticket.Comments = new List<Comment>();
        }
        ticket.Comments.Add(comment);

        // Save changes to the database
        await _context.SaveChangesAsync();
    }

}
