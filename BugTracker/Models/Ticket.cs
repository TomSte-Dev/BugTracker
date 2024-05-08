namespace BugTracker.Models;

public class Ticket
{
    // PK Unique identifier for the ticket
    public int TicketId { get; set; }

    // ID of the project to which the ticket belongs
    public int ProjectId { get; set; }

    // Title of the ticket
    public string Title { get; set; }

    // Description of the ticket
    public string Description { get; set; }

    // ID of the status of the ticket FK for the status table
    public int StatusId { get; set; }

    // Email address of the user assigned to the ticket
    public string AssigneeEmail { get; set; }

    // Email address of the user who reported the ticket
    public string ReporterEmail { get; set; }

    //  Date and time when the ticket was created
    public DateTime DateCreated { get; set; }

    // Date and time when the ticket was last updated
    public DateTime LastUpdateTime { get; set; }

    // List of comments associated with the ticket. refers to the comments table
    public List<Comment> Comments { get; set; }
}