using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Core.Config;
using Mtd.Kiosk.Annunciator.Core.Models;
using System.Collections.Immutable;

namespace Mtd.Kiosk.Annunciator.Service;

internal class AnnunciatorService : BackgroundService, IDisposable
{
	private bool currentlyAnnouncing = false;

	private readonly KioskConfig _config;
	private readonly IReadOnlyCollection<IButtonReader> _readers;
	private readonly IKioskRealTimeClient _kioskRealTimeClient;
	private readonly IAnnunciator _annunciator;
	private readonly ILogger<AnnunciatorService> _logger;
	public AnnunciatorService(
		IOptions<KioskConfig> config,
		IEnumerable<IButtonReader> readers,
		IKioskRealTimeClient kioskRealTimeClient,
		IAnnunciator annunciator,
		ILogger<AnnunciatorService> logger
	)
	{
		ArgumentNullException.ThrowIfNull(config.Value, nameof(config));
		ArgumentException.ThrowIfNullOrWhiteSpace(config.Value.Name, nameof(config.Value.Name));
		ArgumentException.ThrowIfNullOrWhiteSpace(config.Value.KioskId, nameof(config.Value.KioskId));
		ArgumentException.ThrowIfNullOrWhiteSpace(config.Value.StopId, nameof(config.Value.StopId));
		ArgumentNullException.ThrowIfNull(readers, nameof(readers));
		ArgumentNullException.ThrowIfNull(kioskRealTimeClient, nameof(kioskRealTimeClient));
		ArgumentNullException.ThrowIfNull(annunciator, nameof(annunciator));
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_config = config.Value;
		_readers = readers.ToImmutableArray();
		_kioskRealTimeClient = kioskRealTimeClient;
		_annunciator = annunciator;
		_logger = logger;

		ArgumentOutOfRangeException.ThrowIfZero(_readers.Count, nameof(readers));
	}

	#region BackgroundService

	protected async override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var pressedReaders = _readers.Where(r => r.ReadButtonPressed());
			foreach (var reader in pressedReaders)
			{
				ReaderButtonPressed(reader, stoppingToken);
			}
			await Task.Delay(100, stoppingToken);
		}
		await StopAsync(CancellationToken.None);
	}

	public async override Task StartAsync(CancellationToken cancellationToken)
	{
		foreach (var reader in _readers)
		{
			// Will start the BG worker.
			reader.Start();
		}

		_logger.LogInformation("{serviceName} started", nameof(AnnunciatorService));
		await base.StartAsync(cancellationToken);
	}


	public async override Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (var reader in _readers)
		{
			// Stop the BG worker
			reader.Stop();
		}

		_logger.LogInformation("{serviceName} stopped", nameof(AnnunciatorService));

		await base.StopAsync(cancellationToken);
	}

	#endregion BackgroundService

	private async void ReaderButtonPressed(IButtonReader reader, CancellationToken cancellationToken)
	{
		if (currentlyAnnouncing)
		{
			_logger.LogDebug("Already announcing. Ignoring button press.");
			return;
		}

		_logger.LogInformation("{readerName} button pressed", reader.Name);

		currentlyAnnouncing = true;

		IReadOnlyCollection<Departure>? departures;
		try
		{
			departures = await _kioskRealTimeClient.GetRealtime(_config.KioskId, _config.StopId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch departures");
			currentlyAnnouncing = false;
			return;
		}

		_logger.LogDebug("Fetched {count} departures for {kioskId}", departures?.Count ?? -1, _config.KioskId);

		try
		{
			await _annunciator.ReadDepartures(_config.Name, departures, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to announce departures");
		}

		currentlyAnnouncing = false;
	}

}
