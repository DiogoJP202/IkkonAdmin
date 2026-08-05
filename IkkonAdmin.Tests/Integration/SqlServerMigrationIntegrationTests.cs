using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests.Integration;

public sealed class SqlServerMigrationIntegrationTests
{
    [SqlServerIntegrationFact]
    public async Task TodasAsMigrations_SaoAplicadasEmSqlServerReal()
    {
        await using var database = new SqlServerIntegrationDatabase();
        await database.StartAsync();
        await using var dbContext = database.CreateDbContext();

        await dbContext.Database.MigrateAsync();

        Assert.True(await dbContext.Database.CanConnectAsync());
        Assert.Empty(await dbContext.Database.GetPendingMigrationsAsync());
        Assert.Contains(
            "20260805022530_AddStudentAutomations",
            await dbContext.Database.GetAppliedMigrationsAsync());
    }
}
