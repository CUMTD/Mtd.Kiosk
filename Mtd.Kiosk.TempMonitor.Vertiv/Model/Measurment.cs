using System.Text.Json.Serialization;

namespace Mtd.Kiosk.TempMonitor.Vertiv.Model;

public class Measurement
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("value")]
	[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
	public double Value { get; set; }

	[JsonPropertyName("units")]
	public string? Units { get; set; }
}
