using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Readers.Raspi.Config;
using System.Device.Gpio;

namespace Mtd.Kiosk.Annunciator.Readers.Raspi;

public sealed class PiReader : ButtonReader, IButtonReader, IDisposable
{
	public const string KEY = "RASPI";
	private readonly int _gpioPin;
	private readonly PinMode _pinMode;
	private bool disposedValue;
	private GpioController? _controller;
	private DateTime _lastButtonPress = DateTime.MinValue;
	private readonly TimeSpan _debounceDelay;
	public override string Name => "Raspberry Pi Reader"; public PiReader(IOptions<PiReaderConfig> options, ILogger<PiReader> logger) : base(logger)
	{
		ArgumentNullException.ThrowIfNull(options?.Value, nameof(options.Value));

		_gpioPin = options.Value.Pin;
		_pinMode = PinMode.InputPullUp; // Always use InputPullUp for reliable button detection
		_debounceDelay = TimeSpan.FromMilliseconds(Math.Max(50, options.Value.DebounceDelayMs)); // Minimum 50ms
		_controller = new GpioController();
	}
	public override void Start()
	{
		if (disposedValue)
		{
			throw new Exception("Object is disposed. Cannot start.");
		}

		if (_controller is null)
		{
			throw new Exception("Controller is null. Cannot start.");
		}

		_logger.LogDebug("Opening pin {pin}", _gpioPin);
		_controller.OpenPin(_gpioPin, _pinMode);

		Thread.Sleep(500); // prevent a button press on pin open
		_controller.RegisterCallbackForPinValueChangedEvent(_gpioPin, PinEventTypes.Falling, Callback);


		_logger.LogInformation("{readerName} started", Name);
	}
	public override void Stop()
	{

		_logger.LogDebug("Closing pin {pin}", _gpioPin);
		_controller?.UnregisterCallbackForPinValueChangedEvent(_gpioPin, Callback);
		_controller?.ClosePin(_gpioPin);


		_logger.LogInformation("{readerName} stopped", Name);
	}
	private void Callback(object sender, PinValueChangedEventArgs args)
	{
		var now = DateTime.UtcNow;

		// Debounce: ignore button presses that occur too quickly after the previous one
		if (now - _lastButtonPress < _debounceDelay)
		{
			_logger.LogTrace("Button press ignored due to debouncing on pin {pin}", args.PinNumber);
			return;
		}

		_lastButtonPress = now;
		_logger.LogDebug("Button {changeType} on pin {pin}", args.ChangeType, args.PinNumber);
		ButtonPressPending = true;
	}

	#region IDisposable
	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				_controller?.Dispose();
			}

			_controller = null;
			disposedValue = true;
		}
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	#endregion IDisposable

}
