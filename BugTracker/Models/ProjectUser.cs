using BugTracker.Areas.Identity.Data;
using System.Data;

namespace BugTracker.Models;

public class ProjectUser
{
    public int ProjectUserId { get; set; }
    public int ProjectId { get; set; }
    public string UserEmail { get; set; } // Assuming UserId is a string in your identity framework
    public int RoleId { get; set; }

}
