using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Services;

/// <summary>
/// Background service that runs every 24 hours. Checks if there is a full previous day of data, and if there is, calculates some aggregated data points and deletes the data.
/// </summary>
public class ScopedTemperatureAggregatorWorker
{
	private readonly ITemperatureMinutelyRepository _temperatureRepository;
	private readonly ITemperatureDailyRepository _temperatureDailyRepository;
	private readonly ILogger<TemperatureAggregatorWorker> _logger;

	/// <summary>
	/// Constructor for ScopedTemperatureAggregatorWorker.
	/// </summary>
	/// <param name="temperatureRepository"></param>
	/// <param name="temperatureDailyRepository"></param>
	/// <param name="logger"></param>
	public ScopedTemperatureAggregatorWorker(
		ITemperatureMinutelyRepository temperatureRepository,
		ITemperatureDailyRepository temperatureDailyRepository,
		ILogger<TemperatureAggregatorWorker> logger)
	{
		_temperatureRepository = temperatureRepository;
		_temperatureDailyRepository = temperatureDailyRepository;
		_logger = logger;
	}

	internal async Task DoWorkAsync(CancellationToken cancellationToken)
	{
		var kioskIds = await _temperatureRepository.GetTemperatureLoggingKioskIds(cancellationToken);
		if (kioskIds == null || kioskIds.Count == 0)
		{
			_logger.LogWarning("No minutely temp data found.");
			return;
		}

		foreach (var kioskId in kioskIds)
		{
			var uniqueDays = await _temperatureRepository.GetUniqueDaysWithDataByKioskId(kioskId, cancellationToken);
			var daysToAggregate = uniqueDays.Where(d => d.Date != DateTime.Now.Date);

			var existingDaily = await _temperatureDailyRepository.GetByKioskIdAsync(kioskId, cancellationToken);
			var existingDates = existingDaily.Select(td => td.Date.Date).ToHashSet();

			foreach (var day in daysToAggregate)
			{
				if (existingDates.Contains(day.Date))
					continue;

				var aggregated = await _temperatureRepository.AggregateDayData(kioskId, day, cancellationToken);
				if (aggregated != null)
					await _temperatureDailyRepository.AddAsync(aggregated, cancellationToken);
				else
					_logger.LogWarning("Data aggregation returned null for kiosk {KioskId} on {Day}.", kioskId, day);
			}
		}

		await _temperatureDailyRepository.CommitChangesAsync(cancellationToken);
	}
}
