using Mtd.Stopwatch.Core.Entities.Schedule;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A route with its next three departures in the next 60 minutes for e-paper displays.
/// </summary>
/// <remarks>
/// Constructor that maps a generic publicRoute and list of times to an e-paper departure.
/// </remarks>
/// <param name="publicRoute">The public route from the database that is associated with this departure.</param>
/// <param name="departureTimes">All upcoming departure times for this route.</param>
/// <param name="direction">The direction associated with these trips.</param>
public class EPaperDepartureGroup(PublicRoute publicRoute, IEnumerable<EPaperDepartureTime> departureTimes, string direction)
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
	/// The sort order of the route.
	/// </summary>
	[JsonPropertyName("sortOrder")]
	public int SortOrder { get; set; } = publicRoute.PublicRouteGroup.SortNumber;

	/// <summary>
	/// The departure times for the route.
	/// </summary>
	[JsonPropertyName("departureTimes")]
 public IEnumerable<EPaperDepartureTime> DepartureTimes { get; set; } = departureTimes;
}
