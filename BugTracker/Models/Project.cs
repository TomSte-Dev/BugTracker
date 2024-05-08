namespace BugTracker.Models;

public class Project
{
    // The unique identifier for the project
    public int ProjectId { get; set; }

    // The title of the project
    public string Title { get; set; }

    // The description of the project
    public string Description { get; set; }
}
