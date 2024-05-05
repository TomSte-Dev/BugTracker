using System.Net.Sockets;

namespace BugTracker.Models;

public class Project
{
    public int ProjectId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }


}