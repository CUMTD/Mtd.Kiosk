namespace Mtd.Kiosk.Api.Config;

/// <summary>
/// Database connection strings for the applicaiton.
/// </summary>
internal class ConnectionStrings
{

	// TODO: @Ryan, can I just get away with one connection string?

	/// <summary>
	/// Connection string for the Kiosk database.
	/// </summary>
	public required string KioskConnection { get; set; }

	/// <summary>
	/// Connection string for the Stopwatch database.
	/// </summary>
	public required string StopwatchConnection { get; set; }
}
