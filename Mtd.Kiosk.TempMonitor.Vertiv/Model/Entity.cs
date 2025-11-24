using System.Text.Json.Serialization;

namespace Mtd.Kiosk.TempMonitor.Vertiv.Model;

public class Entity
{
	[JsonPropertyName("measurement")]
	public Dictionary<string, Measurement> Measurements { get; set; } = default!;
}
