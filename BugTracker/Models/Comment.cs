namespace BugTracker.Models;

public class Comment
{
    public int CommentId { get; set; }
    public int TicketId { get; set; }
    public string CommentText { get; set; }
    public string CommentedBy { get; set; }
    public DateTime CommentDate { get; set; }
}
