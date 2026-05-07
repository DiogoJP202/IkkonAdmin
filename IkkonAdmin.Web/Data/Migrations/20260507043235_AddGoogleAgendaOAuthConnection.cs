using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAgendaOAuthConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoogleAgendaConexoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContaEmail = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    RefreshTokenProtegido = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Escopos = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConectadoPorUsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoogleAgendaConexoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoogleAgendaConexoes_UsuariosSistema_ConectadoPorUsuarioId",
                        column: x => x.ConectadoPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoogleAgendaConexoes_Ativa",
                table: "GoogleAgendaConexoes",
                column: "Ativa");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleAgendaConexoes_ConectadoPorUsuarioId",
                table: "GoogleAgendaConexoes",
                column: "ConectadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_GoogleAgendaConexoes_ContaEmail",
                table: "GoogleAgendaConexoes",
                column: "ContaEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoogleAgendaConexoes");
        }
    }
}
