using Mtd.Core.Repositories;

namespace Mtd.Kiosk.Core.Repositories;

public interface IRepository<T> : IAsyncReadable<T, IReadOnlyCollection<T>>, IAsyncWriteable<T, IReadOnlyCollection<T>> where T : class
{
}
