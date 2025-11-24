using Mtd.Kiosk.RealTime.Entities.Api;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A simplified version of a departure for display on an LED.
/// </summary>
public class LedDeparture
{
	/// <summary>
	/// The name of the route.
	/// </summary>
	public string Route { get; set; }

	/// <summary>
	/// The time to display: usually "X mins", "DUE", or a "XX:XX PM" time.
	/// </summary>
	public string Time { get; set; }

	/// <summary>
	/// Constructor for LedDeparture.
	/// </summary>
	/// <param name="route"></param>
	/// <param name="time"></param>
	public LedDeparture(string route, string time)
	{
		Route = route;
		Time = time;
	}

	/// <summary>
	/// Constructor for LedDeparture.
	/// </summary>
	/// <param name="departure"></param>
	public LedDeparture(Departure departure)
	{

		Route = departure.FriendlyRouteName.ToUpper();
		Time = departure.DepartsIn;
	}
}
