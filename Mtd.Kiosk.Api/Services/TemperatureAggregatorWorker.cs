namespace Mtd.Kiosk.Api.Services;

/// <summary>
/// Background service that runs every 24 hours. Checks if there is a full previous day of data, and if there is, calculates some aggregated data points and deletes the data.
/// </summary>
public class TemperatureAggregatorWorker : BackgroundService
{
	private readonly ILogger<TemperatureAggregatorWorker> _logger;
	private readonly IServiceProvider _services;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="logger"></param>
	/// <param name="services"></param>
	public TemperatureAggregatorWorker(ILogger<TemperatureAggregatorWorker> logger, IServiceProvider services)
	{
		_logger = logger;
		_services = services;

	}

	/// <inheritdoc/>
	protected override async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		using var scope = _services.CreateScope();
		var scopedWorker = scope.ServiceProvider.GetRequiredService<ScopedTemperatureAggregatorWorker>();

		using PeriodicTimer timer = new(TimeSpan.FromDays(1));

		try
		{
			await scopedWorker.DoWorkAsync(cancellationToken);
			while (await timer.WaitForNextTickAsync(cancellationToken))
			{
				_logger.LogInformation("TemperatureMinutely Aggregator Background Service running.");

				try
				{
					// start stopwatch
					var watch = System.Diagnostics.Stopwatch.StartNew();
					await scopedWorker.DoWorkAsync(cancellationToken);
					watch.Stop();

					_logger.LogInformation("Finished data aggregation in {time}", watch.Elapsed.ToString());
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Failed to execute scoped worker.");
				}
			}
		}
		catch (OperationCanceledException)
		{
			_logger.LogInformation("TemperatureMinutely Aggregator Background Service stopping.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex.Message);
		}
	}
}
