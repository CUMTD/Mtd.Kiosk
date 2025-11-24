using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Core.Config;
using Mtd.Kiosk.Annunciator.Core.Models;
using Mtd.Kiosk.Annunciator.Realtime.KioskApi.DTO;
using System.Collections.Immutable;
using System.Text.Json;

namespace Mtd.Kiosk.Annunciator.Realtime.KioskApi;

public class KioskApiRealtimeClient : IKioskRealTimeClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<KioskApiRealtimeClient> _logger;
	private readonly string _departuresUrlTemplate;

	private static readonly JsonSerializerOptions _jsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
	};

	public KioskApiRealtimeClient(HttpClient httpClient, IOptions<RealTimeClientConfig> options, ILogger<KioskApiRealtimeClient> logger)
	{
		ArgumentNullException.ThrowIfNull(httpClient, nameof(httpClient));
		ArgumentNullException.ThrowIfNull(options?.Value, nameof(options));
		ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.RealTimeAddressTemplate, nameof(options.Value.RealTimeAddressTemplate));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_httpClient = httpClient;
		_departuresUrlTemplate = options.Value.RealTimeAddressTemplate;
		_logger = logger;
	}
	public async Task<IReadOnlyCollection<Departure>?> GetRealtime(string kioskId, string stopId, CancellationToken cancellationToken)
	{
		HttpResponseMessage responseMessage;
		try
		{
			responseMessage = await GetDeparturesFromServer(kioskId, stopId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch departures");
			return null;
		}

		if (!responseMessage.IsSuccessStatusCode)
		{
			_logger.LogError("Got status code {statusCode} from departures API", responseMessage.StatusCode);
			return null;
		}

		try
		{
			var departures = await ConvertResults(responseMessage, cancellationToken);
			return departures;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch convert departures");
			return null;
		}
	}

	private async Task<HttpResponseMessage> GetDeparturesFromServer(string kioskId, string stopId, CancellationToken cancellationToken)
	{
		var url = $"{_departuresUrlTemplate}{stopId}/annunciator?kioskId={kioskId}";
		_logger.LogDebug("Fetching {url}", url);
		var result = await _httpClient.GetAsync(url, cancellationToken);
		return result;
	}

	private static async Task<IReadOnlyCollection<Departure>> ConvertResults(HttpResponseMessage result, CancellationToken cancellationToken)
	{
		var stream = await result.Content.ReadAsStreamAsync(cancellationToken);
		var departures = await JsonSerializer.DeserializeAsync<JsonDeparture[]>(stream, _jsonOptions, cancellationToken);

		if (departures is null)
		{
			return [];
		}

		var converted = departures
			.Select(ConvertJsonDepartureToDeparture)
			.ToImmutableArray();

		return converted;
	}

	private static Departure ConvertJsonDepartureToDeparture(JsonDeparture item)
	{
		var baseName = $"{item.Number} {item.Direction} {item.Name}";
		var name = item.Modifier.Length > 0 ? $"{baseName} to {item.Modifier}" : baseName;
		return new Departure(name, item.DepartsIn, item.IsRealtime);
	}

}
