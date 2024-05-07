namespace BugTracker.Models;

public class Ticket
{
    public int TicketId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int StatusId { get; set; }
    public string AssigneeEmail { get; set; }
    public string ReporterEmail { get; set; }
    //public string Priority { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdateTime { get; set; }

    public List<Comment> Comments { get; set; }
}