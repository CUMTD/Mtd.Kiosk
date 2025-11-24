using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Annunciator.Readers.Raspi.Config;

public class PiReaderConfig
{
	public const string ConfigSectionName = "PiReader";
	[Required]
	public required int Pin { get; set; }
	public bool Enabled { get; set; }

	[Required]
	public bool ExternalResistor { get; set; }

	public int DebounceDelayMs { get; set; } = 250;

}
