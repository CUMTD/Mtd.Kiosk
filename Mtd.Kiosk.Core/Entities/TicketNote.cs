#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Mtd.Core.Entities;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Core.Entities;

public class TicketNote : GuidEntity, IEntity
{
	public string? MarkdownBody { get; set; }
	public string TicketId { get; set; }
	public DateTime CreatedDate { get; set; }
	public string CreatedBy { get; set; }
	public bool Deleted { get; set; }

	[JsonIgnore]
	public virtual Ticket Ticket { get; set; }

	protected TicketNote() : base()
	{
		TicketId = string.Empty;
		CreatedDate = DateTime.Now;
		CreatedBy = string.Empty;
		Deleted = false;
	}

	public TicketNote(string ticketId, string createdBy, string? markdownBody) : this()
	{
		TicketId = ticketId;
		CreatedBy = createdBy;
		MarkdownBody = markdownBody;
	}
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
