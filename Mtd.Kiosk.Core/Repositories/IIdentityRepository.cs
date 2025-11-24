using Mtd.Core.Entities;
using Mtd.Core.Repositories;

namespace Mtd.Kiosk.Core.Repositories;

public interface IIdentityRepository<T> : IAsyncIdentifiable<string, T>, IRepository<T> where T : class, IIdentity<string>
{
}
