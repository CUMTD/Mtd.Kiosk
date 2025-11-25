using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;
using System.ComponentModel.DataAnnotations;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// Controller for tickets.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("tickets")]
public class TicketController : ControllerBase
{
	private readonly ITicketRepository _ticketRepository;
	private readonly ILogger<TicketController> _logger;

	/// <summary>
	/// Constructor for Ticket Controller.
	/// </summary>
	/// <param name="ticketRepository"></param>
	/// <param name="logger"></param>
	public TicketController(ITicketRepository ticketRepository, ILogger<TicketController> logger)
	{
		ArgumentNullException.ThrowIfNull(ticketRepository);
		ArgumentNullException.ThrowIfNull(logger);

		_ticketRepository = ticketRepository;
		_logger = logger;
	}

	/// <summary>
	/// Gets a ticket by its id.
	/// </summary>
	/// <param name="ticketId">The id of the ticket to get</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A ticket.</returns>
	[HttpGet("{ticketId}")]
	[ProducesResponseType<Ticket>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> GetTicketAsync([GuidId(true)] string ticketId, CancellationToken cancellationToken)
	{
		Ticket ticket;
		try
		{
			ticket = await _ticketRepository.GetByIdentityAsync(ticketId, cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket not found: {ticketId}", ticketId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting ticket: {ticketId}", ticketId);
			return StatusCode(500);
		}

		return Ok(ticket);
	}

	/// <summary>
	/// Gets all tickets.
	/// </summary>
	/// <param name="includeClosed">Whether to include closed tickets.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>An array of Tickets.</returns>
	[HttpGet("")]
	[ProducesResponseType<IEnumerable<Ticket>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<Ticket>>> GetAllTicketsAsync([FromQuery] bool includeClosed, CancellationToken cancellationToken)
	{

		IReadOnlyCollection<Ticket> tickets;

		try
		{
			tickets = includeClosed
				? await _ticketRepository.GetAllAsync(cancellationToken)
				: await _ticketRepository.GetAllOpenTicketsAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting tickets");
			return StatusCode(500);
		}

		return Ok(tickets);

	}

	/// <summary>
	/// Creates a new ticket in the database.
	/// </summary>
	/// <param name="newTicketModel">The new ticket to be created.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>The ticket, if successful.</returns>
	[HttpPost]
	[ProducesResponseType<Ticket>(StatusCodes.Status201Created)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> CreateTicketAsync([Required] NewTicketModel newTicketModel, CancellationToken cancellationToken)
	{
		var ticket = newTicketModel.ToTicket();
		try
		{
			await _ticketRepository.AddAsync(ticket, cancellationToken);
			await _ticketRepository.CommitChangesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating ticket");
			return StatusCode(500);
		}

		return CreatedAtAction(nameof(CreateTicketAsync), new { TicketId = ticket.Id }, ticket);
	}

	/// <summary>
	/// Updates a ticket's status to a new TicketStatusType.
	/// </summary>
	/// <param name="ticketId">The ticket id to be updated</param>
	/// <param name="newStatus">The new TicketStatusType of the ticket</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPatch("{ticketId}/status")]
	[ProducesResponseType<Ticket>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> UpdateTicketAsync([GuidId(true)] string ticketId, [FromQuery, Required] TicketStatusType newStatus, CancellationToken cancellationToken)
	{

		Ticket ticket;
		try
		{
			ticket = await _ticketRepository.GetByIdentityAsync(ticketId, cancellationToken);
			ticket.Status = newStatus;
			if (newStatus == TicketStatusType.Resolved)
			{
				ticket.CloseDate = DateTime.Now;
			}

			if (newStatus == TicketStatusType.Open)
			{
				ticket.CloseDate = null;
			}

			await _ticketRepository.CommitChangesAsync(cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket not found: {ticketId}", ticketId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating ticket: {ticketId}", ticketId);
			return StatusCode(500);
		}

		return Ok(ticket);
	}

	/// <summary>
	/// Adds a comment to a ticket.
	/// </summary>
	/// <param name="ticketId">The id of the ticket to add a comment to</param>
	/// <param name="newTicketNoteModel">The new ticket note to add</param>
	/// <param name="cancellationToken"></param>
	/// <returns></returns>
	[HttpPost("{ticketId}/comment")]
	[ProducesResponseType<Ticket>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<Ticket>> AddCommentAsync([FromRoute, GuidId(true)] string ticketId, [FromBody, Required] NewTicketNoteModel newTicketNoteModel, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Adding comment to ticket: {ticketId}", ticketId);
		Ticket ticket;
		var note = newTicketNoteModel.ToTicketNote(ticketId);
		try
		{
			ticket = await _ticketRepository.GetByIdentityAsync(ticketId, cancellationToken);
			ticket.Notes.Add(note);
			await _ticketRepository.CommitChangesAsync(cancellationToken);
		}
		catch (InvalidOperationException ex)
		{
			_logger.LogWarning(ex, "Ticket not found: {ticketId}", ticketId);
			return NotFound();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error adding comment to ticket: {ticketId}", ticketId);
			return StatusCode(500);
		}

		return Ok(ticket);
	}
}
