using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration;

internal class KioskConfiguration : IEntityTypeConfiguration<Core.Entities.Kiosk>
{
	public void Configure(EntityTypeBuilder<Core.Entities.Kiosk> builder)
	{
		_ = builder.ToTable("Kiosk");

		_ = builder.HasKey(k => k.Id);

		_ = builder.Property(k => k.Deleted)
			.HasDefaultValue(false)
			.IsRequired();
	}
}
