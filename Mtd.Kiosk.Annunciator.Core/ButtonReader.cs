using Microsoft.Extensions.Logging;

namespace Mtd.Kiosk.Annunciator.Core;

public abstract class ButtonReader : IButtonReader
{
	public abstract string Name { get; }


	private bool _buttonPressPending;
	protected bool ButtonPressPending
	{
		private get => _buttonPressPending;
		set => _buttonPressPending = value;
	}

	protected readonly ILogger<ButtonReader> _logger;

	protected ButtonReader(ILogger<ButtonReader> logger)
	{
		ArgumentNullException.ThrowIfNull(logger, nameof(logger));
		_logger = logger;
	}

	public abstract void Start();
	public abstract void Stop();
	public virtual bool ReadButtonPressed(bool peek = false)
	{
		var val = ButtonPressPending;
		if (!peek)
		{
			_logger.LogTrace("Button press read. Resetting buttonPressPending to false");
			ButtonPressPending = false;
		}
		return val;
	}
	public abstract void Dispose();
}
