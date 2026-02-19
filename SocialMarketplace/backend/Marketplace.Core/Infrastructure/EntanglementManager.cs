using System.Data;
using Microsoft.Extensions.Logging;

namespace Marketplace.Core.Infrastructure;

public sealed class EntanglementManager : IEntanglementManager
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly ILogger<EntanglementManager> _logger;

    public EntanglementManager(IConnectionFactory connectionFactory, ILogger<EntanglementManager> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<IDbConnection, IDbTransaction, Task<T>> action)
    {
        await using var connection = (Npgsql.NpgsqlConnection)await _connectionFactory.CreateWriteConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var result = await action(connection, transaction);
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed, rolling back");
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> action)
    {
        await using var connection = (Npgsql.NpgsqlConnection)await _connectionFactory.CreateWriteConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            await action(connection, transaction);
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed, rolling back");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
