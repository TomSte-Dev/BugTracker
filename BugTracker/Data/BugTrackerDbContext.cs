using BugTracker.Areas.Identity.Data;
using BugTracker.Models;
using BugTracker.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BugTracker.Data;

public class BugTrackerDbContext : IdentityDbContext<BugTrackerUser>
{
    public BugTrackerDbContext(DbContextOptions<BugTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Status> Statuses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }

public DbSet<BugTracker.Models.Comment> Comment { get; set; } = default!;
}
