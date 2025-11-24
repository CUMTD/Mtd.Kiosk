using Mtd.Kiosk.Core.Entities;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// For returning daily temp data to the frontend
/// </summary>
/// <param name="temperature">The temperature data entity containing daily temperature and humidity information.</param>
public class TemperatureDailyDataPoint(TemperatureDaily temperature)
{
	/// <summary>
	/// The date of the data in Unix time milliseconds.
	/// </summary>
	[JsonPropertyName("date")]
	public long Date { get; set; } = temperature.Date.ToUnixTimeMilliseconds();

	/// <summary>
	/// Minimum temperature of the day, in Fahrenheit.
	/// </summary>
	[JsonPropertyName("minTempFahrenheit")]
	public byte MinTempFahrenheit { get; set; } = temperature.MinTempFahrenheit;

	/// <summary>
	/// Maximum temperature of the day, in Fahrenheit.
	/// </summary>
	[JsonPropertyName("maxTempFahrenheit")]
	public byte MaxTempFahrenheit { get; set; } = temperature.MaxTempFahrenheit;

	/// <summary>
	/// Average temperature of the day, in Fahrenheit.
	/// </summary>
	[JsonPropertyName("avgTempFahrenheit")]
	public byte AvgTempFahrenheit { get; set; } = temperature.AvgTempFahrenheit;

	/// <summary>
	/// Minimum relative humidity of the day, in percentage.
	/// </summary>
	[JsonPropertyName("minRelHumidity")]
	public byte MinRelHumidity { get; set; } = temperature.MinRelHumidity;

	/// <summary>
	/// Maximum relative humidity of the day, in percentage.
	/// </summary>
	[JsonPropertyName("maxRelHumidity")]
	public byte MaxRelHumidity { get; set; } = temperature.MaxRelHumidity;

	/// <summary>
	/// Average relative humidity of the day, in percentage.
	/// </summary>
	[JsonPropertyName("avgRelHumidity")]
	public byte AvgRelHumidity { get; set; } = temperature.AvgRelHumidity;
}
