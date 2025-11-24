namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// The response model for the dark mode endpoint.
/// </summary>
public class DarkModeResponseModel
{
	/// <summary>
	/// Whether or not dark mode should be enabled.
	/// </summary>
	public bool UseDarkMode { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="DarkModeResponseModel"/> class.
	/// </summary>
	/// <param name="useDarkMode"></param>
	public DarkModeResponseModel(bool useDarkMode)
	{
		UseDarkMode = useDarkMode;
	}
}
