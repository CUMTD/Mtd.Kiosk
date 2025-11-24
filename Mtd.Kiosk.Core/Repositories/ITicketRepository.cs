using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories;

public interface ITicketRepository : IIdentityRepository<Ticket>
{
	Task<IReadOnlyCollection<Ticket>> GetByKioskIdAsync(string kioskId, CancellationToken cancellationToken);

	Task<IReadOnlyCollection<Ticket>> GetAllOpenTicketsAsync(CancellationToken cancellationToken);

	Task<int> GetOpenTicketCountAsync(string kioskId, CancellationToken cancellationToken);

}
