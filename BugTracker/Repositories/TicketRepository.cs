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
        return await _context.Tickets.FindAsync(id);
    }

    public async Task UpdateTicket(Ticket ticket)
    {
        _context.Update(ticket);
        await _context.SaveChangesAsync();
    }
}
