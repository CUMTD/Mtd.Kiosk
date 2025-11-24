using Mtd.Kiosk.Annunciator.Core.Models;

namespace Mtd.Kiosk.Annunciator.Core;

public interface IKioskRealTimeClient
{
	Task<IReadOnlyCollection<Departure>?> GetRealtime(string kioskId, string stopId, CancellationToken cancellationToken);
}
