using BugTracker.Data;

namespace BugTracker.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly BugTrackerDbContext _context;

    public ProjectRepository(BugTrackerDbContext context)
    {
        _context = context;
    }
}
