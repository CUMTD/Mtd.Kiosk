using Mtd.Kiosk.Core.Entities;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// For returning temp data to the frontend
/// </summary>
/// <param name="temperature"></param>
public class TemperatureMinutelyDataPoint(TemperatureMinutely temperature)
{
	/// <summary>
	/// The temperature in Fahrenheit.
	/// </summary>
	[JsonPropertyName("tempFahrenheit")]
	public byte TemperatureFahrenheit { get; set; } = temperature.TempFahrenheit;

	/// <summary>
	/// The relative humidity in percentage.
	/// </summary>
	[JsonPropertyName("relHumidity")]
	public byte RelativeHumidity { get; set; } = temperature.RelHumidity;

	/// <summary>
	/// The timestamp of the temperature reading in milliseconds since Unix epoch.
	/// </summary>
	[JsonPropertyName("timestamp")]
	public long Timestamp { get; set; } = temperature.Timestamp.ToUnixTimeMilliseconds();

	/// <summary>
	/// The type of sensor that recorded the conditions.
	/// </summary>
	[JsonPropertyName("sensorType")]
	public TemperatureSensorType SensorType { get; set; } = temperature.SensorType;

}
