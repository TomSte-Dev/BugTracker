using Microsoft.AspNetCore.Identity;

namespace BugTracker.Areas.Identity.Data;

// Add profile data for application users by adding properties to the BugTrackerUser class
public class BugTrackerUser : IdentityUser
{
    // Name added for viewing purposes
    // Currently utilised in the top navigation bar
    public string FirstName { get; set; }
    public string LastName { get; set; }

}

