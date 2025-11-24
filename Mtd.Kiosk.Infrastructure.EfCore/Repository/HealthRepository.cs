using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class HealthRepository(KioskContext context) : AsyncEFRepository<Health>(context), IHealthRepository
{

	public async Task<Health?> GetHeartbeatByIdentityAndTypeAsync(string identity, HeartbeatType heartbeatType, CancellationToken cancellationToken)
	{
		var heartbeat = await _dbSet
			.Where(h => h.KioskId == identity && h.Type == heartbeatType)
			.FirstOrDefaultAsync(cancellationToken);

		return heartbeat;
	}
}
