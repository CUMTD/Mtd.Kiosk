using Mtd.Stopwatch.Core.Entities.Schedule;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A route with its next three departures in the next 60 minutes.
/// </summary>
/// <remarks>
/// Constructor that maps a generic publicRoute and list of times to an LCD departure.
/// </remarks>
/// <param name="publicRoute">The public route from the database that is associated with this departure.</param>
/// <param name="lcdDepartures">All upcoming departure times for this route.</param>
/// <param name="direction">The direction associated with these trips.</param>
/// <param name="isAcrossStreet">Whether the stop is across the street from the kiosk.</param>
public class LcdDepartureGroup(PublicRoute publicRoute, IEnumerable<LcdDepartureTime> lcdDepartures, string direction, bool isAcrossStreet)
{
	/// <summary>
	/// The route number.
	/// </summary>
	[JsonPropertyName("number")]
	public string? Number { get; set; } = publicRoute.RouteNumber;

	/// <summary>
	/// The route name.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = publicRoute.PublicRouteGroup.RouteName;

	/// <summary>
	/// The direction/headsign of the route.
	/// </summary>
	[JsonPropertyName("direction")]
	public string Direction { get; set; } = direction;

	/// <summary>
	/// The foreground hex color of the route, including the #.
	/// </summary>
	[JsonPropertyName("foregroundHexColor")]
	public string ForegroundHexColor { get; set; } = $"#{publicRoute.PublicRouteGroup.HexTextColor}";
	/// <summary>
	/// The background hex color of the route, including the #.
	/// </summary>
	[JsonPropertyName("backgroundHexColor")]
	public string BackgroundHexColor { get; set; } = $"#{publicRoute.PublicRouteGroup.HexColor}";

	/// <summary>
	/// The sort order of the route.
	/// </summary>
	[JsonPropertyName("sortOrder")]
	public int SortOrder { get; set; } = publicRoute.PublicRouteGroup.SortNumber;

	/// <summary>
	/// Whether the stop is across the street from the kiosk.
	/// </summary>
	[JsonPropertyName("isAcrossStreet")]
	public bool IsAcrossStreet { get; set; } = isAcrossStreet;

	/// <summary>
	/// The departure times for the route.
	/// </summary>
	[JsonPropertyName("departureTimes")]
	public IEnumerable<LcdDepartureTime> DepartureTimes { get; set; } = lcdDepartures;
}
