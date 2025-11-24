using Mtd.Kiosk.RealTime.Entities.Api;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A general message for the LCD.
/// </summary>
/// <remarks>
/// Constructor that maps a generic general message to an LCD general message.
/// </remarks>
/// <param name="message">The message to display.</param>
public class LcdGeneralMessage(GeneralMessage message)
{
	/// <summary>
	/// Whether the message blocks real-time departures.
	/// </summary>
	[JsonPropertyName("blocksRealtime")]
	public bool BlocksRealTime { get; set; } = message.BlockRealtime;
	/// <summary>
	/// The text of the message.
	/// </summary>
	[JsonPropertyName("text")]
	public string Text { get; set; } = message.Text;
}
