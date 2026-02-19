using System.Data;

namespace Marketplace.Core.Infrastructure;

public interface IEntanglementManager
{
    Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> action);
    Task ExecuteInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> action);
}
