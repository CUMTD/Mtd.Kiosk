namespace Mtd.Kiosk.Annunciator.Core.Config;

public class KioskConfig
{
	public const string ConfigSectionName = "Kiosk";
	public required string Name { get; set; }
	public required string KioskId { get; set; }
	public required string StopId { get; set; }
	public required string ApiKey { get; set; }

	public required string HeartbeatEndpoint { get; set; }
}
