
using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities;

public class TemperatureDaily : IEntity
{
	public string KioskId { get; set; } = string.Empty;

	public DateTimeOffset Date { get; set; }

	public byte MinTempFahrenheit { get; set; }

	public byte MaxTempFahrenheit { get; set; }

	public byte AvgTempFahrenheit { get; set; }

	public byte MinRelHumidity { get; set; }

	public byte MaxRelHumidity { get; set; }

	public byte AvgRelHumidity { get; set; }

	public TemperatureDaily(string kioskId, DateTimeOffset date, byte minTempFahrenheit, byte maxTempFahrenheit, byte avgTempFahrenheit, byte minRelHumidity, byte maxRelHumidity, byte avgRelHumidity)
	{
		KioskId = kioskId;
		Date = date;
		MinTempFahrenheit = minTempFahrenheit;
		MaxTempFahrenheit = maxTempFahrenheit;
		AvgTempFahrenheit = avgTempFahrenheit;
		MinRelHumidity = minRelHumidity;
		MaxRelHumidity = maxRelHumidity;
		AvgRelHumidity = avgRelHumidity;
	}
}
