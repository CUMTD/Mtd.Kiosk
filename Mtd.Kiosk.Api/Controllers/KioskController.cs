using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// A collection of endpoints for interacting and modifying kiosk data.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("kiosks")]
public class KioskController : ControllerBase
{
	private readonly IKioskRepository _kioskRepository;
	private readonly IHealthRepository _heartbeatRepository;
	private readonly ITicketRepository _ticketRepository;
	private readonly ILogger<KioskController> _logger;
	private readonly ApiConfiguration _apiConfiguration;

	/// <summary>
	/// Constructor for the Kiosk controller.
	/// </summary>
	/// <param name="kioskRepository"></param>
	/// <param name="heartbeatRepository"></param>
	/// <param name="ticketRepository"></param>
	/// <param name="logger"></param>
	/// <param name="apiConfiguration"></param>
	/// <exception cref="ArgumentNullException"></exception>
	public KioskController(IKioskRepository kioskRepository, IHealthRepository heartbeatRepository, ITicketRepository ticketRepository, IOptions<ApiConfiguration> apiConfiguration, ILogger<KioskController> logger)
	{
		ArgumentNullException.ThrowIfNull(kioskRepository);
		ArgumentNullException.ThrowIfNull(heartbeatRepository);
		ArgumentNullException.ThrowIfNull(ticketRepository);
		ArgumentNullException.ThrowIfNull(kioskRepository);
		ArgumentNullException.ThrowIfNull(logger);
		ArgumentNullException.ThrowIfNull(apiConfiguration);

		_kioskRepository = kioskRepository;
		_heartbeatRepository = heartbeatRepository;
		_ticketRepository = ticketRepository;
		_apiConfiguration = apiConfiguration.Value;
		_logger = logger;
	}

	/// <summary>
	/// Get a kiosk by its id.
	/// </summary>
	/// <param name="kioskId">The kiosk id to get.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A Kiosk object</returns>
	[HttpGet("{kioskId}")]
	[ProducesResponseType<Core.Entities.Kiosk>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Core.Entities.Kiosk>> GetKiosk([GuidId(true)] string kioskId, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Getting kiosk: {kioskId}", kioskId);
		Core.Entities.Kiosk kiosk;
		try
		{
			kiosk = await _kioskRepository.GetByIdentityWithTicketsAsync(kioskId, cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Kiosk not found: {kioskId}", kioskId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting kiosk: {kioskId}", kioskId);
			return StatusCode(500);
		}

		return Ok(kiosk);
	}

	/// <summary>
	/// Get all kiosks.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of all kiosks.</returns>
	[HttpGet]
	[ProducesResponseType<IEnumerable<Core.Entities.Kiosk>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<Core.Entities.Kiosk>>> GetAllKiosks(CancellationToken cancellationToken)
	{
		IReadOnlyCollection<Core.Entities.Kiosk> kiosks;
		try
		{
			_logger.LogDebug("Getting all kiosks");
			kiosks = await _kioskRepository.GetAllAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting all kiosks");
			return StatusCode(500);
		}

		return Ok(kiosks);
	}

	/// <summary>
	/// Create a new kiosk.
	/// </summary>
	/// <param name="createKiosk"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>The kiosk, if successfully added.</returns>
	[HttpPost]
	[ProducesResponseType<Core.Entities.Kiosk>(StatusCodes.Status201Created)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Core.Entities.Kiosk>> CreateKiosk([FromBody, Required] CreateKioskModel createKiosk, CancellationToken cancellationToken)
	{
		var kiosk = new Core.Entities.Kiosk(createKiosk.KioskId);
		try
		{
			await _kioskRepository.AddAsync(kiosk, cancellationToken);
			await _kioskRepository.CommitChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating kiosk with id {kioskId}", createKiosk.KioskId);
			return StatusCode(500);
		}

		return CreatedAtAction(nameof(GetKiosk), new { KioskId = kiosk.Id }, kiosk);
	}

	// TODO: make an object for this

	/// <summary>
	/// Get all kiosk health statuses.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of KioskHealthResponseModel objects</returns>
	[HttpGet("health")]
	[ProducesResponseType<IEnumerable<KioskHealthResponseModel>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<KioskHealthResponseModel>>> GetAllKioskHealth(CancellationToken cancellationToken)
	{
		_logger.LogDebug("Getting all kiosk health");

		IReadOnlyCollection<Core.Entities.Kiosk> kiosks;
		try
		{
			// get all kiosks, then get health for each kiosk, then return json object with health status of each kiosk
			kiosks = await _kioskRepository.GetAllAsync(true, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Unable to load kiosks from DB");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		// for each kiosk, get health status with KioskHealth (avoid DbContext threading issues)
		var kioskHealthTasks = kiosks.Select(k => KioskHealth(k.Id, cancellationToken).Result);
		return Ok(kioskHealthTasks);

	}

	/// <summary>
	/// Get the health status of a single kiosk.
	/// </summary>
	/// <param name="kioskId">The id of the kiosk to get the status for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpGet("{kioskId}/health")]
	[ProducesResponseType<KioskHealthResponseModel>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<KioskHealthResponseModel>> GetKioskHealth([GuidId(true)] string kioskId, CancellationToken cancellationToken)
	{
		try
		{
			var kioskHealth = await KioskHealth(kioskId, cancellationToken);
			return Ok(kioskHealth);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting health for kiosk: {kioskId}", kioskId);
			return StatusCode(500);
		}
	}

	/// <summary>
	/// Get tickets for a kiosk id.
	/// </summary>
	/// <param name="kioskId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of Ticket objects.</returns>
	[HttpGet("{kioskId}/tickets")]
	[ProducesResponseType<IEnumerable<Ticket>>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<Ticket>>> GetTicketsByKiosk([GuidId(true)] string kioskId, CancellationToken cancellationToken)
	{
		IEnumerable<Ticket> tickets;
		try
		{
			var dbTickets = await _ticketRepository.GetByKioskIdAsync(kioskId, cancellationToken);

			// sort into open and closed tickets
			tickets = (dbTickets?.OrderByDescending(t => t.Status == TicketStatusType.Open).ThenByDescending(t => t.OpenDate)) ?? Enumerable.Empty<Ticket>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting tickets for kiosk: {kioskId}", kioskId);
			return StatusCode(500);
		}

		return Ok(tickets);
	}

	#region Helpers

	// calculate health for each possible HeartbeatType
	private async Task<HealthStatus> CalculateHealth(string kioskId, HeartbeatType heartbeatType, CancellationToken cancellationToken)
	{
		Health? lastHeartbeat;
		try
		{
			lastHeartbeat = await _heartbeatRepository.GetHeartbeatByIdentityAndTypeAsync(kioskId, heartbeatType, cancellationToken);

		}
		catch (Exception ex)
		{
			// TODO: this was noisy in development
			_logger.LogError(ex, "Error getting heartbeats for kiosk: {kioskId}", kioskId);
			return HealthStatus.Unknown;
		}

		if (lastHeartbeat == null)
		{
			return HealthStatus.Unknown;
		}

		return lastHeartbeat.GetHealthStatusForTime(DateTime.UtcNow, _apiConfiguration.WarningHeartbeatThresholdMinutes, _apiConfiguration.CriticalHeartbeatThresholdMinutes);
	}

	/// <summary>
	/// Get the health status of a kiosk.
	/// </summary>
	/// <param name="kioskId">The kiosk id to fetch the health for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A KioskHealthResponseModel object.</returns>
	private async Task<KioskHealthResponseModel> KioskHealth(string kioskId, CancellationToken cancellationToken)
	{
		_logger.LogTrace("Getting health for kiosk: {kioskId}", kioskId);

		// TODO: implement this once buttons are sending heartbeats
		//var buttonHealth = HealthStatus.Unknown;
		var buttonHealth = await CalculateHealth(kioskId, HeartbeatType.Button, cancellationToken);
		var ledHealth = await CalculateHealth(kioskId, HeartbeatType.LED, cancellationToken);
		var lcdHealth = await CalculateHealth(kioskId, HeartbeatType.LCD, cancellationToken);

		var openTicketCount = 0;
		try
		{
			openTicketCount = await _ticketRepository.GetOpenTicketCountAsync(kioskId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Error getting open ticket count for kiosk: {kioskId}. Setting to zero for result.", kioskId);
		}

		var healthStatuses = new IndividualHealthStatuses(buttonHealth, ledHealth, lcdHealth);
		return new KioskHealthResponseModel(kioskId, healthStatuses, openTicketCount);
	}

	#endregion Helpers
}
