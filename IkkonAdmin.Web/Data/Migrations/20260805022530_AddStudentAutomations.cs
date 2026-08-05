using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentAutomations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Aulas_TurmaHorarioId",
                table: "Aulas");

            migrationBuilder.AddColumn<bool>(
                name: "AvaliarConquistasAutomaticamente",
                table: "ConfiguracoesSistema",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "GerarAulasAutomaticamente",
                table: "ConfiguracoesSistema",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "HorarioAutomacoesAreaAluno",
                table: "ConfiguracoesSistema",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(3, 30, 0));

            migrationBuilder.AddColumn<int>(
                name: "HorizonteGeracaoAulasSemanas",
                table: "ConfiguracoesSistema",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataOcorrenciaRecorrencia",
                table: "Aulas",
                type: "date",
                nullable: true);

            migrationBuilder.Sql(
                """
                WITH [Occurrences] AS (
                    SELECT
                        [Id],
                        CONVERT(date, [Inicio]) AS [OccurrenceDate],
                        ROW_NUMBER() OVER (
                            PARTITION BY [TurmaHorarioId], CONVERT(date, [Inicio])
                            ORDER BY [Id]
                        ) AS [RowNumber]
                    FROM [Aulas]
                    WHERE [TurmaHorarioId] IS NOT NULL
                )
                UPDATE [Aulas]
                SET [DataOcorrenciaRecorrencia] = [Occurrences].[OccurrenceDate]
                FROM [Aulas]
                INNER JOIN [Occurrences] ON [Occurrences].[Id] = [Aulas].[Id]
                WHERE [Occurrences].[RowNumber] = 1;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_TurmaHorarioId_DataOcorrenciaRecorrencia",
                table: "Aulas",
                columns: new[] { "TurmaHorarioId", "DataOcorrenciaRecorrencia" },
                unique: true,
                filter: "[TurmaHorarioId] IS NOT NULL AND [DataOcorrenciaRecorrencia] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Aulas_TurmaHorarioId_DataOcorrenciaRecorrencia",
                table: "Aulas");

            migrationBuilder.DropColumn(
                name: "AvaliarConquistasAutomaticamente",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "GerarAulasAutomaticamente",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "HorarioAutomacoesAreaAluno",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "HorizonteGeracaoAulasSemanas",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "DataOcorrenciaRecorrencia",
                table: "Aulas");

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_TurmaHorarioId",
                table: "Aulas",
                column: "TurmaHorarioId");
        }
    }
}
