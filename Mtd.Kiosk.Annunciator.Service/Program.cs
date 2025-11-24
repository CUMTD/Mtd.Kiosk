using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Mtd.Kiosk.Annunciator.Azure;
using Mtd.Kiosk.Annunciator.Azure.Config;
using Mtd.Kiosk.Annunciator.Core;
using Mtd.Kiosk.Annunciator.Core.Config;
using Mtd.Kiosk.Annunciator.Readers.Raspi;
using Mtd.Kiosk.Annunciator.Readers.Raspi.Config;
using Mtd.Kiosk.Annunciator.Readers.SeaDacLite;
using Mtd.Kiosk.Annunciator.Readers.SeaDacLite.Config;
using Mtd.Kiosk.Annunciator.Readers.Simple;
using Mtd.Kiosk.Annunciator.Readers.Simple.Config;
using Mtd.Kiosk.Annunciator.Realtime.KioskApi;
using Mtd.Kiosk.Annunciator.Service;
using Mtd.Kiosk.Annunciator.Service.Extensions;
using Serilog;
using System.Reflection;
using System.Runtime.InteropServices;

try
{
	var host = Host
		.CreateDefaultBuilder(args)
		.ConfigureAppConfiguration((context, config) =>
		{
			var basePath = context.HostingEnvironment.ContentRootPath;
			config
				.SetBasePath(basePath)
				.AddJsonFile("appsettings.json", true, true)
				.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true, true)
				.AddEnvironmentVariables("Kiosk_Annunciator__");

			if (context.HostingEnvironment.IsDevelopment())
			{
				var assembly = Assembly.GetExecutingAssembly();
				config.AddUserSecrets(assembly, true);
			}
		})
		.ConfigureServices((context, services) =>
		{
			_ = services
				.Configure<HostOptions>(hostOptions =>
				{
					hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
				});

			// Options
			_ = services
				.Configure<PiReaderConfig>(context.Configuration.GetSection(PiReaderConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<PiReaderConfig>(PiReaderConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(PiReaderConfig.ConfigSectionName));

			_ = services
				.Configure<PressEveryNSecondsReaderConfig>(context.Configuration.GetSection(PressEveryNSecondsReaderConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<PressEveryNSecondsReaderConfig>(PressEveryNSecondsReaderConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(PressEveryNSecondsReaderConfig.ConfigSectionName));

			_ = services
				.Configure<SeaDacLiteReaderConfig>(context.Configuration.GetSection(SeaDacLiteReaderConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<SeaDacLiteReaderConfig>(SeaDacLiteReaderConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(SeaDacLiteReaderConfig.ConfigSectionName));

			_ = services
				.Configure<RealTimeClientConfig>(context.Configuration.GetSection(RealTimeClientConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<RealTimeClientConfig>(RealTimeClientConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(RealTimeClientConfig.ConfigSectionName));

			_ = services
				.Configure<KioskConfig>(context.Configuration.GetSection(KioskConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<KioskConfig>(KioskConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(KioskConfig.ConfigSectionName));

			_ = services
				.Configure<AzureAnnunciatorConfig>(context.Configuration.GetSection(AzureAnnunciatorConfig.ConfigSectionName))
				.AddOptionsWithValidateOnStart<AzureAnnunciatorConfig>(AzureAnnunciatorConfig.ConfigSectionName)
				.Bind(context.Configuration.GetSection(AzureAnnunciatorConfig.ConfigSectionName));

			// Readers
			_ = services.AddKeyedSingleton<IButtonReader, PiReader>(PiReader.KEY);
			_ = services.AddKeyedSingleton<IButtonReader, PressEveryNSecondsReader>(PressEveryNSecondsReader.KEY);
			_ = services.AddKeyedSingleton<IButtonReader, SeaDacReader>(SeaDacReader.KEY);

			_ = services.AddSingleton<IEnumerable<IButtonReader>>(serviceProvider =>
			{
				var env = serviceProvider.GetRequiredService<IHostEnvironment>();
				var providers = new List<IButtonReader>();

				// The PiReader Only Works on Linux
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					var piReaderConfig = serviceProvider.GetRequiredService<IOptions<PiReaderConfig>>().Value;
					if (piReaderConfig.Enabled)
					{
						providers.Add(serviceProvider.GetRequiredKeyedService<IButtonReader>(Mtd.Kiosk.Annunciator.Readers.Raspi.PiReader.KEY));
					}
				}

				// The PressEveryNSecondsReader and seaDacReader can be enabled/disabled in the config.
				var everyNConfig = serviceProvider.GetRequiredService<IOptions<PressEveryNSecondsReaderConfig>>().Value;
				if (everyNConfig.Enabled)
				{
					providers.Add(serviceProvider.GetRequiredKeyedService<IButtonReader>(PressEveryNSecondsReader.KEY));
				}

				var seaDacConfig = serviceProvider.GetRequiredService<IOptions<SeaDacLiteReaderConfig>>().Value;
				if (seaDacConfig.Enabled)
				{
					providers.Add(serviceProvider.GetRequiredKeyedService<IButtonReader>(SeaDacReader.KEY));
				}

				return providers;
			});

			// Clients
			_ = services.AddHttpClient<IKioskRealTimeClient, KioskApiRealtimeClient>(client =>
			{
				client.DefaultRequestHeaders.Add("Accept", "application/json");

				var serviceProvider = services.BuildServiceProvider();
				var kioskConfig = serviceProvider.GetRequiredService<IOptions<KioskConfig>>().Value;
				client.DefaultRequestHeaders.Add("X-ApiKey", kioskConfig.ApiKey);
			});

			// Annunciators
			_ = services.AddSingleton<IAnnunciator, AzureAnnunciator>();

			// Services
			_ = services.AddHostedService<AnnunciatorService>();
			services.AddHostedService<HeartbeatWorker>();

		})
		.UseSerilog((context, loggingConfig) =>
		{
			loggingConfig
				.ReadFrom
				.Configuration(context.Configuration);
		}, true)
		.AddOSSpecificService()
		.Build();

	await host.RunAsync();
}
catch (Exception ex)
{
	Console.WriteLine(ex.ToString());
	Log.Fatal(ex, "Host terminated unexpectedly");
	Environment.ExitCode = 1;
}
finally
{
	Log.CloseAndFlush();
}
