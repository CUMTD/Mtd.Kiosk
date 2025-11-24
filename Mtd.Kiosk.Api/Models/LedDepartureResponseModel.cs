namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for the response to a request for LED departures.
/// </summary>
/// <remarks>
/// Constructor for LedDepartureResponseModel.
/// </remarks>
/// <param name="departures"></param>
public class LedDepartureResponseModel(IReadOnlyCollection<LedDeparture> departures)
{
	/// <summary>
	/// The departures to display on the LED.
	/// </summary>
	public IReadOnlyCollection<LedDeparture> Departures { get; set; } = departures;
}
