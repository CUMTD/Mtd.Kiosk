namespace Mtd.Kiosk.RealTime.Config;

public class ApiRealTimeClientConfig
{
	public required string BaseUrl { get; set; }
	public required string SmEndpoint { get; set; }
	public required string GmEndpoint { get; set; }
	public Uri SmUri => new(BaseUrl + SmEndpoint);
	public Uri GmUri => new(BaseUrl + GmEndpoint);
}
