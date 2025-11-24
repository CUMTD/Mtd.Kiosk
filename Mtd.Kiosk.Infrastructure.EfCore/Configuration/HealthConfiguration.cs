using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration;

internal class HealthConfiguration : IEntityTypeConfiguration<Health>
{
	public void Configure(EntityTypeBuilder<Health> builder)
	{
		_ = builder.ToTable("Health");

		_ = builder.HasKey(k => new { k.KioskId, k.Type });


		_ = builder
			.Property(k => k.LastHeartbeat)
			.IsRequired();

		_ = builder
			.Property(k => k.Type)
			.IsRequired();

		_ = builder
			.HasOne(k => k.Kiosk)
			.WithMany()
			.HasForeignKey(k => k.KioskId);
	}
}
