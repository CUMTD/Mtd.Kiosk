namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// A simplified version of a general message.
/// </summary>
/// <remarks>
/// Constructor for SimpleGeneralMessage.
/// </remarks>
/// <param name="stopId"></param>
/// <param name="message"></param>
/// <param name="blockRealtime"></param>
public class SimpleGeneralMessage(string stopId, string message, bool blockRealtime)
{
	/// <summary>
	/// The stop id of the general message.
	/// </summary>
	public string StopId { get; set; } = stopId;
	/// <summary>
	/// The message to display.
	/// </summary>
	public string Message { get; set; } = message;

	/// <summary>
	/// Whether to block realtime updates when displaying the message.
	/// </summary>
	public bool BlockRealtime { get; set; } = blockRealtime;
}

