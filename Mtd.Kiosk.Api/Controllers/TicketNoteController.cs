using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Controllers;

// TODO: Is this needed? What is there difference between ticket-notes and /tickets/{ticketId}/comment?

/// <summary>
/// Controller for ticket notes.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("ticket-notes")]
public class TicketNoteController : ControllerBase
{
	private readonly ITicketNoteRepository _ticketNoteRepository;
	private readonly ILogger<TicketNoteController> _logger;

	/// <summary>
	/// Constructor for TicketNoteController.
	/// </summary>
	/// <param name="ticketNoteRepository"></param>
	/// <param name="logger"></param>
	public TicketNoteController(ITicketNoteRepository ticketNoteRepository, ILogger<TicketNoteController> logger)
	{
		ArgumentNullException.ThrowIfNull(ticketNoteRepository);
		ArgumentNullException.ThrowIfNull(logger);

		_ticketNoteRepository = ticketNoteRepository;
		_logger = logger;
	}

	/// <summary>
	/// Delete a ticket note/comment.
	/// </summary>
	/// <param name="noteId">The note id to delete</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpDelete("{noteId}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult> DeleteTicketNoteAsync([GuidId(true)] string noteId, CancellationToken cancellationToken)
	{
		TicketNote ticketNote;
		try
		{

			ticketNote = await _ticketNoteRepository.GetByIdentityAsync(noteId, cancellationToken);
			ticketNote.Deleted = true;
			await _ticketNoteRepository.CommitChangesAsync(cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket note not found: {noteId}", noteId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting ticket note");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		await _ticketNoteRepository.DeleteAsync(ticketNote, cancellationToken);
		return NoContent();
	}

	/// <summary>
	/// Update a ticket note/comment with the provided markdown body.
	/// </summary>
	/// <param name="noteId">The note/comment to update.</param>
	/// <param name="updatedTicketModel">An UpdateTicketModel with new data</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPatch("{noteId}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult> UpdateTicketNoteAsync([GuidId(true)] string noteId, [FromBody, Required] UpdateTicketModel updatedTicketModel, CancellationToken cancellationToken)
	{
		TicketNote? ticketNote;
		try
		{
			ticketNote = await _ticketNoteRepository.GetByIdentityOrDefaultAsync(noteId, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to fetch note with id {noteId}", noteId);
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		if (ticketNote == null)
		{
			return NotFound();
		}

		ticketNote.MarkdownBody = updatedTicketModel.MarkdownBody;

		try
		{
			await _ticketNoteRepository.CommitChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating ticket note");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		return Ok();
	}
}
