namespace BugTracker.Models;

using System;

// Represents a comment on a ticket in the BugTracker application
public class Comment
{
    // Unique identifier for the comment
    public int CommentId { get; set; }

    // Foreign key for the ticket associated with the comment
    public int TicketId { get; set; }

    // Text content of the comment
    public string CommentText { get; set; }

    // Email of the user who made the comment
    public string CommentedBy { get; set; }

    // Date and time when the comment was made
    public DateTime CommentDate { get; set; }
}
