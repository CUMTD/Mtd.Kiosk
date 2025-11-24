using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.IpDisplaysApi;

namespace Mtd.Kiosk.Api.Controllers;

/// <summary>
/// API methods related to previewing the content of LED signs.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("led-preview")]
public class LedPreviewController : ControllerBase
{
	private readonly HttpClient _httpClient;
	private readonly IOptions<IpDisplaysApiClientConfig> _ledConfig;
	private readonly IpDisplaysApiClientFactory _ipDisplaysAPIClientFactory;
	private readonly ILogger<LedPreviewController> _logger;
	private readonly ILogger<IPDisplaysApiClient> _ipDisplaysApiClientLogger;

	/// <summary>
	///
	/// </summary>
	/// <param name="httpClient"></param>
	/// <param name="ledConfig"></param>
	/// <param name="displayLogger"></param>
	/// <param name="logger"></param>
	public LedPreviewController(HttpClient httpClient, IOptions<IpDisplaysApiClientConfig> ledConfig, ILogger<IPDisplaysApiClient> displayLogger, ILogger<LedPreviewController> logger)
	{
		ArgumentNullException.ThrowIfNull(httpClient);
		ArgumentNullException.ThrowIfNull(ledConfig);
		ArgumentNullException.ThrowIfNull(displayLogger);
		ArgumentNullException.ThrowIfNull(logger);

		_httpClient = httpClient;
		_ledConfig = ledConfig;
		_ipDisplaysApiClientLogger = displayLogger;
		_logger = logger;

		_ipDisplaysAPIClientFactory = new IpDisplaysApiClientFactory(ledConfig, displayLogger);
	}

	/// <summary>
	/// Gets a PNG image of the currently displaying content for the LED at the provided address.
	/// </summary>
	/// <param name="ledIp">The IP address of the LED to preview.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A PNG image representing the current contents of the LED sign.</returns>
	[HttpGet("")]
	[ProducesResponseType<FileResult>(StatusCodes.Status200OK)]
	[ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	[Produces("image/png")]
	public async Task<ActionResult> GetLedPreviewImage([FromQuery, IpAddress(true)] string ledIp, CancellationToken cancellationToken)
	{
		Uri? previewImageUri;
		try
		{
			var client = _ipDisplaysAPIClientFactory.CreateClient(ledIp);
			var ledSign = new LedSign(ledIp, client, _logger);
			previewImageUri = await client.GetLedPreviewImageUri();
		}
		catch (TimeoutException)
		{
			_logger.LogWarning("Timeout getting Led preview image for {ip}", ledIp);
			return NotFound("Timeout getting LED preview.");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error getting Led preview image at {ip}", ledIp);
			return StatusCode(500);
		}

		if (previewImageUri == null)
		{
			_logger.LogWarning("Got a null response from SOAP client for LED preview of {ip}", ledIp);
			return StatusCode(500);
		}

		try
		{
			//download the image and return it
			var image = await _httpClient.GetByteArrayAsync(previewImageUri, cancellationToken);
			return File(image, "image/png");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error downloading LED preview image at URL: {url}", previewImageUri);
			return StatusCode(500);
		}
	}
}
