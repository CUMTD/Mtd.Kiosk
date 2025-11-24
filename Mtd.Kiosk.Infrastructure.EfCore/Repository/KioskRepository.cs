using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Repositories;
using System.Collections.Immutable;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class KioskRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, Core.Entities.Kiosk>(context), IKioskRepository
{
	public Task<Core.Entities.Kiosk> GetByIdentityWithTicketsAsync(string identity, CancellationToken cancellationToken)
	{
		return _dbSet.Include(k => k.Tickets)
			.SingleAsync(k => k.Id == identity, cancellationToken);
	}

	async Task<IReadOnlyCollection<Core.Entities.Kiosk>> IKioskRepository.GetAllAsync(bool includeTickets, CancellationToken cancellationToken)
	{
		if (includeTickets)
		{
			var kiosks = await _dbSet.Include(k => k.Tickets).ThenInclude(t => t.Notes).ToArrayAsync(cancellationToken);

			return kiosks.ToImmutableArray();
		}
		else
		{
			var kiosks = await _dbSet.ToArrayAsync(cancellationToken);
			return kiosks.ToImmutableArray();

		}
	}
}
