using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Annunciator.Azure.Config;

public class AzureAnnunciatorConfig
{
	public const string ConfigSectionName = "AzureAnnunciator";

	[Required]
	public required string SubscriptionKey { get; set; }
	[Required]
	public required string ServiceRegion { get; set; }

	public string? SpeakerOutputDevice { get; set; } // https://learn.microsoft.com/en-us/azure/ai-services/speech-service/how-to-select-audio-input-devices
}
