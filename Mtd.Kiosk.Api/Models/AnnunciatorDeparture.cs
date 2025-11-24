using System.Text.RegularExpressions;
using Departure = Mtd.Kiosk.RealTime.Entities.Api.Departure;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A simplified version of a departure for placyback on an annunciator.
/// </summary>
/// <remarks>
/// Constructor
/// </remarks>
/// <param name="departure"></param>
public class AnnunciatorDeparture(Departure departure)
{
	private static readonly string[] _nonDestinationStrings = ["express"];

	/// <summary>
	/// The route number as a string.
	/// </summary>
	public string Number { get; set; } = departure.RouteNumber;

	/// <summary>
	/// The route direction (e.g. West, North, A, U)
	/// </summary>
	public string Direction { get; set; } = departure.Direction;

	/// <summary>
	/// The route name/color
	/// </summary>
	public string Name
	{
		get
		{
			var routeName = departure.RouteId.ToLower();
			if (routeName.Contains("hopper"))
			{
				var removedHopper = routeName.Replace("hopper", "");
				return removedHopper + " hopper";
			}
			else
			{
				return routeName;
			}
		}
	}

	/// <summary>
	/// The route modifier, e.g. "to LSQ"
	/// </summary>
	/// 
	public string Modifier
	{
		get
		{

			var routeIdWithoutHopper = Regex.Replace(departure.RouteId, "hopper", string.Empty, RegexOptions.IgnoreCase);
			var destination = Regex.Replace(departure.RouteColor, $"{routeIdWithoutHopper}|hopper|{departure.RouteColor}", string.Empty, RegexOptions.IgnoreCase).Trim();

			if (destination.Length > 0)
			{
				if (_nonDestinationStrings.Any(str => string.Equals(str, destination, StringComparison.OrdinalIgnoreCase)))
				{
					return destination;
				}

				return destination.ToLower();
			}

			return string.Empty;

		}
		set
		{
			// Do nothing
		}
	}
	/// <summary>
	/// The time until the departure, e.g "1 min," "5 mins," "9:06 AM"
	/// </summary>
	public string DepartsIn { get; set; } = departure.DepartsIn;

	/// <summary>
	/// Whether the departure is realtime tracked.
	/// </summary>
	public bool IsRealtime { get; set; } = departure.IsRealTime;

}
