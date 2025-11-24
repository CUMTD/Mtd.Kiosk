using Microsoft.EntityFrameworkCore;
using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class TemperatureMinutelyRepository(KioskContext context) : AsyncEFRepository<TemperatureMinutely>(context), ITemperatureMinutelyRepository
{
	public async Task<TemperatureDaily?> AggregateDayData(string kioskId, DateTime date, CancellationToken cancellationToken)
	{
		var query = _dbSet.Where(t => t.KioskId == kioskId && t.Timestamp.Date == date.Date);

		if (!await query.AnyAsync())
		{
			return null;
		}

		var tempMin = await query.MinAsync(t => t.TempFahrenheit, cancellationToken);
		var tempMax = await query.MaxAsync(t => t.TempFahrenheit, cancellationToken);
		var tempAvg = (byte)await query.AverageAsync(t => t.TempFahrenheit, cancellationToken);

		var relHumidityMin = await query.MinAsync(t => t.RelHumidity, cancellationToken);
		var relHumidityMax = await query.MaxAsync(t => t.RelHumidity, cancellationToken);
		var relHumidityAvg = (byte)await query.AverageAsync(t => t.RelHumidity, cancellationToken);

		return new TemperatureDaily(kioskId, date, tempMin, tempMax, tempAvg, relHumidityMin, relHumidityMax, relHumidityAvg);
	}

	public async Task DeleteDataByDate(DateTime date, CancellationToken cancellationToken)
	{
		await _dbSet.Where(td => td.Timestamp.Date == date.Date).ExecuteDeleteAsync(cancellationToken);
	}

	public async Task<IReadOnlyCollection<TemperatureMinutely>> GetDayDataAsync(string kioskId, DateTime date, CancellationToken cancellationToken)
	{
		var temps = await _dbSet.Where(t => t.Timestamp.Date == date.Date).ToArrayAsync();
		return temps;
	}

	public async Task<IReadOnlyCollection<TemperatureMinutely>> GetTempsBetweenDatesAsync(string kioskId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken)
	{

		var temps = await _dbSet
			.Where(t => t.KioskId == kioskId && t.Timestamp >= start && t.Timestamp <= end)
			.OrderBy(t => t.Timestamp)
			.ToArrayAsync(cancellationToken);

		return temps;

	}

	public async Task<IReadOnlyCollection<string>> GetTemperatureLoggingKioskIds(CancellationToken cancellationToken)
	{
		var kioskIds = await _dbSet.Select(t => t.KioskId).Distinct().ToArrayAsync(cancellationToken);
		return kioskIds;
	}

	public async Task<IReadOnlyCollection<DateTime>> GetUniqueDaysWithDataByKioskId(string kioskId, CancellationToken cancellationToken)
	{
		var days = await _dbSet
			.Where(t => t.KioskId == kioskId)
			.Select(t => t.Timestamp.Date)
			.Distinct()
			.ToArrayAsync(cancellationToken);

		return days;
	}

	public Task<IReadOnlyCollection<TemperatureMinutely>> GetPastMonthTempsAsync(string kioskId, CancellationToken cancellationToken) => throw new NotImplementedException();
}
