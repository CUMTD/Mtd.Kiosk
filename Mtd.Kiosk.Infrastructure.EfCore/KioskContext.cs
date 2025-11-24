using Microsoft.EntityFrameworkCore;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Infrastructure.EfCore.Configuration;

namespace Mtd.Kiosk.Infrastructure.EfCore;

public class KioskContext(DbContextOptions<KioskContext> options) : DbContext(options)
{
	public DbSet<Health> Heartbeats { get; protected set; }
	public DbSet<Core.Entities.Kiosk> Kiosks { get; protected set; }
	public DbSet<Ticket> Tickets { get; protected set; }
	public DbSet<TicketNote> TicketNotes { get; protected set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfiguration(new HealthConfiguration());
		modelBuilder.ApplyConfiguration(new KioskConfiguration());
		modelBuilder.ApplyConfiguration(new TicketConfiguration());
		modelBuilder.ApplyConfiguration(new TicketNoteConfiguration());
		modelBuilder.ApplyConfiguration(new TemperatureMinutelyConfiguration());
		modelBuilder.ApplyConfiguration(new TemperatureDailyConfiguration());

	}
}
