using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration;

internal class TemperatureDailyConfiguration : IEntityTypeConfiguration<TemperatureDaily>
{
	public void Configure(EntityTypeBuilder<TemperatureDaily> builder)
	{
		_ = builder
			.ToTable("TemperatureDaily");

		_ = builder
			.HasKey(t => new { t.KioskId, t.Date });

		_ = builder
			.Property(t => t.MinTempFahrenheit)
			.IsRequired();

		_ = builder
			.Property(t => t.MaxTempFahrenheit)
			.IsRequired();

		_ = builder
			.Property(t => t.AvgTempFahrenheit)
			.IsRequired();

		_ = builder
			.Property(t => t.MinRelHumidity)
			.IsRequired();

		_ = builder
			.Property(t => t.MaxRelHumidity)
			.IsRequired();

		_ = builder
			.Property(t => t.AvgRelHumidity)
			.IsRequired();
	}
}
