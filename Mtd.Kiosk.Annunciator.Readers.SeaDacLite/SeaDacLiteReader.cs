using Microsoft.Extensions.Logging;
using Mtd.Kiosk.Annunciator.Core;
using Sealevel;

namespace Mtd.Kiosk.Annunciator.Readers.SeaDacLite;

public sealed class SeaDacReader(ILogger<SeaDacReader> logger) : ButtonReader(logger), IDisposable
{
	private const int ModbusStart = 0;
	private const int InputQuantity = 4;
	private const int ReadDelay = 16;
	private const bool IsBlocking = false;
	private const int PollingIntervalMs = 10;

	public override string Name => "SeaDAC Lite Reader";
	public const string KEY = "SEADAC";

	private readonly SeaMAX _seaMax = new();
	private CancellationTokenSource? _cts;
	private bool _isDisposed = false;
	private bool _isRunning = false;
	private readonly byte[] _bytes = new byte[1]; // stores current button state (byte[] required by SeaMAX library)
	private bool _lastState = false;

	public override void Start()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, Name);

		if (_isRunning)
		{
			_logger.LogWarning("{Name} is already running", Name);
			return;
		}

		OpenRead();

		_cts = new CancellationTokenSource();
		_isRunning = true;
		Task.Run(() => PollButtonAsync(_cts.Token));

		_logger.LogInformation("{Name} started", Name);
	}

	private void OpenRead()
	{
		if (!_seaMax.IsSeaDACInitialized)
		{
			var initResult = _seaMax.SDL_Initialize();
			if (initResult < 0)
			{
				throw new InvalidOperationException($"SDL_Initialize failed: {initResult}");
			}

			var deviceCount = _seaMax.SDL_SearchForDevices();
			if (deviceCount < 0)
			{
				throw new InvalidOperationException($"SDL_SearchForDevices failed: {deviceCount}");
			}

			if (deviceCount == 0)
			{
				throw new InvalidOperationException("No SDL devices found");
			}

			if (_seaMax.SDL_FirstDevice() < 0)
			{
				throw new InvalidOperationException("SDL_FirstDevice failed");
			}
		}

		if (!_seaMax.IsSeaMAXOpen)
		{
			var id = 0;
			if (_seaMax.SDL_GetDeviceID(ref id) < 0)
			{
				throw new InvalidOperationException("SDL_GetDeviceID failed");
			}

			if (_seaMax.SM_Open($"SeaDAC Lite {id}") < 0)
			{
				throw new InvalidOperationException("SM_Open failed");
			}
		}
	}

	private async Task PollButtonAsync(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			try
			{
				CheckButtonState();
				await Task.Delay(PollingIntervalMs, token);
			}
			catch (TaskCanceledException)
			{
				// Expected when cancelled
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error monitoring button state");
			}
		}

		CleanupAfterStop();
	}

	private void CheckButtonState()
	{
		if (_seaMax.SM_NotifyInputState(0) != 2)
		{
			_seaMax.SM_NotifyOnInputChange(ModbusStart, InputQuantity, _bytes, ReadDelay, IsBlocking ? 1 : 0);
		}

		var currentState = _bytes[0] >= 1;
		if (currentState == _lastState)
		{
			return;
		}

		_logger.LogDebug("Button state changed to {State}", currentState);
		if (currentState)
		{
			ButtonPressPending = true;
		}

		_lastState = currentState;
	}

	private void CleanupAfterStop()
	{
		_seaMax.SM_NotifyInputState(1);
		_isRunning = false;
	}

	public override void Stop()
	{
		if (_isRunning)
		{
			_cts?.Cancel();
			_logger.LogInformation("{Name} stopped", Name);
		}
	}

	public override void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}

		Stop();

		if (_seaMax.IsSeaMAXOpen)
		{
			_seaMax.SM_NotifyInputState(1);
			_seaMax.SM_Close();
		}

		if (_seaMax.IsSeaDACInitialized)
		{
			_seaMax.SDL_Cleanup();
		}

		_cts?.Dispose();
		_isDisposed = true;
	}
}
