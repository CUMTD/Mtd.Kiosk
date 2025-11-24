using System.Text.Json.Serialization;

namespace Mtd.Kiosk.RealTime.Entities.Api;

public class Departure
{
	[JsonPropertyName("stopId")]
	public required string StopId { get; set; }
	[JsonPropertyName("headsign")]
	public required string Headsign { get; set; }
	[JsonPropertyName("routeId")]
	public required string RouteId { get; set; }
	[JsonPropertyName("direction")]
	public required string Direction { get; set; }
	[JsonPropertyName("blockId")]
	public required string BlockId { get; set; }
	[JsonPropertyName("recordedTime")]
	public required DateTime RecordedTime { get; set; }
	[JsonPropertyName("scheduledDeparture")]
	public required DateTime ScheduledDeparture { get; set; }
	[JsonPropertyName("estimatedDeparture")]
	public required DateTime EstimatedDeparture { get; set; }
	[JsonPropertyName("vehicleId")]
	public required string VehicleId { get; set; }
	[JsonPropertyName("originStopId")]
	public required string OriginStopId { get; set; }
	[JsonPropertyName("destinationStopId")]
	public required string DestinationStopId { get; set; }
	[JsonPropertyName("latitude")]
	public required double Latitude { get; set; }
	[JsonPropertyName("longitude")]
	public required double Longitude { get; set; }
	[JsonPropertyName("shapeId")]
	public required string ShapeId { get; set; }
	[JsonPropertyName("tripPrefix")]
	public required string TripPrefix { get; set; }
	[JsonPropertyName("tripId")]
	public required string TripId { get; set; }
	[JsonPropertyName("minutesTillDeparture")]
	public required int MinutesTillDeparture { get; set; }
	[JsonPropertyName("isRealTime")]
	public required bool IsRealTime { get; set; }
	[JsonPropertyName("isHopper")]
	public required bool IsHopper { get; set; }
	[JsonPropertyName("destination")]
	public required string Destination { get; set; }
	[JsonPropertyName("departsIn")]
	public required string DepartsIn { get; set; }
	[JsonPropertyName("isIStop")]
	public required bool IsIStop { get; set; }
	[JsonPropertyName("routeNumber")]
	public required string RouteNumber { get; set; }
	[JsonPropertyName("routeSortNumber")]
	public required int RouteSortNumber { get; set; }
	[JsonPropertyName("routeColor")]
	public required string RouteColor { get; set; }
	[JsonPropertyName("friendlyRouteName")]
	public required string FriendlyRouteName { get; set; }
	[JsonPropertyName("uniqueId")]
	public required string UniqueId { get; set; }
}
