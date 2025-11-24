using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.RealTime.Config;
using Mtd.Kiosk.RealTime.Entities.Api;
using System.Text.Json;

namespace Mtd.Kiosk.RealTime;

public class ApiRealTimeClient
{
	public readonly ApiRealTimeClientConfig _config;
	public readonly HttpClient _client;
	public readonly ILogger<ApiRealTimeClient> _logger;

	public ApiRealTimeClient(IOptions<ApiRealTimeClientConfig> config, HttpClient client, ILogger<ApiRealTimeClient> logger)
	{
		ArgumentNullException.ThrowIfNull(config?.Value, nameof(config));
		ArgumentNullException.ThrowIfNull(client, nameof(client));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_config = config.Value;
		_client = client;
		_logger = logger;
	}

	public async Task<GeneralMessage[]?> GetGeneralMessagesAsync(CancellationToken cancellationToken)
	{

		HttpResponseMessage httpResponseMessage;
		try
		{
			// TODO DEV
			httpResponseMessage = await _client.GetAsync(_config.GmUri, cancellationToken);
			httpResponseMessage.EnsureSuccessStatusCode();

		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "Error getting general messages from realtime API. Server returned an {code} code.", ex.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting general messages from realtime API.");
			return null;
		}

		try
		{
			using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

			var gmResult = await JsonSerializer.DeserializeAsync<GeneralMessage[]>(contentStream, cancellationToken: cancellationToken);
			return gmResult;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deserializing general messaging");
			return null;
		}
	}

	public async Task<Departure[]?> GetRealTimeForStopsAsync(string[] stopIds, CancellationToken cancellationToken)
	{
		HttpResponseMessage httpResponseMessage;
		try
		{
			var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);

			for (int i = 0; i < stopIds.Length; i++)
			{
				queryString.Add("stopIds", stopIds[i]);
			}

			queryString.Add("previewTime", "60");

			var url = $"{_config.SmUri?.ToString()}?{queryString}";

			httpResponseMessage = await _client.GetAsync(url, cancellationToken);
			httpResponseMessage.EnsureSuccessStatusCode();
		}
		catch (HttpRequestException ex)
		{
			_logger.LogError(ex, "Error getting real-time data for stops {stopIds}. Server returned an {code} code.", stopIds, ex.StatusCode);
			return null;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting real-time data for stops {stopIds}", stopIds);
			return null;
		}

		try
		{
			using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken);

			if (contentStream.Length == 0)
			{
				_logger.LogWarning("Got a response with no content for stops {stopIds}", stopIds);
				return null;
			}

			var smResult = await JsonSerializer.DeserializeAsync<Departure[]>(contentStream, cancellationToken: cancellationToken);
			return smResult;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deserializing real-time data for stops {stopIds}", stopIds);
			return null;
		}
	}
}
