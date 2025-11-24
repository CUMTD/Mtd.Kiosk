using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Mtd.Kiosk.Api.Attributes;
using Mtd.Kiosk.Api.Config;
using Mtd.Kiosk.Api.Filters;
using Mtd.Kiosk.Api.Services;
using Mtd.Kiosk.Core.Repositories;
using Mtd.Kiosk.Infrastructure.EfCore;
using Mtd.Kiosk.Infrastructure.EfCore.Repository;
using Mtd.Kiosk.IpDisplaysApi;
using Mtd.Kiosk.RealTime;
using Mtd.Kiosk.RealTime.Config;
using Mtd.Stopwatch.Core.Entities.Schedule;
using Mtd.Stopwatch.Core.Repositories.Schedule;
using Mtd.Stopwatch.Core.Repositories.Transit;
using Mtd.Stopwatch.Infrastructure.EFCore;
using Mtd.Stopwatch.Infrastructure.EFCore.Repositories.Schedule;
using Mtd.Stopwatch.Infrastructure.EFCore.Repositories.Transit;
using Serilog;

namespace Mtd.Kiosk.Api.Extensions;

internal static class WebApplicationBuilderExtensions
{

	public static WebApplicationBuilder Configure(this WebApplicationBuilder builder) => builder
		.AddConfiguration()
		.ConfigureLogging()
		.ConfigureApi()
		.ConfigureDB()
		.ConfigureDI()
		.ConfigureHTTPClient();

	private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
	{
		if (builder.Environment.IsDevelopment())
		{
			_ = builder.Configuration.AddUserSecrets<Program>();
		}

		_ = builder.Configuration.AddEnvironmentVariables("Kiosk_");

		_ = builder.Services.AddOptions<ApiAuthentication>()
			.Bind(builder.Configuration.GetSection(nameof(ApiAuthentication)))
			.ValidateDataAnnotations();

		_ = builder.Services
			.AddOptions<ConnectionStrings>()
			.BindConfiguration("ConnectionStrings")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services
			.AddOptions<ApiConfiguration>()
			.BindConfiguration("ApiConfiguration")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services
			.AddOptions<ApiRealTimeClientConfig>()
			.BindConfiguration("RealTimeClientConfig")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		_ = builder.Services
			.AddOptions<IpDisplaysApiClientConfig>()
			.BindConfiguration("IPDisplaysApiClient")
			.ValidateDataAnnotations()
			.ValidateOnStart();

		return builder;
	}
	private static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
	{
		_ = builder.Host.UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));

		return builder;
	}
	private static WebApplicationBuilder ConfigureApi(this WebApplicationBuilder builder)
	{
		var corsPolicyName = builder.Configuration["Cors:PolicyName"] ?? throw new InvalidOperationException("Cors:PolicyName not defined");
		var corsAllowedOrigin = builder.Configuration["Cors:AllowedOrigins"] ?? throw new InvalidOperationException("Cors:AllowedOrigins not defined");

		_ = builder.Services.AddControllers(options => options.ModelBinderProviders.Insert(0, new GuidModelBinderProvider()));

		_ = builder.Services.AddCors(options => options.AddPolicy(
			corsPolicyName,
			policy => policy
			.WithOrigins(corsAllowedOrigin)
			.AllowAnyHeader()
			.AllowAnyMethod()
		));

		_ = builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddOpenApi(options => options.AddDocumentTransformer((document, context, cancellationToken) =>
			{
				document.Info = new Microsoft.OpenApi.OpenApiInfo
				{
					Version = "1.0",
					Title = "Kiosk API",
					Description = "MTD Kiosk API",
					Contact = new Microsoft.OpenApi.OpenApiContact
					{
						Name = "MTD",
						Email = "developer@mtd.org"
					}
				};

				// === Add API key security scheme ===
				const string schemeName = "ApiKey";
				document.Components ??= new OpenApiComponents();
				if (document.Components.SecuritySchemes is null)
				{
					document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
				}

				if (!document.Components.SecuritySchemes.ContainsKey(schemeName))
				{
					document.Components.SecuritySchemes[schemeName] = new OpenApiSecurityScheme
					{
						Type = SecuritySchemeType.ApiKey,
						In = ParameterLocation.Header,
						Name = "X-ApiKey",
						Description = "Provide your API key in the header using X-ApiKey."
					};
				}

				// === Apply requirement to all operations ===
				var requirement = new OpenApiSecurityRequirement
				{
					[new OpenApiSecuritySchemeReference(schemeName, document)] = []
				};

				if (document.Paths is not null)
				{
					foreach (var path in document.Paths.Values)
					{
						// Paths themselves shouldn’t be null, but be defensive about Operations
						if (path?.Operations is null)
						{
							continue;
						}

						foreach (var operation in path.Operations.Values)
						{
							operation.Security ??= [];
							operation.Security.Add(requirement);
						}
					}
				}

				return Task.CompletedTask;
			}));

		_ = builder.Services.AddControllers(options => options.Filters.Add<ApiKeyFilter>());

		return builder;
	}
	private static WebApplicationBuilder ConfigureDB(this WebApplicationBuilder builder)
	{
		_ = builder.Services.AddDbContextPool<KioskContext>((sp, options) =>
		{
			var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.KioskConnection;
			options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
			options.UseLazyLoadingProxies();
		});

		_ = builder.Services.AddDbContextPool<StopwatchContext>((sp, options) =>
		{
			var connectionString = sp.GetRequiredService<IOptions<ConnectionStrings>>().Value.StopwatchConnection;
			options.UseSqlServer(connectionString);
			options.UseLazyLoadingProxies();
		});

		return builder;
	}
	private static WebApplicationBuilder ConfigureDI(this WebApplicationBuilder builder)
	{
		_ = builder.Services.AddScoped<IPublicRouteRepository<IReadOnlyCollection<PublicRoute>>, PublicRouteRepository>();
		_ = builder.Services.AddScoped<IPublicRouteGroupRepository<IReadOnlyCollection<PublicRouteGroup>>, PublicRouteGroupRepository>();
		_ = builder.Services.AddScoped<IRouteRepository<IReadOnlyCollection<Stopwatch.Core.Entities.Transit.Route>>, RouteRepository>();

		_ = builder.Services.AddScoped<IKioskRepository, KioskRepository>();
		_ = builder.Services.AddScoped<IHealthRepository, HealthRepository>();
		_ = builder.Services.AddScoped<ITicketRepository, TicketRepository>();
		_ = builder.Services.AddScoped<ITicketNoteRepository, TicketNoteRepository>();
		_ = builder.Services.AddScoped<ITemperatureMinutelyRepository, TemperatureMinutelyRepository>();
		_ = builder.Services.AddScoped<ITemperatureDailyRepository, TemperatureDailyRepository>();

		_ = builder.Services.AddScoped<ScopedTemperatureAggregatorWorker>();
		_ = builder.Services.AddHostedService<TemperatureAggregatorWorker>();

		_ = builder.Services.AddScoped<IpDisplaysApiClientFactory>();

		_ = builder.Services.AddMemoryCache();

		return builder;
	}
	private static WebApplicationBuilder ConfigureHTTPClient(this WebApplicationBuilder builder)
	{
		builder
			.Services
		.AddHttpClient<ApiRealTimeClient>()
		.AddStandardResilienceHandler(options =>
		{
			// Timeout
			options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
			options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(10);

			// Retry
			options.Retry.MaxRetryAttempts = 3;
			options.Retry.Delay = TimeSpan.FromMilliseconds(500);
			options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
			options.Retry.UseJitter = true;

			// Circuit breaker
			options.CircuitBreaker.FailureRatio = 0.75;                             // 75% failures...
			options.CircuitBreaker.MinimumThroughput = 4;                           // over at least 4 calls
			options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);     // window: 60s
			options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(120);       // open for 120s
		});

		return builder;
	}
}
