using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for the response of the e-paper departure endpoint.
/// </summary>
/// <remarks>
/// Constructor for the e-paper departure response model.
/// </remarks>
/// <param name="groupedDepartures">Up to three upcoming departures for the current route.</param>
public class EPaperDepartureResponseModel(IEnumerable<EPaperDepartureGroup> groupedDepartures)
{
	/// <summary>
	/// A list of upcoming departures (up to 3).
	/// </summary>
	[JsonPropertyName("groupedDepartures")]
	public IEnumerable<EPaperDepartureGroup> Routes { get; set; } = groupedDepartures;
}
