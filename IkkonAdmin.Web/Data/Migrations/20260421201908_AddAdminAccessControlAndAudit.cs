using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAccessControlAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataExclusaoUtc",
                table: "UsuariosSistema",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Excluido",
                table: "UsuariosSistema",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ExcluidoPorUsuarioId",
                table: "UsuariosSistema",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuditoriaLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioResponsavelId = table.Column<int>(type: "int", nullable: true),
                    UsuarioAfetadoId = table.Column<int>(type: "int", nullable: true),
                    Acao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Entidade = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntidadeId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    DadosAntesJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DadosDepoisJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnderecoIp = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    DataEventoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditoriaLogs_UsuariosSistema_UsuarioAfetadoId",
                        column: x => x.UsuarioAfetadoId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuditoriaLogs_UsuariosSistema_UsuarioResponsavelId",
                        column: x => x.UsuarioResponsavelId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PermissoesSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    IsSistema = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacaoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissoesSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolesSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    IsSistema = table.Column<bool>(type: "bit", nullable: false),
                    DataCriacaoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosPermissoes",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    PermissaoId = table.Column<int>(type: "int", nullable: false),
                    DataConcessaoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPermissoes", x => new { x.UsuarioId, x.PermissaoId });
                    table.ForeignKey(
                        name: "FK_UsuariosPermissoes_PermissoesSistema_PermissaoId",
                        column: x => x.PermissaoId,
                        principalTable: "PermissoesSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosPermissoes_UsuariosSistema_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolesPermissoes",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissaoId = table.Column<int>(type: "int", nullable: false),
                    DataVinculoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermissoes", x => new { x.RoleId, x.PermissaoId });
                    table.ForeignKey(
                        name: "FK_RolesPermissoes_PermissoesSistema_PermissaoId",
                        column: x => x.PermissaoId,
                        principalTable: "PermissoesSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermissoes_RolesSistema_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RolesSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosRoles",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DataVinculoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosRoles", x => new { x.UsuarioId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_RolesSistema_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RolesSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosRoles_UsuariosSistema_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaLogs_DataEventoUtc",
                table: "AuditoriaLogs",
                column: "DataEventoUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaLogs_Entidade_Acao",
                table: "AuditoriaLogs",
                columns: new[] { "Entidade", "Acao" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaLogs_UsuarioAfetadoId",
                table: "AuditoriaLogs",
                column: "UsuarioAfetadoId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaLogs_UsuarioResponsavelId",
                table: "AuditoriaLogs",
                column: "UsuarioResponsavelId");

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesSistema_Ativo_IsSistema",
                table: "PermissoesSistema",
                columns: new[] { "Ativo", "IsSistema" });

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesSistema_Codigo",
                table: "PermissoesSistema",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermissoes_PermissaoId",
                table: "RolesPermissoes",
                column: "PermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesSistema_Ativo_IsSistema",
                table: "RolesSistema",
                columns: new[] { "Ativo", "IsSistema" });

            migrationBuilder.CreateIndex(
                name: "IX_RolesSistema_Codigo",
                table: "RolesSistema",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosPermissoes_PermissaoId",
                table: "UsuariosPermissoes",
                column: "PermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosRoles_RoleId",
                table: "UsuariosRoles",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaLogs");

            migrationBuilder.DropTable(
                name: "RolesPermissoes");

            migrationBuilder.DropTable(
                name: "UsuariosPermissoes");

            migrationBuilder.DropTable(
                name: "UsuariosRoles");

            migrationBuilder.DropTable(
                name: "PermissoesSistema");

            migrationBuilder.DropTable(
                name: "RolesSistema");

            migrationBuilder.DropColumn(
                name: "DataExclusaoUtc",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "Excluido",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "ExcluidoPorUsuarioId",
                table: "UsuariosSistema");
        }
    }
}
