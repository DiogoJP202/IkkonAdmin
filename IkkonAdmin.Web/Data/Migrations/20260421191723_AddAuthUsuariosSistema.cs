using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthUsuariosSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UsuariosSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Login = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LoginNormalizado = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmailNormalizado = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NomeExibicao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TipoAcesso = table.Column<int>(type: "int", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: true),
                    DataCriacaoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UltimoLoginUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosSistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsuariosSistema_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_AlunoId",
                table: "UsuariosSistema",
                column: "AlunoId",
                unique: true,
                filter: "[AlunoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_EmailNormalizado",
                table: "UsuariosSistema",
                column: "EmailNormalizado",
                unique: true,
                filter: "[EmailNormalizado] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_LoginNormalizado",
                table: "UsuariosSistema",
                column: "LoginNormalizado",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosSistema_TipoAcesso_Ativo",
                table: "UsuariosSistema",
                columns: new[] { "TipoAcesso", "Ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsuariosSistema");
        }
    }
}
