namespace BugTracker.Models;

public class ProjectUser
{
    // The unique identifier for the project user
    public int ProjectUserId { get; set; }

    // The identifier of the project associated with this user
    public int ProjectId { get; set; }

    // The email of the user associated with this project
    public string UserEmail { get; set; } // UserId is a string in the identity framework

    // The identifier of the role assigned to this user in the project
    public int RoleId { get; set; }
}

