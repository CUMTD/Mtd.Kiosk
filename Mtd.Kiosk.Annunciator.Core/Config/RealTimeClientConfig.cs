using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Annunciator.Core.Config;

public class RealTimeClientConfig
{
	public const string ConfigSectionName = "RealTimeClient";
	[Required]
	public required string RealTimeAddressTemplate { get; set; }
}
