using Mtd.Kiosk.Api.Extensions;
using Serilog;

// Bootstrap logger to log to console in case app crashes before Serilog is configured
// Will be overwritten when Serilog is fully configured.
Log.Logger = new LoggerConfiguration()
	.MinimumLevel.Debug()
	.WriteTo.Console()
	.CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Handles unobserved exceptions thrown in Tasks that were not awaited or whose exceptions were never accessed.
// This event is raised when the Task is garbage collected and the exception remains unhandled.
// Calling SetObserved() prevents the process from being affected and suppresses the default behavior.
TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
{
	Log.Fatal(eventArgs.Exception, "An unobserved task exception occurred.");
	eventArgs.SetObserved();
};

builder.Configure();

var app = builder.Build();

app.ConfigureApp();

try
{
	Log.Information("Application started successfully in {Environment}", app.Environment.EnvironmentName);
	await app.RunAsync();
}
catch (Exception ex)
{
	Log.Fatal(ex, "Application terminated unexpectedly");
	throw;
}
finally
{
	Log.CloseAndFlush();
}
