using Mtd.Kiosk.Core.Entities;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A collection of health statuses for each component.
/// </summary>
/// <remarks>
/// Constructor for IndividualHealthStatuses.
/// </remarks>
/// <param name="buttonStatus">The current health status for the annunciator button.</param>
/// <param name="ledStatus">The current health status for the LED sign.</param>
/// <param name="lcdStatus">The current health status for the LCD display.</param>
public class IndividualHealthStatuses(HealthStatus buttonStatus, HealthStatus ledStatus, HealthStatus lcdStatus)
{
	/// <summary>
	/// The health status of the button.
	/// </summary>
	[JsonPropertyName("button")]
	public HealthStatus ButtonStatus { get; set; } = buttonStatus;

	/// <summary>
	/// The health status of the LED.
	/// </summary>
	[JsonPropertyName("led")]
	public HealthStatus LedStatus { get; set; } = ledStatus;

	/// <summary>
	/// The health status of the LCD.
	/// </summary>
	[JsonPropertyName("lcd")]
	public HealthStatus LcdStatus { get; set; } = lcdStatus;
	internal HealthStatus GetOverallStatus() => new[] { ButtonStatus, LedStatus, LcdStatus }.Max();
}
