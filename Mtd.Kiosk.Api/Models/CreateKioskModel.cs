using Mtd.Kiosk.Api.Attributes;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// View model to create a new kiosk in the database.
/// </summary>
public class CreateKioskModel
{
	/// <summary>
	/// The Kiosk ID to create in the database.
	/// </summary>
	[GuidId(true)]
	public required string KioskId { get; set; }
}
