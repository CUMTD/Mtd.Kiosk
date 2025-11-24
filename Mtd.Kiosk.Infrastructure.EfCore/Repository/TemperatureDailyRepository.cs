using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class TemperatureDailyRepository(KioskContext context) : AsyncEFRepository<TemperatureDaily>(context), ITemperatureDailyRepository
{
	public async Task<IReadOnlyCollection<TemperatureDaily>> GetByKioskIdAsync(string kioskId, CancellationToken cancellationToken)
	{
		var temps = await _dbSet.Where(t => t.KioskId == kioskId).ToArrayAsync(cancellationToken);
		return temps;
	}
}
