using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Core.Repositories;

public interface ITemperatureDailyRepository : IRepository<TemperatureDaily>
{
	Task<IReadOnlyCollection<TemperatureDaily>> GetByKioskIdAsync(string kioskId, CancellationToken cancellationToken);
}
