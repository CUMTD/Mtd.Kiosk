using Serilog;

namespace Mtd.Kiosk.Api.Extensions;

internal static class WebApplicationExtensions
{
	public static WebApplication ConfigureApp(this WebApplication app)
	{
		if (app.Environment.IsProduction())
		{
			_ = app.UseHsts();
		}

		// Log HTTP requests in Serilog
		_ = app.UseSerilogRequestLogging();

		_ = app.UseHttpsRedirection();

		_ = app.MapOpenApi("/openapi/{documentName}.yml");

		_ = app.UseDefaultFiles();
		_ = app.UseStaticFiles();

		_ = app.UseRouting();

		_ = app.UseAuthorization();

		var corsPolicyName = app.Configuration["Cors:PolicyName"] ?? throw new InvalidOperationException("Cors:PolicyName not defined");
		_ = app.UseCors(corsPolicyName);

		_ = app.MapControllers();

		return app;
	}
}
