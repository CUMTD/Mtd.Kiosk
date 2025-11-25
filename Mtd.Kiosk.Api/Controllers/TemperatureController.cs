using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// Controller for ingesting and processing kiosk temperature data.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("temperature")]
public class TemperatureController : ControllerBase
{
	private readonly ITemperatureMinutelyRepository _temperatureRepository;
	private readonly ITemperatureDailyRepository _temperatureDailyRepository;
	private readonly ILogger<TemperatureController> _logger;

	/// <summary>
	/// Constructor for TemperatureController.
	/// </summary>
	/// <param name="temperatureRepository"></param>
	/// <param name="temperatureDailyRepository"></param>
	/// <param name="logger"></param>
	public TemperatureController(
		ITemperatureMinutelyRepository temperatureRepository,
		ITemperatureDailyRepository temperatureDailyRepository,
		ILogger<TemperatureController> logger)
	{
		ArgumentNullException.ThrowIfNull(temperatureRepository, nameof(temperatureRepository));
		ArgumentNullException.ThrowIfNull(temperatureDailyRepository, nameof(temperatureDailyRepository));

		ArgumentNullException.ThrowIfNull(logger, nameof(logger));
		_temperatureRepository = temperatureRepository;
		_temperatureDailyRepository = temperatureDailyRepository;
		_logger = logger;
	}

	/// <summary>
	/// Logs the temperature and relative humidity for a kiosk.
	/// </summary>
	/// <param name="kioskId"></param>
	/// <param name="temp"></param>
	/// <param name="humidity"></param>
	/// <param name="sensorType"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPost("{kioskId}")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> LogKioskConditionsAsync([FromRoute] string kioskId, [FromQuery] byte temp, [FromQuery] byte humidity, [FromQuery] TemperatureSensorType sensorType, CancellationToken cancellationToken)
	{
		try
		{
			await _temperatureRepository.AddAsync(new TemperatureMinutely(kioskId, temp, humidity, sensorType), cancellationToken);
			await _temperatureRepository.CommitChangesAsync(cancellationToken);
			return Created();
		}
		catch (Exception e)
		{
			return Problem(e.Message);
		}
	}

	/// <summary>
	/// Gets the most recent temperature history for a kiosk.
	/// </summary>
	/// <param name="kioskId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpGet("{kioskId}/recent")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<IReadOnlyCollection<TemperatureMinutelyDataPoint>>> GetRecentTempHistoryAsync([FromRoute] string kioskId, CancellationToken cancellationToken)
	{
		try
		{
			var temps = await _temperatureRepository.GetTempsBetweenDatesAsync(kioskId, DateTimeOffset.Now.AddDays(-30).Date, DateTimeOffset.Now, cancellationToken);
			// convert to temp data points
			var tempDataPoints = temps.Where(t => t.SensorType == TemperatureSensorType.Adafruit).Select(t => new TemperatureMinutelyDataPoint(t)).ToArray();
			return Ok(tempDataPoints);
		}
		catch (Exception e)
		{
			return Problem(e.Message);
		}
	}

	/// <summary>
	/// Get the dailytemperature history for a kiosk.
	/// </summary>
	/// <param name="kioskId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpGet("{kioskId}/daily")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<IReadOnlyCollection<TemperatureMinutelyDataPoint>>> GetDailyTempHistoryAsync([FromRoute] string kioskId, CancellationToken cancellationToken)
	{
		try
		{
			var temps = await _temperatureDailyRepository.GetByKioskIdAsync(kioskId, cancellationToken);
			// convert to temp data points
			var tempDataPoints = temps.Select(t => new TemperatureDailyDataPoint(t)).ToArray();
			return Ok(tempDataPoints);
		}
		catch (Exception e)
		{
			return Problem(e.Message);
		}
	}

	/// <summary>
	/// Get the daily temperature history for all kiosks
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpGet("daily")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<IReadOnlyCollection<IReadOnlyCollection<TemperatureDaily>>>> GetAllDailyTempHistoryAsync(CancellationToken cancellationToken)
	{
		var allTempDataPoints = new List<IReadOnlyCollection<TemperatureDaily>>();

		var kiosks = await _temperatureRepository.GetTemperatureLoggingKioskIds(cancellationToken);

		for (var i = 0; i < kiosks.Count; i++)
		{
			var kioskId = kiosks.ElementAt(i);

			try
			{
				var temps = await _temperatureDailyRepository.GetByKioskIdAsync(kioskId, cancellationToken);
				// convert to temp data points
				var tempDataPoints = temps.ToList();

				allTempDataPoints.Add(tempDataPoints);

			}
			catch (Exception e)
			{
				_logger.LogError(e, "Error getting temperature data for kiosk {KioskId}", kioskId);
			}
		}

		return Ok(allTempDataPoints.Where(temps => temps.Any()).ToList());
	}
}
