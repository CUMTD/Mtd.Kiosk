using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration;

internal class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
	public void Configure(EntityTypeBuilder<Ticket> builder)
	{
		_ = builder
			.ToTable("Ticket");

		_ = builder
			.HasKey(t => t.Id);

		_ = builder
			.Property(t => t.KioskId)
			.IsRequired();

		_ = builder
			.Property(t => t.Status)
			.IsRequired();

		_ = builder
			.Property(t => t.OpenDate)
			.IsRequired();

		_ = builder
			.Property(t => t.CloseDate);

		_ = builder
			.Property(t => t.OpenedBy)
			.IsRequired();

		_ = builder
			.Property(t => t.Title)
			.IsRequired();

		_ = builder
			.Property(t => t.Description);

		_ = builder
			.HasOne(t => t.Kiosk)
			.WithMany(k => k.Tickets)
			.HasForeignKey(t => t.KioskId);
	}
}
