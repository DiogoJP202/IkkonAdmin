using System.Data;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Data;

public static class DatabaseBootstrap
{
    private const string InitialMigrationId = "20260421025358_InitialCreate";
    private const string InitialProductVersion = "10.0.6";

    public static void EnsureDatabaseReady(ApplicationDbContext dbContext)
    {
        TentarRegistrarBaselineQuandoSchemaJaExiste(dbContext);
        dbContext.Database.Migrate();
        GarantirSchemaAlunoTurma(dbContext);
        SeedData.Initialize(dbContext);
    }

    private static void TentarRegistrarBaselineQuandoSchemaJaExiste(ApplicationDbContext dbContext)
    {
        if (!dbContext.Database.CanConnect())
        {
            return;
        }

        var appliedMigrations = dbContext.Database.GetAppliedMigrations();
        if (appliedMigrations.Any())
        {
            return;
        }

        if (!SchemaPrincipalJaExiste(dbContext))
        {
            return;
        }

        dbContext.Database.ExecuteSqlRaw(
            """
            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
            END;
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0})
            BEGIN
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES ({0}, {1});
            END;
            """,
            InitialMigrationId,
            InitialProductVersion);
    }

    private static bool SchemaPrincipalJaExiste(ApplicationDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();
        var deveFechar = connection.State != ConnectionState.Open;

        if (deveFechar)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT COUNT(1)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_SCHEMA = 'dbo'
                  AND TABLE_NAME IN ('Turmas', 'Alunos', 'Mensalidades', 'ExamesGraduacao');
                """;

            var total = Convert.ToInt32(command.ExecuteScalar() ?? 0);
            return total >= 3;
        }
        finally
        {
            if (deveFechar)
            {
                connection.Close();
            }
        }
    }

    private static void GarantirSchemaAlunoTurma(ApplicationDbContext dbContext)
    {
        dbContext.Database.ExecuteSqlRaw(
            """
            IF OBJECT_ID(N'[dbo].[AlunosTurmas]', N'U') IS NULL
            BEGIN
                CREATE TABLE [dbo].[AlunosTurmas](
                    [AlunoId] INT NOT NULL,
                    [TurmaId] INT NOT NULL,
                    [DataVinculo] DATETIME2 NOT NULL CONSTRAINT [DF_AlunosTurmas_DataVinculo] DEFAULT (SYSUTCDATETIME()),
                    CONSTRAINT [PK_AlunosTurmas] PRIMARY KEY ([AlunoId], [TurmaId]),
                    CONSTRAINT [FK_AlunosTurmas_Alunos_AlunoId] FOREIGN KEY ([AlunoId]) REFERENCES [dbo].[Alunos]([Id]) ON DELETE CASCADE,
                    CONSTRAINT [FK_AlunosTurmas_Turmas_TurmaId] FOREIGN KEY ([TurmaId]) REFERENCES [dbo].[Turmas]([Id]) ON DELETE CASCADE
                );

                CREATE INDEX [IX_AlunosTurmas_TurmaId] ON [dbo].[AlunosTurmas]([TurmaId]);
            END;
            """);

        dbContext.Database.ExecuteSqlRaw(
            """
            INSERT INTO [dbo].[AlunosTurmas] ([AlunoId], [TurmaId], [DataVinculo])
            SELECT a.[Id], a.[TurmaId], SYSUTCDATETIME()
            FROM [dbo].[Alunos] a
            WHERE a.[TurmaId] IS NOT NULL
              AND NOT EXISTS (
                  SELECT 1
                  FROM [dbo].[AlunosTurmas] at
                  WHERE at.[AlunoId] = a.[Id]
                    AND at.[TurmaId] = a.[TurmaId]
              );
            """);
    }
}
