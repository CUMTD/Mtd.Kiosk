using Mtd.Kiosk.Core.Entities;
using System.Text.Json.Serialization;

namespace Mtd.Kiosk.Api.Models;

/// <summary>
/// The health of a kiosk.
/// </summary>
/// <remarks>
/// Constructor for KioskHealthResponseModel.
/// </remarks>
/// <param name="kioskId">The ID of the kiosk.</param>
/// <param name="healthStatuses">The health statuses of the individual components.</param>
/// <param name="openTicketCount">The number of open tickets on the kiosk.</param>
public class KioskHealthResponseModel(string kioskId, IndividualHealthStatuses healthStatuses, int openTicketCount)
{
	/// <summary>
	/// The id of the kiosk.
	/// </summary>
	public string KioskId { get; set; } = kioskId;

	/// <summary>
	/// The overall health of the kiosk.
	/// </summary>
	public HealthStatus OverallHealth => IndividualHealthStatuses.GetOverallStatus();

	/// <summary>
	/// A collection of health statuses for each component.
	/// </summary>
	[JsonPropertyName("healthStatuses")]
	public IndividualHealthStatuses IndividualHealthStatuses { get; set; } = healthStatuses;

	/// <summary>
	/// The number of open tickets for the kiosk.
	/// </summary>
	[JsonPropertyName("openTicketCount")]
	public int OpenTicketCount { get; set; } = openTicketCount;
}

