using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using System.Collections.Immutable;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class TicketRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, Ticket>(context), ITicketRepository
{
	public async Task<IReadOnlyCollection<Ticket>> GetAllOpenTicketsAsync(CancellationToken cancellationToken)
	{
		var openTickets = await _dbSet.Where(t => t.Status != TicketStatusType.Resolved).ToArrayAsync(cancellationToken);
		return openTickets.ToImmutableArray();
	}

	public Task<int> GetOpenTicketCountAsync(string kioskId, CancellationToken cancellationToken)
	{
		return _dbSet.CountAsync(t => t.KioskId == kioskId && t.Status == TicketStatusType.Open, cancellationToken);
	}

	public async Task<IReadOnlyCollection<Ticket>> GetByKioskIdAsync(string kioskId, CancellationToken cancellationToken)
	{
		var kiosk = await _dbSet
			.Where(t => t.KioskId == kioskId)
			.Include(t => t.Notes.Where(tn => !tn.Deleted).OrderByDescending(tn => tn.CreatedDate))
			.ToArrayAsync(cancellationToken);

		return kiosk.ToImmutableArray();

	}
}
