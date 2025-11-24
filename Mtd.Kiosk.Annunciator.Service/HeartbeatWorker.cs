using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core.Config;
using System.Text;
using System.Text.Json;

namespace Mtd.Kiosk.Annunciator.Service;

internal class HeartbeatWorker : BackgroundService
{
	private readonly KioskConfig _config;
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly ILogger<HeartbeatWorker> _logger;

	public HeartbeatWorker(IOptions<KioskConfig> config,
IHttpClientFactory httpClientFactory, ILogger<HeartbeatWorker> logger)
	{
		ArgumentNullException.ThrowIfNull(config.Value, nameof(config));
		ArgumentNullException.ThrowIfNull(httpClientFactory, nameof(httpClientFactory));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_config = config.Value;
		_httpClientFactory = httpClientFactory;
		_logger = logger;
	}

	protected async override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var client = _httpClientFactory.CreateClient();
				client.DefaultRequestHeaders.Add("X-ApiKey", _config.ApiKey);

				var content = new StringContent(
								JsonSerializer.Serialize(new { timestamp = DateTime.UtcNow }),
								Encoding.UTF8,
								"application/json");

				var response = await client.PostAsync(_config.HeartbeatEndpoint + $"?kioskId={_config.KioskId}", content, stoppingToken);

				if (response.IsSuccessStatusCode)
				{
					_logger.LogDebug("Hearbeat succeeded at {time}", DateTime.UtcNow);
				}
				else
				{
					_logger.LogDebug("POST failed with status {status}", response.StatusCode);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred during HTTP POST");
			}

			await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
		}
	}
}
