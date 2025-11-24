using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration;

internal class TemperatureMinutelyConfiguration : IEntityTypeConfiguration<TemperatureMinutely>
{
	public void Configure(EntityTypeBuilder<TemperatureMinutely> builder)
	{
		_ = builder
			.ToTable("TemperatureMinutely");

		_ = builder
			.HasKey(t => new { t.KioskId, t.Timestamp, t.SensorType });

		_ = builder
			.Property(t => t.TempFahrenheit)
			.IsRequired();

		_ = builder
			.Property(t => t.RelHumidity)
			.IsRequired();

		_ = builder
			.Property(t => t.SensorType)
			.IsRequired();

	}
}
