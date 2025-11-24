using Mtd.Kiosk.RealTime.Entities.Api;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for the response to the general messaging endpoint.
/// </summary>
public class MessagingResponseModel
{
	/// <summary>
	/// The array of general messages.
	/// </summary>
	public IReadOnlyCollection<SimpleGeneralMessage> GeneralMessages { get; set; }

	/// <summary>
	/// Contructor that augments a GeneralMessage array to a MessagingResponseModel.
	/// </summary>
	/// <param name="messageResult"></param>
	public MessagingResponseModel(GeneralMessage[] messageResult)
	{
		var generalMessages = new List<SimpleGeneralMessage>();
		foreach (var message in messageResult)
		{
			if (message.StopIds != null && message.StopIds.Length > 0)
			{
				foreach (var stopId in message.StopIds)
				{
					var simpleGeneralMessage = new SimpleGeneralMessage(stopId, message.Text, message.BlockRealtime);
					generalMessages.Add(simpleGeneralMessage);
				}
			}
		}

		GeneralMessages = generalMessages;
	}
}
