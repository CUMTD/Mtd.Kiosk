using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories;

public interface IHealthRepository : IRepository<Health>
{
	Task<Health?> GetHeartbeatByIdentityAndTypeAsync(string identity, HeartbeatType heartbeatType, CancellationToken cancellationToken);
}
