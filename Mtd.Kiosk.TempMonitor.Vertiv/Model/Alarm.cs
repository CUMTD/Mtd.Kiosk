using System.Text.Json.Serialization;

namespace Mtd.Kiosk.TempMonitor.Vertiv.Model;

public class Alarm
{
	[JsonPropertyName("state")]
	public string? State { get; set; }

	[JsonPropertyName("severity")]
	public string? Severity { get; set; }
}
