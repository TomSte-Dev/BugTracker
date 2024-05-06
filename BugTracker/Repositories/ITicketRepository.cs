using BugTracker.Models;

namespace BugTracker.Repositories;

public interface ITicketRepository
{
    IEnumerable<Ticket> AllTickets { get; }
    IEnumerable<Status> AllStatuses { get; }

    //CRUD
    Task AddTicket(Ticket ticket);
    Task<Ticket?> GetTicketById(int? id);
    Task UpdateTicket(Ticket ticket);
    Task DeleteTicketById(int? id);

}
