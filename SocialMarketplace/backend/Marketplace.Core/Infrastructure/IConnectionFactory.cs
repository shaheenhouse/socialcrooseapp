using System.Data;

namespace Marketplace.Core.Infrastructure;

public interface IConnectionFactory
{
    IDbConnection CreateReadConnection();
    IDbConnection CreateWriteConnection();
    Task<IDbConnection> CreateReadConnectionAsync();
    Task<IDbConnection> CreateWriteConnectionAsync();
}
