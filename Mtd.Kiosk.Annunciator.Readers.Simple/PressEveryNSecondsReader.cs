using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Readers.Simple.Config;
using System.ComponentModel;

namespace Mtd.Kiosk.Annunciator.Readers.Simple;

public sealed class PressEveryNSecondsReader : ButtonReader, IButtonReader
{
	public const string KEY = "EVERY_N";
	public override string Name => "Press Every N Seconds Reader";

	private readonly TimeSpan _interval;

	private BackgroundWorker? _worker;
	private bool _isBackgroundWorkerCurrentlyRunning;
	private bool _isDisposed;

	public PressEveryNSecondsReader(IOptions<PressEveryNSecondsReaderConfig> options, ILogger<PressEveryNSecondsReader> logger) : base(logger)
	{
		ArgumentNullException.ThrowIfNull(options?.Value, nameof(options));
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(options.Value.Seconds, 1, nameof(options.Value.Seconds));

		_interval = TimeSpan.FromSeconds(options.Value.Seconds);
	}

	public override void Start()
	{
		if (_isBackgroundWorkerCurrentlyRunning)
		{
			_logger.LogWarning("{workerName} is already running", Name);
		}
		else if (_isDisposed)
		{
			_logger.LogError("{workerName} has already been disposed", Name);
		}
		else
		{
			// create a new BackgroundWorker and start it
			_logger.LogInformation("Starting worker");
			_worker = CreateWorker();
			_isBackgroundWorkerCurrentlyRunning = true;
			_worker.RunWorkerAsync();
		}
	}

	public override void Stop()
	{
		_worker?.CancelAsync();
		Dispose();
		_logger.LogInformation("Stopped worker {workerName}", Name);
	}

	private async void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
	{
		if (sender is not BackgroundWorker bgWorker)
		{
			// this should never happen
			_logger.LogError("BackgroundWorker is null");
			return;
		}

		// Run the DetectButtonPress method in a loop until the BackgroundWorker is canceled
		while (!bgWorker.CancellationPending)
		{
			await Task.Delay((int)_interval.TotalMilliseconds);
			ButtonPressPending = true;
		}

		// The BackgroundWorker has been canceled
		_isBackgroundWorkerCurrentlyRunning = false;
		Dispose();
	}

	private BackgroundWorker CreateWorker()
	{
		// create a new worker and register do work event
		var worker = new BackgroundWorker
		{
			WorkerReportsProgress = true,
			WorkerSupportsCancellation = true
		};

		worker.DoWork += BackgroundWorker_DoWork;

		return worker;
	}

	#region IDisposable

	private void Dispose(bool disposing)
	{
		if (!_isDisposed)
		{
			if (disposing)
			{
				_worker?.Dispose();
				_worker = null;
			}

			_isDisposed = true;
		}
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion IDisposable

}
