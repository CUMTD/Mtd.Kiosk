using Mtd.Kiosk.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for creating a new ticket note.
/// </summary>
public class NewTicketNoteModel
{
	/// <summary>
	/// The body of the ticket note, in markdown format.
	/// </summary>
	public string? MarkdownBody { get; set; }
	/// <summary>
	/// The name of the user who created the ticket note.
	/// </summary>
	[Required]
	public required string CreatedBy { get; set; }
	/// <summary>
	/// Converts the model to a TicketNote.
	/// </summary>
	/// <param name="ticketId"></param>
	/// <returns>A TicketNote object</returns>
	public TicketNote ToTicketNote(string ticketId) => new(ticketId, CreatedBy, MarkdownBody);
}
