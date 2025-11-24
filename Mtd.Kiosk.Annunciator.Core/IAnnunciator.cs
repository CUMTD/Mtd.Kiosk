using Mtd.Kiosk.Annunciator.Core.Models;

namespace Mtd.Kiosk.Annunciator.Core;

public interface IAnnunciator
{
	Task ReadDepartures(string stopName, IReadOnlyCollection<Departure>? departures, CancellationToken cancellationToken);
}
