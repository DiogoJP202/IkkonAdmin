using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleTipoAcessoAndPermissionExpansion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoAcesso",
                table: "RolesSistema",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_RolesSistema_TipoAcesso_Ativo",
                table: "RolesSistema",
                columns: new[] { "TipoAcesso", "Ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RolesSistema_TipoAcesso_Ativo",
                table: "RolesSistema");

            migrationBuilder.DropColumn(
                name: "TipoAcesso",
                table: "RolesSistema");
        }
    }
}
