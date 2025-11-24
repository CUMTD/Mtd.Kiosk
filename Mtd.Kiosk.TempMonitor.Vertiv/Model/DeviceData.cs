using System.Text.Json.Serialization;

namespace Mtd.Kiosk.TempMonitor.Vertiv.Model;

public class DeviceData
{
	[JsonPropertyName("entity")]
	public Dictionary<string, Entity> Entities { get; set; } = default!;

	// Remaining properties are included but not required for core functionality
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("state")]
	public string? State { get; set; }

	[JsonPropertyName("alarm")]
	public Alarm? Alarm { get; set; }

	[JsonPropertyName("name")]
	public string? Name { get; set; }
}
