using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Marketplace.Core.Infrastructure;

public sealed class ConnectionFactory : IConnectionFactory, IDisposable
{
    private readonly NpgsqlDataSource _readDataSource;
    private readonly NpgsqlDataSource _writeDataSource;
    private readonly ILogger<ConnectionFactory> _logger;

    public ConnectionFactory(IConfiguration configuration, ILogger<ConnectionFactory> logger)
    {
        _logger = logger;

        var readConnectionString = configuration.GetConnectionString("ReadConnection") 
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not configured");

        var writeConnectionString = configuration.GetConnectionString("WriteConnection") 
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string not configured");

        var readBuilder = new NpgsqlDataSourceBuilder(readConnectionString);
        ConfigureDataSource(readBuilder);
        _readDataSource = readBuilder.Build();

        var writeBuilder = new NpgsqlDataSourceBuilder(writeConnectionString);
        ConfigureDataSource(writeBuilder);
        _writeDataSource = writeBuilder.Build();

        _logger.LogInformation("Connection factory initialized with read/write splitting");
    }

    private static void ConfigureDataSource(NpgsqlDataSourceBuilder builder)
    {
        builder.EnableDynamicJson();
        builder.ConnectionStringBuilder.Pooling = true;
        builder.ConnectionStringBuilder.MinPoolSize = 5;
        builder.ConnectionStringBuilder.MaxPoolSize = 100;
        builder.ConnectionStringBuilder.ConnectionIdleLifetime = 300;
        builder.ConnectionStringBuilder.ConnectionPruningInterval = 10;
    }

    public IDbConnection CreateReadConnection()
    {
        var connection = _readDataSource.CreateConnection();
        connection.Open();
        return connection;
    }

    public IDbConnection CreateWriteConnection()
    {
        var connection = _writeDataSource.CreateConnection();
        connection.Open();
        return connection;
    }

    public async Task<IDbConnection> CreateReadConnectionAsync()
    {
        var connection = _readDataSource.CreateConnection();
        await connection.OpenAsync();
        return connection;
    }

    public async Task<IDbConnection> CreateWriteConnectionAsync()
    {
        var connection = _writeDataSource.CreateConnection();
        await connection.OpenAsync();
        return connection;
    }

    public void Dispose()
    {
        _readDataSource.Dispose();
        _writeDataSource.Dispose();
    }
}
