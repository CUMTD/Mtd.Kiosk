using Mtd.Infrastructure.EFCore.Repositories;
using Mtd.Kiosk.Core.Entities;
using Mtd.Kiosk.Core.Repositories;

namespace Mtd.Kiosk.Infrastructure.EfCore.Repository;

public class TicketNoteRepository(KioskContext context) : AsyncEFIdentifiableRepository<string, TicketNote>(context), ITicketNoteRepository
{

}
