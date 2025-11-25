using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using Mtd.Kiosk.RealTime;
using Mtd.Kiosk.RealTime.Entities.Api;
using Mtd.Stopwatch.Core.Entities.Transit;
using Mtd.Stopwatch.Core.Repositories.Transit;
using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// A collection of endpoints for fetching departure data for display on kiosks.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("departures")]
public class DeparturesController : ControllerBase
{
	private readonly ApiRealTimeClient _realTimeClient;
	private readonly IHealthRepository _heartbeatRepository;
	private readonly IKioskRepository _kioskRepository;
	private readonly IRouteRepository<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>> _routeRepository;
	private readonly IMemoryCache _cache;
	private readonly ILogger<DeparturesController> _logger;

	/// <summary>
	/// Constructor for the Departures controller.
	/// </summary>
	/// <param name="realTimeClient"></param>
	/// <param name="healthRepository"></param>
	/// <param name="kioskRepository"></param>
	/// <param name="routeRepository"></param>
	/// <param name="cache"></param>
	/// <param name="logger"></param>
	public DeparturesController(
		ApiRealTimeClient realTimeClient,
		IHealthRepository healthRepository,
		IKioskRepository kioskRepository,
		IRouteRepository<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>> routeRepository,
		IMemoryCache cache,
		ILogger<DeparturesController> logger)
	{
		ArgumentNullException.ThrowIfNull(realTimeClient, nameof(realTimeClient));
		ArgumentNullException.ThrowIfNull(healthRepository, nameof(healthRepository));
		ArgumentNullException.ThrowIfNull(kioskRepository, nameof(kioskRepository));
		ArgumentNullException.ThrowIfNull(routeRepository, nameof(routeRepository));
		ArgumentNullException.ThrowIfNull(cache, nameof(cache));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_realTimeClient = realTimeClient;
		_heartbeatRepository = healthRepository;
		_kioskRepository = kioskRepository;
		_routeRepository = routeRepository;
		_cache = cache;
		_logger = logger;
	}

	/// <summary>
	/// Get the next departures for an LED sign.
	/// </summary>
	/// <param name="stopId">The stop Id to fetch departures for</param>
	/// <param name="kioskId">The kiosk Id, for logging a heartbeat</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of LED departures</returns>
	[HttpGet("{stopId}/led")]
	[ProducesResponseType<IEnumerable<LedDeparture>>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<LedDeparture>>> GetLedDeparturesAsync([StopId(true)] string stopId, [FromQuery, GuidId(false)] string? kioskId, CancellationToken cancellationToken)
	{

		Departure[]? realTimeClientDepartures;
		try
		{
			realTimeClientDepartures = await _realTimeClient.GetRealTimeForStopsAsync([stopId], cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get departures from real-time client for {stopId}.", stopId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		if (realTimeClientDepartures == null)
		{
			_logger.LogError("Real-time client returned null for {stopId}.", stopId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		// the following code takes all departures and does some sorting and filtering.
		// it also converts the departures to LedDeparture response object.
		// it groups the departures by route and direction, then selects the first departure for each group.
		// this avoids using valuable LED real-estate to show an upcoming departure for a route that
		// already has a bus coming sooner.
		var departures = realTimeClientDepartures
			.OrderBy(departure => departure.MinutesTillDeparture)
			.ThenBy(departure => departure.RouteSortNumber)
			.GroupBy(departure => new { departure.FriendlyRouteName, departure.Direction })
			.Select(group => group.First())
			.Select(departure => new LedDeparture(departure));

		return Ok(departures);
	}

	/// <summary>
	/// Get the next departures for an LCD screen.
	/// </summary>
	/// <param name="primaryStopId">The primary stop Id to fetch departures for</param>
	/// <param name="additionalStopIds">The stop Ids to fetch departures for</param>
	/// <param name="kioskId">The kiosk Id, for logging a heartbeat</param>
	/// <param name="max">The maximum number of route groups to return</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of LLcdDepartureGroup objects</returns>
	[HttpGet("{primaryStopId}/lcd")]
	[ProducesResponseType<LcdDepartureResponseModel>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<LcdDepartureResponseModel>> GetLcdDeparturesAsync(
		[StopId(true)]
		string primaryStopId,
		[StopIdArray(true)]
		[FromQuery]
		string[] additionalStopIds,
		[GuidId(false)]
		string? kioskId,
		CancellationToken cancellationToken,
		[FromQuery, Range(1, int.MaxValue)]
		int max = int.MaxValue
	)
	{
		await LogHeartbeatAsync(HeartbeatType.LCD, kioskId);

		// combine the primary stop ID with the additional stop IDs into string[]
		var stopIds = new List<string> { primaryStopId };
		stopIds.AddRange(additionalStopIds);

		// fetch from API
		Departure[]? departures = null;
		try
		{
			departures = await _realTimeClient.GetRealTimeForStopsAsync(stopIds.ToArray(), cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get deparutres from real-time client for {stopIds}.", stopIds);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		// ensure API returned data.
		if (departures == null)
		{
			_logger.LogWarning("Real-time client returned null for {stopId}.", stopIds);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		// get all routes from the database
		// these will be used to enhance the response with route names and colors.
		var routes = await GetCacheAllRoutesAsync(cancellationToken);
		if (routes == null)
		{
			_logger.LogError("Could not load GTFS routes from DB so cannot show departures.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		// group departures by public route ID
		var groupedDepartures = departures
			.GroupBy(departure => new
			{
				publicRouteId = routes.FirstOrDefault(route => route.Id == departure.RouteId)?.PublicRouteId ?? "UNKNOWN_PUBLIC_ROUTE_ID",
				direction = departure.Direction
			})
			.Take(max);

		// convert to LCD departure response model
		var lcdDepartures = new List<LcdDepartureGroup>();
		foreach (var group in groupedDepartures)
		{
			// determine if the stop is across the street from the kiosk, default false if additionalStopIds is empty or null
			var isAcrossStreet = additionalStopIds.Length > 0 && group.First().StopId != primaryStopId;

			// get the next 3 departures for each route
			// and convert to response model
			var lcdDepartureTimes = group
				.OrderBy(departure => departure.MinutesTillDeparture)
				.ThenBy(departure => departure.RouteSortNumber)
				.ThenBy(departure => departure.RouteNumber)
				.Take(3)
				.Select(d => new LcdDepartureTime(d));

			// match the public route for this group.
			var publicRoute = routes
				.First(route => route.PublicRouteId != null && route.PublicRouteId == group.Key.publicRouteId)
				.PublicRoute;

			// public route should never be null here since we check for it in our first statement above
			var lcdDeparture = new LcdDepartureGroup(publicRoute!, lcdDepartureTimes, group.First().Direction, isAcrossStreet);

			lcdDepartures.Add(lcdDeparture);
		}

		// sort so that non isAcrossStreet routes will come immediately before isAcrossStreet routes
		lcdDepartures = [.. lcdDepartures
			.OrderBy(lcdDeparture => lcdDeparture.SortOrder)
			.ThenBy(lcdDeparture => lcdDeparture.IsAcrossStreet)];

		return Ok(new LcdDepartureResponseModel(lcdDepartures));

	}

	/// <summary>
	/// Get the next departures, formatted for an annunciator.
	/// </summary>
	/// <param name="stopId"></param>
	/// <param name="kioskId"></param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpGet("{stopId}/annunciator")]
	[ProducesResponseType<LcdDepartureResponseModel>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<AnnunciatorResponseModel>> GetAnnunciatorDeparturesAsync([StopId(true)] string stopId, [GuidId(false)] string kioskId, CancellationToken cancellationToken)
	{
		await LogHeartbeatAsync(HeartbeatType.Button, kioskId);

		Departure[]? realTimeClientDepartures;
		try
		{
			realTimeClientDepartures = await _realTimeClient.GetRealTimeForStopsAsync([stopId], cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to get departures from real-time client for {stopId}.", stopId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		if (realTimeClientDepartures == null)
		{
			_logger.LogError("Real-time client returned null for {stopId}.", stopId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		var departures = realTimeClientDepartures
			.OrderBy(departure => departure.MinutesTillDeparture)
			.ThenBy(departure => departure.RouteSortNumber)
			.GroupBy(departure => new { departure.FriendlyRouteName, departure.Direction })
			.Select(group => group.First())
			.Select(departure => new AnnunciatorDeparture(departure));

		return Ok(departures);

	}

	#region Helpers

	private async Task<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>?> GetCacheAllRoutesAsync(CancellationToken cancellationToken)
	{
		const string CACHE_KEY = "GtfsRoutes";

		if (!_cache.TryGetValue(CACHE_KEY, out IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>? publicRoutes))
		{
			try
			{
				publicRoutes = await _routeRepository.GetAllWithDirectionAsync(cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting public routes from database");
				return null;
			}

			_cache.Set(CACHE_KEY, publicRoutes, TimeSpan.FromMinutes(15));
		}

		return publicRoutes;
	}

	/// <summary>
	/// Logs the kiosk heartbeat using a "fire and forget" pattern.
	/// </summary>
	/// <param name="type">The type of component to log a heartbeat for.</param>
	/// <param name="kioskId">The ID of the kiosk to log. If this is null, this method will not do anything.</param>
	private async Task LogHeartbeatAsync(HeartbeatType type, string? kioskId)
	{
		if (string.IsNullOrWhiteSpace(kioskId))
		{
			_logger.LogDebug("No Kiosk ID provided, not logging heartbeat of type {heartbeatType}", type);
			return;
		}

		Core.Entities.Kiosk? kiosk;
		try
		{
			kiosk = await _kioskRepository.GetByIdentityOrDefaultAsync(kioskId, CancellationToken.None);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch kiosk {kioskId}", kioskId);
			return;
		}

		if (kiosk == null)
		{
			try
			{
				kiosk = await _kioskRepository.AddAsync(new Core.Entities.Kiosk(kioskId), CancellationToken.None);
				await _kioskRepository.CommitChangesAsync(CancellationToken.None);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to add new kiosk {kioskId}", kioskId);
				return;
			}
		}

		Health? heartbeat = null;
		try
		{
			heartbeat = await _heartbeatRepository.GetHeartbeatByIdentityAndTypeAsync(kioskId, type, CancellationToken.None);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch heartbeat.");
		}

		if (heartbeat != null)
		{
			heartbeat.LastHeartbeat = DateTime.UtcNow;
		}
		else
		{
			heartbeat = new Health(kioskId, type);
			await _heartbeatRepository.AddAsync(heartbeat, CancellationToken.None);
		}

		try
		{
			await _heartbeatRepository.CommitChangesAsync(CancellationToken.None);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to commit heartbeat {heartbeatType} for kiosk {kioskId}", heartbeat.Type, heartbeat.KioskId);
		}
	}

	#endregion Helpers
}
