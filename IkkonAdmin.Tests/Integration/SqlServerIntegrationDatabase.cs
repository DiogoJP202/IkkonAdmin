using IkkonAdmin.Web.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace IkkonAdmin.Tests.Integration;

internal sealed class SqlServerIntegrationDatabase : IAsyncDisposable
{
    private readonly MsSqlContainer container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithCleanUp(true)
        .Build();

    public string ConnectionString => container.GetConnectionString();

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        return container.StartAsync(cancellationToken);
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    public ValueTask DisposeAsync()
    {
        return container.DisposeAsync();
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class SqlServerIntegrationFactAttribute : FactAttribute
{
    public SqlServerIntegrationFactAttribute()
    {
        if (!string.Equals(
                Environment.GetEnvironmentVariable("RUN_SQL_INTEGRATION_TESTS"),
                "1",
                StringComparison.Ordinal))
        {
            Skip = "Defina RUN_SQL_INTEGRATION_TESTS=1 para executar com Docker e SQL Server real.";
        }
    }
}
