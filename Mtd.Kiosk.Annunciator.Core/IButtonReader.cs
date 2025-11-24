namespace Mtd.Kiosk.Annunciator.Core;

public interface IButtonReader : IDisposable
{
	string Name { get; }
	void Start();
	void Stop();
	bool ReadButtonPressed(bool peek = false);
}
