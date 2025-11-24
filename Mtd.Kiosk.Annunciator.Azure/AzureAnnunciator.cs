using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Azure.Config;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Core.Models;
using System.Security;
using System.Text;

namespace Mtd.Kiosk.Annunciator.Azure;

public class AzureAnnunciator : IAnnunciator
{
	private readonly ILogger<AzureAnnunciator> _logger;
	private readonly SpeechSynthesizer _synth;

	public AzureAnnunciator(IOptions<AzureAnnunciatorConfig> config, ILogger<AzureAnnunciator> logger)
	{
		ArgumentNullException.ThrowIfNull(config?.Value, nameof(config));
		ArgumentException.ThrowIfNullOrWhiteSpace(config.Value.SubscriptionKey, nameof(config.Value.SubscriptionKey));
		ArgumentException.ThrowIfNullOrWhiteSpace(config.Value.ServiceRegion, nameof(config.Value.ServiceRegion));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		var speechConfig = SpeechConfig.FromSubscription(config.Value.SubscriptionKey, config.Value.ServiceRegion);
		if (config.Value.SpeakerOutputDevice == null)
		{
			// use default output device if none specified
			_synth = new SpeechSynthesizer(speechConfig, AudioConfig.FromDefaultSpeakerOutput());
		}
		else
		{
			_synth = new SpeechSynthesizer(speechConfig, AudioConfig.FromSpeakerOutput(config.Value.SpeakerOutputDevice));
		}

		_logger = logger;
	}

	public async Task ReadDepartures(string stopName, IReadOnlyCollection<Departure>? departures, CancellationToken cancellationToken)
	{
		// no departures
		if (departures == null)
		{
			_logger.LogWarning("Departures object was null");
			await ReadError();
		}
		else if (departures.Count == 0)
		{
			_logger.LogInformation("No upcoming departures");
			await ReadLine("There are no departures in the next sixty minutes.");
		}
		else
		{
			_logger.LogInformation("Reading departures");
			await ReadLine($"Departures for {stopName} as of {DateTime.Now:h:mm tt}");
			// read each line
			foreach (var departure in departures)
			{
				await ReadLine($"{departure.Name} {departure.JoinWord} {departure.Time}");
			}
		}
	}

	public Task ReadError() =>
			ReadLine("There was an error loading departures. Please try again later or call 384-8188.");

	private async Task ReadLine(string text)
	{
		using var result = await _synth.SpeakSsmlAsync(GenerateSsml(text));

		if (result.Reason == ResultReason.SynthesizingAudioCompleted)
		{
			_logger.LogDebug("Speech synthesized to speaker for text [{text}], id={id}", text, result.ResultId);
		}
		else if (result.Reason == ResultReason.Canceled)
		{
			var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
			_logger.LogWarning("CANCELED: Reason={reason}, id={id}", cancellation.Reason, result.ResultId);

			if (cancellation.Reason == CancellationReason.Error)
			{
				_logger.LogError("CANCELED: ErrorCode={error}, ErrorDetails=[{details}], id={id}", cancellation.ErrorCode, cancellation.ErrorDetails, result.ResultId);
			}
		}
		else
		{
			_logger.LogWarning("Unknown error Reason={reason}, id={id}", result.Reason.ToString(), result.ResultId);
		}

	}

	/// <summary>
	/// Create SSML XML for reading text.
	/// </summary>
	/// <param name="text">The text to read.</param>
	/// <remarks>https://docs.microsoft.com/en-us/azure/cognitive-services/speech-service/speech-synthesis-markup?tabs=csharp</remarks>
	/// <returns>XML</returns>
	private string GenerateSsml(string text)
	{
		var sb = new StringBuilder();

		sb.AppendLine("<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"https://www.w3.org/2001/mstts\" xml:lang=\"en-US\">");
		sb.AppendLine("\t<voice name=\"en-US-AriaNeural\">");
		sb.AppendLine("\t\t<mstts:express-as style=\"customerservice\">");
		sb.AppendLine($"\t\t\t{EscapeTextSaml(text)}");
		sb.AppendLine("\t\t</mstts:express-as>");
		sb.AppendLine("\t</voice>");
		sb.AppendLine("</speak>");

		var xml = sb.ToString();

		_logger.LogTrace("Generated SSML '{text}'", xml);

		return xml;
	}

	private static string EscapeTextSaml(string text) => SecurityElement.Escape(text);
}
