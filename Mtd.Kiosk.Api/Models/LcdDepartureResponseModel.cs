using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for the response of the LCD departure endpoint.
/// </summary>
/// <remarks>
/// Constructor for the LCD departure response model.
/// </remarks>
/// <param name="groupedDepartures">Up to three upcoming departures for the current route.</param>
public class LcdDepartureResponseModel(IEnumerable<LcdDepartureGroup> groupedDepartures)
{
	/// <summary>
	/// A list of upcoming departures (up to 3).
	/// </summary>
	[JsonPropertyName("groupedDepartures")]
	public IEnumerable<LcdDepartureGroup> Routes { get; set; } = groupedDepartures;
}

