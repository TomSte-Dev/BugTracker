namespace BugTracker.Models;

// Represents the model for displaying error information in views
public class ErrorViewModel
{
    // The unique identifier for the request associated with the error
    public string? RequestId { get; set; }

    // Indicates whether to show the request identifier in the view
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
