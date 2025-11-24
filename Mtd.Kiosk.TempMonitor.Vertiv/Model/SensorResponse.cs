using System.Text.Json.Serialization;

namespace Mtd.Kiosk.TempMonitor.Vertiv.Model;

public class SensorResponse
{
	[JsonPropertyName("data")]
	public Dictionary<string, DeviceData> Data { get; set; } = default!;

	[JsonPropertyName("retCode")]
	public int RetCode { get; set; }

	[JsonPropertyName("retMsg")]
	public string? RetMsg { get; set; }
}
