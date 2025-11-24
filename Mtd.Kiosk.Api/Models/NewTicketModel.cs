using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for creating a new ticket.
/// </summary>
public class NewTicketModel
{
	/// <summary>
	/// The id of the kiosk that the ticket was opened on.
	/// </summary>
	[GuidId(true)]
	public required string KioskId { get; set; }
	/// <summary>
	/// The name of the user who opened the ticket.
	/// </summary>
	[Required, RegularExpression("^[A-Za-z]+(?:[-'][A-Za-z]+)?(?:\\s[A-Za-z]+(?:[-'][A-Za-z]+)?)*$")]
	public required string OpenedBy { get; set; }
	/// <summary>
	/// The description of the ticket.
	/// </summary>
	public string? Description { get; set; }
	/// <summary>
	/// The title of the ticket.
	/// </summary>
	///
	[Required]
	public required string Title { get; set; }
	/// <summary>
	/// Converts the model to a Ticket.
	/// </summary>
	/// <returns>A ticket object.</returns>
	internal Ticket ToTicket() => new(KioskId, OpenedBy, Title, Description);
}
