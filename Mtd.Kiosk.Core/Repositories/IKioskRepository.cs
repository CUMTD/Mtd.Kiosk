namespace Mtd.Kiosk.Core.Repositories;

public interface IKioskRepository : IIdentityRepository<Entities.Kiosk>
{

	Task<Entities.Kiosk> GetByIdentityWithTicketsAsync(string identity, CancellationToken cancellationToken);
	Task<IReadOnlyCollection<Entities.Kiosk>> GetAllAsync(bool includeTickets, CancellationToken cancellationToken);

}
