using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Config;

/// <summary>
/// Configuration for the API. Includes thresholds for warning and critical heartbeats.
/// </summary>
public class ApiConfiguration
{
	/// <summary>
	/// The number of minutes between the last heartbeat and the current time that will trigger a warning.
	/// </summary>
	[Required]
	[Range(1, int.MaxValue)]
	public required int WarningHeartbeatThresholdMinutes { get; set; }

	/// <summary>
	/// The number of minutes between the last heartbeat and the current time that will trigger a critical alert.
	/// </summary>
	[Required]
	[Range(1, int.MaxValue)]
	public required int CriticalHeartbeatThresholdMinutes { get; set; }
}
