using BugTracker.Areas.Identity.Data;
using BugTracker.Models;
using BugTracker.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace BugTracker.Data;

// Represents the database context for the BugTracker application
public class BugTrackerDbContext : IdentityDbContext<BugTrackerUser>
{
    // Constructor to initialize the database context
    public BugTrackerDbContext(DbContextOptions<BugTrackerDbContext> options)
        : base(options)
    {
    }

    // DbSet properties for entity types representing database tables
    public DbSet<Project> Projects { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<ProjectUser> ProjectUsers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Status> Statuses { get; set; }
    public DbSet<Comment> Comment { get; set; }

    // Override the OnModelCreating method to configure the model
    // Nothing additional is added here currently just the base implementation
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
