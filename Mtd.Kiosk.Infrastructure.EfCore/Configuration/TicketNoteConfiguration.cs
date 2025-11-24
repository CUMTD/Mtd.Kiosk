using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mtd.Kiosk.Core.Entities;

namespace Mtd.Kiosk.Infrastructure.EfCore.Configuration;

internal class TicketNoteConfiguration : IEntityTypeConfiguration<TicketNote>
{
	public void Configure(EntityTypeBuilder<TicketNote> builder)
	{
		_ = builder
			.ToTable("TicketNote");

		_ = builder
			.HasKey(t => t.Id);

		_ = builder
			.Property(t => t.MarkdownBody);

		_ = builder
			.Property(t => t.TicketId)
			.IsRequired();

		_ = builder
			.Property(t => t.CreatedDate)
			.IsRequired();

		_ = builder
			.Property(t => t.CreatedBy);

		_ = builder
			.Property(t => t.Deleted)
			.HasDefaultValue(false)
			.IsRequired();

		_ = builder
			.HasOne(note => note.Ticket)
			.WithMany(ticket => ticket.Notes)
			.HasForeignKey(note => note.TicketId);

	}
}
