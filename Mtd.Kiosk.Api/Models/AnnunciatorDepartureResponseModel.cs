namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for the response to a request for Annunciator departures.
/// </summary>
/// <remarks>
/// Constructor for LedDepartureResponseModel.
/// </remarks>

public class AnnunciatorResponseModel(IReadOnlyCollection<AnnunciatorDeparture> departures)
{
	/// <summary>
	/// The departures to display on the LED.
	/// </summary>
	public IReadOnlyCollection<AnnunciatorDeparture> Departures { get; set; } = departures;
}
