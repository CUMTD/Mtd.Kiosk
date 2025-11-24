using Microsoft.AspNetCore.Mvc;
using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.Api.Models;
using Mtd.Kiosk.RealTime;
using Mtd.Kiosk.RealTime.Entities.Api;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// Controller for messaging.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("general-messaging")]
public class MessagingController : ControllerBase
{
	private readonly ApiRealTimeClient _realTimeClient;
	private readonly ILogger<DeparturesController> _logger;
	/// <summary>
	/// Constructor for MessagingController.
	/// </summary>
	/// <param name="realTimeClient"></param>
	/// <param name="logger"></param>
	public MessagingController(ApiRealTimeClient realTimeClient, ILogger<DeparturesController> logger)
	{
		ArgumentNullException.ThrowIfNull(realTimeClient);
		ArgumentNullException.ThrowIfNull(logger);

		_realTimeClient = realTimeClient;
		_logger = logger;
	}

	/// <summary>
	/// Get general messages from the SIRI realtime endpoint.
	/// </summary>
	/// <param name="cancellationToken"></param>
	/// <returns>Currently active general messages</returns>
	[HttpGet]
	[ProducesResponseType<IEnumerable<SimpleGeneralMessage>>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<IEnumerable<SimpleGeneralMessage>>> GetGeneralMessages(CancellationToken cancellationToken)
	{
		GeneralMessage[]? result;
		try
		{
			result = await _realTimeClient.GetGeneralMessagesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting general messages from real-time API.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		if (result == null)
		{
			_logger.LogWarning("No general messages returned from realtime api.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		var response = new MessagingResponseModel(result);
		return Ok(response.GeneralMessages);
	}
	/// <summary>
	/// Get general messages for an LCD display.
	/// </summary>
	/// <param name="stopId">The stop id to get general messages for.</param>
	/// <param name="cancellationToken"></param>
	/// <returns>A GeneralMessage object, 204 if no messages.</returns>
	[HttpGet("lcd")]
	[ProducesResponseType<GeneralMessage>(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<GeneralMessage>> GetLcdGeneralMessagesAsync([FromQuery, StopId(true)] string stopId, CancellationToken cancellationToken)
	{
		GeneralMessage[]? currentGeneralMessages = null;
		try
		{
			currentGeneralMessages = await _realTimeClient.GetGeneralMessagesAsync(cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting general messages from real-time API.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		if (currentGeneralMessages == null)
		{
			_logger.LogWarning("No general messages returned from realtime api.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		if (currentGeneralMessages.Length > 0)
		{
			var generalMessage = currentGeneralMessages
				.OrderByDescending(gm => gm.BlockRealtime)
				.FirstOrDefault(gm => gm.StopIds != null && gm.StopIds.Contains(stopId));

			if (generalMessage != null)
			{
				return Ok(generalMessage);
			}
		}

		return NoContent();
	}
}
