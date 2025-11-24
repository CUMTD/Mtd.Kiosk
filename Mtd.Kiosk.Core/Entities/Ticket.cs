#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Mtd.Core.Entities;

namespace Mtd.Kiosk.Core.Entities;

public class Ticket() : GuidEntity(), IEntity
{
	public string KioskId { get; set; } = string.Empty;
	public TicketStatusType Status { get; set; } = TicketStatusType.Open;
	public DateTime OpenDate { get; set; } = DateTime.Now;
	public DateTime? CloseDate { get; set; }
	public string OpenedBy { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string Title { get; set; } = string.Empty;
	public virtual Kiosk Kiosk { get; set; }
	public virtual ICollection<TicketNote> Notes { get; set; } = [];

	public Ticket(string kioskId, string openedBy, string title, string? description) : this()
	{
		KioskId = kioskId;
		OpenedBy = openedBy;
		Title = title;
		Description = description;
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
