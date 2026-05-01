using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConfiguracoesSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfiguracoesSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NomeEscola = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    EmailFinanceiro = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    TelefoneContato = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ValorMensalidadePadrao = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    DiaVencimentoPadrao = table.Column<int>(type: "int", nullable: false),
                    DiasToleranciaAtraso = table.Column<int>(type: "int", nullable: false),
                    PercentualMultaAtraso = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PercentualJurosMes = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AplicarMultaJurosAutomaticamente = table.Column<bool>(type: "bit", nullable: false),
                    GerarMensalidadesAutomaticamente = table.Column<bool>(type: "bit", nullable: false),
                    EnviarLembreteCobranca = table.Column<bool>(type: "bit", nullable: false),
                    DiasAntecedenciaLembrete = table.Column<int>(type: "int", nullable: false),
                    MensagemBoasVindasPadrao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChecklistAdmissaoPadrao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PermitirDesligamentoComPendencia = table.Column<bool>(type: "bit", nullable: false),
                    AtualizarNivelAutomaticamenteNaGraduacao = table.Column<bool>(type: "bit", nullable: false),
                    UltimaAtualizacaoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesSistema", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesSistema");
        }
    }
}
