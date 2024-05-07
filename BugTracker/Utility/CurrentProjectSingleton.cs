using BugTracker.Models;

namespace BugTracker.Utility;

// Singleton class to centralise storing the currently selected project
public class CurrentProjectSingleton
{
    // The single instance of this class
    private static CurrentProjectSingleton _instance;
    private static Project? _currentProject;
    private static string? _currentUserRole;


    // Private constructor to prevent external instantiation
    private CurrentProjectSingleton()
    {
        // Default value for the project
        // Set on first instantion
        _currentProject = null;
    }

    // Property to access the singleton instance
    public static CurrentProjectSingleton Instance
    {
        // Initialization of the singleton instance
        get
        {
            // If an instance doesnt exist we create one
            if (_instance == null)
            {
                _instance = new CurrentProjectSingleton();
            }
            return _instance;
        }
    }

    // Property to get or set the current selected project
    public Project? CurrentProject
    {
        get { return _currentProject; }
        set { _currentProject = value; }
    }

    public static string? CurrentUserRole
    {
        get { return _currentUserRole; }
        set { _currentUserRole = value; }
    }
}
