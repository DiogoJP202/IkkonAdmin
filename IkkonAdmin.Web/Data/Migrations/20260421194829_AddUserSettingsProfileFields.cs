using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettingsProfileFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoPerfilUrl",
                table: "UsuariosSistema",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdiomaPreferencia",
                table: "UsuariosSistema",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "NotificarEmail",
                table: "UsuariosSistema",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotificarSistema",
                table: "UsuariosSistema",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "UsuariosSistema",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TemaPreferencia",
                table: "UsuariosSistema",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoPerfilUrl",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "IdiomaPreferencia",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "NotificarEmail",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "NotificarSistema",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "UsuariosSistema");

            migrationBuilder.DropColumn(
                name: "TemaPreferencia",
                table: "UsuariosSistema");
        }
    }
}
