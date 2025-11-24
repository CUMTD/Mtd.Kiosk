namespace Mtd.Kiosk.Core.Entities;

/// <summary>
/// The current health status of a component in a kiosk.
/// </summary>
public enum HealthStatus
{
	/// <summary>
	/// Health status is unknown.
	/// </summary>
	Unknown = -1,
	/// <summary>
	/// Kiosk is healthy.
	/// </summary>
	Healthy = 0,
	/// <summary>
	/// Kiosk health status is in a warning state.
	/// </summary>
	Warning = 1,
	/// <summary>
	/// Kiosk is unhealth.
	/// </summary>
	Critical = 2,
}
