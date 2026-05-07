using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleAgendaAndInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventarioItens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CodigoInterno = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    Categoria = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EstadoConservacao = table.Column<int>(type: "int", nullable: false),
                    Localizacao = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DisponivelParaAula = table.Column<bool>(type: "bit", nullable: false),
                    DisponivelParaEvento = table.Column<bool>(type: "bit", nullable: false),
                    DataAquisicao = table.Column<DateOnly>(type: "date", nullable: true),
                    ValorEstimado = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AtualizadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CriadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    AtualizadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioItens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarioItens_UsuariosSistema_AtualizadoPorUsuarioId",
                        column: x => x.AtualizadoPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventarioItens_UsuariosSistema_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InventarioMovimentacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventarioItemId = table.Column<int>(type: "int", nullable: false),
                    GoogleEventId = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    TipoMovimentacao = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    DataInicioUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataFimUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsavelUsuarioId = table.Column<int>(type: "int", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    CriadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventarioMovimentacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventarioMovimentacoes_InventarioItens_InventarioItemId",
                        column: x => x.InventarioItemId,
                        principalTable: "InventarioItens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventarioMovimentacoes_UsuariosSistema_ResponsavelUsuarioId",
                        column: x => x.ResponsavelUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_Ativo",
                table: "InventarioItens",
                column: "Ativo");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_AtualizadoPorUsuarioId",
                table: "InventarioItens",
                column: "AtualizadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_Categoria",
                table: "InventarioItens",
                column: "Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_Categoria_Status_Ativo",
                table: "InventarioItens",
                columns: new[] { "Categoria", "Status", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_CodigoInterno",
                table: "InventarioItens",
                column: "CodigoInterno",
                unique: true,
                filter: "[CodigoInterno] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_CriadoPorUsuarioId",
                table: "InventarioItens",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_Status",
                table: "InventarioItens",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioItens_Tipo",
                table: "InventarioItens",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioMovimentacoes_DataInicioUtc",
                table: "InventarioMovimentacoes",
                column: "DataInicioUtc");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioMovimentacoes_GoogleEventId",
                table: "InventarioMovimentacoes",
                column: "GoogleEventId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioMovimentacoes_InventarioItemId",
                table: "InventarioMovimentacoes",
                column: "InventarioItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioMovimentacoes_ResponsavelUsuarioId",
                table: "InventarioMovimentacoes",
                column: "ResponsavelUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_InventarioMovimentacoes_TipoMovimentacao",
                table: "InventarioMovimentacoes",
                column: "TipoMovimentacao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventarioMovimentacoes");

            migrationBuilder.DropTable(
                name: "InventarioItens");
        }
    }
}
