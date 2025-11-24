using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// Model for updating a ticket.
/// </summary>
public class UpdateTicketModel
{
	/// <summary>
	/// The body of the ticket, in markdown format.
	/// </summary>
	[Required]
	public required string MarkdownBody { get; set; }
}
