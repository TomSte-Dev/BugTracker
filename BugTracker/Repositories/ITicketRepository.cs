using BugTracker.Models;

namespace BugTracker.Repositories;

public interface ITicketRepository
{
    #region Properties

    // Gets all tickets from the repository.
    IEnumerable<Ticket> AllTickets { get; }

    // Gets all statuses from the repository.
    IEnumerable<Status> AllStatuses { get; }

    #endregion

    #region CRUD Operations

    // Adds a new ticket to the repository.
    Task AddTicket(Ticket ticket);

    // Retrieves a ticket by its ID from the repository.
    Task<Ticket?> GetTicketById(int? id);

    // Updates an existing ticket in the repository.
    Task UpdateTicket(Ticket ticket);

    // Deletes a ticket by its ID from the repository.
    Task DeleteTicketById(int? id);

    #endregion

    // Add comments to a ticket.
    Task AddCommentToTicket(Ticket ticket, Comment comment);
}
