using BugTracker.Data;

namespace BugTracker.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly BugTrackerDbContext _context;

    public TicketRepository(BugTrackerDbContext context)
    {
        _context = context;
    }
}
