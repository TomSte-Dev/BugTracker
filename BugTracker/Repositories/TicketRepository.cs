using BugTracker.Data;
using BugTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Sockets;

namespace BugTracker.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly BugTrackerDbContext _context;

    public TicketRepository(BugTrackerDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Ticket> AllTickets
    {
        get { return _context.Tickets.ToList(); }
    }

    public IEnumerable<Status> AllStatuses
    {
        get { return _context.Statuses.ToList();}
    }

    public async Task AddTicket(Ticket ticket)
    {
        _context.Add(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteTicketById(int? id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Ticket?> GetTicketById(int? id)
    {
        return await _context.Tickets
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.TicketId == id);
    }

    public async Task UpdateTicket(Ticket ticket)
    {
        _context.Update(ticket);
        await _context.SaveChangesAsync();
    }

    public async Task AddCommentToTicket(Ticket ticket, Comment comment)
    {
        // Add the new comment to the ticket's comments collection
        if(ticket.Comments == null)
        {
            List<Comment> newComments = new List<Comment>();
            ticket.Comments = newComments;
        }
        ticket.Comments.Add(comment);

        // Save changes to the database
        await _context.SaveChangesAsync();
    }
}
