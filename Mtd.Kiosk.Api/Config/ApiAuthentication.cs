using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Config;

/// <summary>
/// Configuration for the API related to authentication.
/// </summary>
internal class ApiAuthentication
{
	/// <summary>
	/// The API keys that are allowed to access the API.
	/// </summary>
	[Required]
	public required string[] ApiKeys { get; set; }
}
