using Mtd.Core.Entities;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Core.Entities;

public class Kiosk() : GuidEntity, IEntity
{
	public bool Deleted { get; set; } = false;
	// dont want to auto include tickets

	[JsonIgnore]
	public virtual ICollection<Ticket> Tickets { get; set; } = [];

	public Kiosk(string guid) : this()
	{
		Id = guid;
	}
}
