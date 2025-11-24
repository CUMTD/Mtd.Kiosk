using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Api.Config;

namespace Mtd.Kiosk.Api.Filters;

internal class ApiKeyFilter : IAuthorizationFilter
{
	private readonly ILogger<ApiKeyFilter> _logger;
	private readonly string[] _keys;

	public ApiKeyFilter(IOptions<ApiAuthentication> authenticationOptions, ILogger<ApiKeyFilter> logger)
	{
		ArgumentNullException.ThrowIfNull(authenticationOptions?.Value?.ApiKeys, nameof(authenticationOptions));
		if (authenticationOptions.Value.ApiKeys.Length == 0)
		{
			throw new ArgumentException("No API keys provided.", nameof(authenticationOptions));
		}

		ArgumentNullException.ThrowIfNull(logger, nameof(logger));

		_logger = logger;
		_keys = authenticationOptions.Value.ApiKeys;
	}

	public void OnAuthorization(AuthorizationFilterContext context)
	{
		_logger.LogTrace("Executing {filterName}", nameof(ApiKeyFilter));

		var keyFromRequest = string.Empty;
		if (context.HttpContext.Request.Headers.TryGetValue("X-ApiKey", out var headerKey))
		{
			_logger.LogDebug("Got key from 'X-ApiKey' header.");
			keyFromRequest = headerKey;
		}
		else if (context.HttpContext.Request.Query.TryGetValue("key", out var queryKey))
		{
			_logger.LogDebug("Got key from 'key' query string param");
			keyFromRequest = queryKey;
		}
		else
		{
			_logger.LogInformation("No API key provided.");
		}

		if (_keys.Any(k => string.Equals(k, keyFromRequest, StringComparison.Ordinal)))
		{
			_logger.LogTrace("Good API key.");
			return;
		}
		else
		{
			_logger.LogWarning("API Key provided but no matches. {key}", keyFromRequest);
		}

		context.Result = new UnauthorizedResult();
	}
}
