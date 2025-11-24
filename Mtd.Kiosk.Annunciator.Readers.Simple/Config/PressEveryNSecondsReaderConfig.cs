using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Annunciator.Readers.Simple.Config;

public class PressEveryNSecondsReaderConfig
{
	public const string ConfigSectionName = "PressEveryNSecondsReader";

	[Required, Range(1, int.MaxValue)]
	public int Seconds { get; set; }
	public bool Enabled { get; set; }
}
