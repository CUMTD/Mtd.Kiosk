using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// A collection of endpoints for fetching departure data for display on kiosks.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("heartbeat")]
public class HeartbeatController : ControllerBase
{
	private readonly IHealthRepository _heartbeatRepository;
	private readonly IKioskRepository _kioskRepository;
	private readonly ILogger<HeartbeatController> _logger;

	/// <summary>
	/// Heartbeat controller constructor.
	/// </summary>
	/// <param name="heartbeatRepository"></param>
	/// <param name="kioskRepository"></param>
	/// <param name="logger"></param>
	public HeartbeatController(
		IHealthRepository heartbeatRepository,
		IKioskRepository kioskRepository,
		ILogger<HeartbeatController> logger)
	{
		ArgumentNullException.ThrowIfNull(heartbeatRepository, nameof(heartbeatRepository));
		ArgumentNullException.ThrowIfNull(kioskRepository, nameof(kioskRepository));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_heartbeatRepository = heartbeatRepository;
		_kioskRepository = kioskRepository;
		_logger = logger;
	}

	/// <summary>
	/// Ingest a heartbeat for an LED sign.
	/// </summary>
	/// <param name="kioskId">The kiosk Id, for logging a heartbeat</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPost("led")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> IngestLEDHeartbeat([FromQuery] string? kioskId, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(kioskId) || !Guid.TryParse(kioskId, out _))
		{
			return BadRequest("Not a valid GUID.");
		}

		// get heartbeat entry for LED

		var existingHeartbeat = await _heartbeatRepository.GetHeartbeatByIdentityAndTypeAsync(kioskId, HeartbeatType.LED, cancellationToken);

		try
		{
			// update the heartbeat's LastUpdated time

			if (existingHeartbeat != null)
			{
				existingHeartbeat.LastHeartbeat = DateTime.UtcNow;
			}
			else
			{
				// create a new heartbeat entry
				var heartbeat = new Health(kioskId, HeartbeatType.LED);
				await _heartbeatRepository.AddAsync(heartbeat, cancellationToken);
			}

			await _heartbeatRepository.CommitChangesAsync(cancellationToken);
			_logger.LogInformation("Logged LED heartbeat for kiosk {KioskId}", kioskId);
		}
		catch
		{
			_logger.LogError("Failed to log LED heartbeat for kiosk {KioskId}", kioskId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		return Created();
	}
}

