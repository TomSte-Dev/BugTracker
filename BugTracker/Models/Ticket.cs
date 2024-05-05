namespace BugTracker.Models;

public class Ticket
{
    public int TicketId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int StatusId { get; set; }
    public int TicketAssigneeId { get; set; }
    public int ReporterId { get; set; }
    public string Priority { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime LastUpdateTime { get; set; }


}