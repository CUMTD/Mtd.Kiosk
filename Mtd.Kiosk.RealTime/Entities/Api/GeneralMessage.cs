using System.Text.Json.Serialization;

namespace Mtd.Kiosk.RealTime.Entities.Api;

public class GeneralMessage
{
	[JsonPropertyName("id")]
	public required string Id { get; set; }
	[JsonPropertyName("displayId")]
	public required string DisplayId { get; set; }

	[JsonPropertyName("text")]
	public required string Text { get; set; }

	[JsonPropertyName("startTime")]
	public required DateTime StartTime { get; set; }

	[JsonPropertyName("endTime")]
	public required DateTime EndTime { get; set; }

	[JsonPropertyName("blockRealtime")]
	public required bool BlockRealtime { get; set; }

	[JsonPropertyName("stopIds")]
	public required string[] StopIds { get; set; }
}
