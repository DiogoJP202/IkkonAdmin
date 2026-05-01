using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAlunoTurmasManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlunosTurmas",
                columns: table => new
                {
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    TurmaId = table.Column<int>(type: "int", nullable: false),
                    DataVinculo = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunosTurmas", x => new { x.AlunoId, x.TurmaId });
                    table.ForeignKey(
                        name: "FK_AlunosTurmas_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlunosTurmas_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlunosTurmas_TurmaId",
                table: "AlunosTurmas",
                column: "TurmaId");

            migrationBuilder.Sql(
                """
                INSERT INTO [AlunosTurmas] ([AlunoId], [TurmaId], [DataVinculo])
                SELECT a.[Id], a.[TurmaId], SYSUTCDATETIME()
                FROM [Alunos] a
                WHERE a.[TurmaId] IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM [AlunosTurmas] at
                      WHERE at.[AlunoId] = a.[Id]
                        AND at.[TurmaId] = a.[TurmaId]
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlunosTurmas");
        }
    }
}
