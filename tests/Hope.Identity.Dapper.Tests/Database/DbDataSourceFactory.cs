using System.Data.Common;
using System.Reflection;
using DbUp;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

using DefaultTypeMap = Dapper.DefaultTypeMap;

namespace Hope.Identity.Dapper.Tests;

public sealed class DbDataSourceFactory : IAsyncLifetime
{
    
    private PostgreSqlContainer? _dbContainer;
    private Respawner? _respawner;
    private DbDataSource? _respawnerDataSource;


    public async Task InitializeAsync()
    {
        _dbContainer = new PostgreSqlBuilder().Build();
        await _dbContainer.StartAsync();

        var connectionString = _dbContainer.GetConnectionString();
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransaction()
            .Build();

        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            throw new InvalidOperationException($"Database migration failed. Details: {result.Error}");
        }
        _respawnerDataSource = Create();
        await using var connection = await _respawnerDataSource.OpenConnectionAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public", "identity"]
        });
    }

    public async Task DisposeAsync()
    {
        if (_dbContainer is not null)
        {
            await _dbContainer.DisposeAsync();
        }
    }


    public DbDataSource Create()
    {
        if (_dbContainer is null)
        {
            throw new InvalidOperationException("Factory data is not initialized");
        }
        var connectionString = _dbContainer.GetConnectionString();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        DefaultTypeMap.MatchNamesWithUnderscores = true;

        return dataSourceBuilder.Build();
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null || _respawnerDataSource is null)
        {
            throw new InvalidOperationException("Factory data is not initialized");
        }
        await using var connection = await _respawnerDataSource.OpenConnectionAsync();

        await _respawner.ResetAsync(connection);
    }

}
