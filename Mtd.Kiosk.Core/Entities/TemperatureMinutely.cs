
using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities;

public class TemperatureMinutely : IEntity
{
	public string KioskId { get; set; } = string.Empty;

	public DateTimeOffset Timestamp { get; set; }

	public TemperatureSensorType SensorType { get; set; }

	public byte TempFahrenheit { get; set; }

	public byte RelHumidity { get; set; }

	public TemperatureMinutely(string kioskId, byte tempFahrenheit, byte relHumidity, TemperatureSensorType sensorType) : this()
	{
		KioskId = kioskId;
		TempFahrenheit = tempFahrenheit;
		RelHumidity = relHumidity;
		Timestamp = DateTimeOffset.Now.ToLocalTime();
		SensorType = sensorType;
	}

	public TemperatureMinutely()
	{
		Timestamp = DateTimeOffset.Now.ToLocalTime();
	}
}
