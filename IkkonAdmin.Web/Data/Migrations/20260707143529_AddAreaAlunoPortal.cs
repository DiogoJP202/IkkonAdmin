using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkkonAdmin.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAreaAlunoPortal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comunicados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Importante = table.Column<bool>(type: "bit", nullable: false),
                    Fixado = table.Column<bool>(type: "bit", nullable: false),
                    PublicadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiraEmUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    CriadoPorUsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comunicados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comunicados_UsuariosSistema_CriadoPorUsuarioId",
                        column: x => x.CriadoPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoTipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Obrigatorio = table.Column<bool>(type: "bit", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoTipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventosAlunoPortal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Local = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Importante = table.Column<bool>(type: "bit", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    GoogleEventoId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosAlunoPortal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Insignias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icone = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    RegraAutomatica = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Insignias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TurmaHorarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurmaId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time", nullable: false),
                    HoraFim = table.Column<TimeOnly>(type: "time", nullable: false),
                    Local = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurmaHorarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurmaHorarios_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TurmaInstrutores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurmaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioSistemaId = table.Column<int>(type: "int", nullable: false),
                    Principal = table.Column<bool>(type: "bit", nullable: false),
                    DataInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    DataFim = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TurmaInstrutores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TurmaInstrutores_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TurmaInstrutores_UsuariosSistema_UsuarioSistemaId",
                        column: x => x.UsuarioSistemaId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComunicadosAlvos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComunicadoId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: true),
                    TurmaId = table.Column<int>(type: "int", nullable: true),
                    Todos = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicadosAlvos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComunicadosAlvos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComunicadosAlvos_Comunicados_ComunicadoId",
                        column: x => x.ComunicadoId,
                        principalTable: "Comunicados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComunicadosAlvos_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComunicadosLeituras",
                columns: table => new
                {
                    ComunicadoId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    LidoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComunicadosLeituras", x => new { x.ComunicadoId, x.AlunoId });
                    table.ForeignKey(
                        name: "FK_ComunicadosLeituras_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComunicadosLeituras_Comunicados_ComunicadoId",
                        column: x => x.ComunicadoId,
                        principalTable: "Comunicados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentoSolicitacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoTipoId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    SolicitadoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataSolicitacaoUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataLimite = table.Column<DateOnly>(type: "date", nullable: true),
                    ObservacaoAdministrativa = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoSolicitacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoSolicitacoes_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentoSolicitacoes_DocumentoTipos_DocumentoTipoId",
                        column: x => x.DocumentoTipoId,
                        principalTable: "DocumentoTipos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentoSolicitacoes_UsuariosSistema_SolicitadoPorUsuarioId",
                        column: x => x.SolicitadoPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EventosAlunoPortalAlvos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventoAlunoPortalId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: true),
                    TurmaId = table.Column<int>(type: "int", nullable: true),
                    Todos = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosAlunoPortalAlvos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosAlunoPortalAlvos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventosAlunoPortalAlvos_EventosAlunoPortal_EventoAlunoPortalId",
                        column: x => x.EventoAlunoPortalId,
                        principalTable: "EventosAlunoPortal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventosAlunoPortalAlvos_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlunoInsignias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    InsigniaId = table.Column<int>(type: "int", nullable: false),
                    ConcedidaEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcedidaPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    Origem = table.Column<int>(type: "int", nullable: false),
                    Observacao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlunoInsignias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlunoInsignias_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlunoInsignias_Insignias_InsigniaId",
                        column: x => x.InsigniaId,
                        principalTable: "Insignias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AlunoInsignias_UsuariosSistema_ConcedidaPorUsuarioId",
                        column: x => x.ConcedidaPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Aulas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurmaId = table.Column<int>(type: "int", nullable: false),
                    TurmaHorarioId = table.Column<int>(type: "int", nullable: true),
                    InstrutorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    Inicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Local = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aulas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aulas_TurmaHorarios_TurmaHorarioId",
                        column: x => x.TurmaHorarioId,
                        principalTable: "TurmaHorarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Aulas_Turmas_TurmaId",
                        column: x => x.TurmaId,
                        principalTable: "Turmas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Aulas_UsuariosSistema_InstrutorUsuarioId",
                        column: x => x.InstrutorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DocumentoEnvios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentoSolicitacaoId = table.Column<int>(type: "int", nullable: false),
                    ArquivoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NomeArquivoOriginal = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TamanhoBytes = table.Column<long>(type: "bigint", nullable: false),
                    EnviadoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnviadoPorUsuarioId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentoEnvios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentoEnvios_DocumentoSolicitacoes_DocumentoSolicitacaoId",
                        column: x => x.DocumentoSolicitacaoId,
                        principalTable: "DocumentoSolicitacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentoEnvios_UsuariosSistema_EnviadoPorUsuarioId",
                        column: x => x.EnviadoPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FrequenciasAlunos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AulaId = table.Column<int>(type: "int", nullable: false),
                    AlunoId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Justificada = table.Column<bool>(type: "bit", nullable: false),
                    Justificativa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegistradoPorUsuarioId = table.Column<int>(type: "int", nullable: true),
                    RegistradoEmUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FrequenciasAlunos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FrequenciasAlunos_Alunos_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Alunos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FrequenciasAlunos_Aulas_AulaId",
                        column: x => x.AulaId,
                        principalTable: "Aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FrequenciasAlunos_UsuariosSistema_RegistradoPorUsuarioId",
                        column: x => x.RegistradoPorUsuarioId,
                        principalTable: "UsuariosSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlunoInsignias_AlunoId_InsigniaId",
                table: "AlunoInsignias",
                columns: new[] { "AlunoId", "InsigniaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlunoInsignias_ConcedidaPorUsuarioId",
                table: "AlunoInsignias",
                column: "ConcedidaPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_AlunoInsignias_InsigniaId",
                table: "AlunoInsignias",
                column: "InsigniaId");

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_InstrutorUsuarioId",
                table: "Aulas",
                column: "InstrutorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_Status",
                table: "Aulas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_TurmaHorarioId",
                table: "Aulas",
                column: "TurmaHorarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Aulas_TurmaId_Inicio",
                table: "Aulas",
                columns: new[] { "TurmaId", "Inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_Comunicados_Ativo_PublicadoEmUtc_ExpiraEmUtc",
                table: "Comunicados",
                columns: new[] { "Ativo", "PublicadoEmUtc", "ExpiraEmUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Comunicados_CriadoPorUsuarioId",
                table: "Comunicados",
                column: "CriadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicadosAlvos_AlunoId",
                table: "ComunicadosAlvos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicadosAlvos_ComunicadoId_AlunoId_TurmaId_Todos",
                table: "ComunicadosAlvos",
                columns: new[] { "ComunicadoId", "AlunoId", "TurmaId", "Todos" });

            migrationBuilder.CreateIndex(
                name: "IX_ComunicadosAlvos_TurmaId",
                table: "ComunicadosAlvos",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_ComunicadosLeituras_AlunoId",
                table: "ComunicadosLeituras",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoEnvios_DocumentoSolicitacaoId",
                table: "DocumentoEnvios",
                column: "DocumentoSolicitacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoEnvios_EnviadoPorUsuarioId",
                table: "DocumentoEnvios",
                column: "EnviadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoSolicitacoes_AlunoId_Status",
                table: "DocumentoSolicitacoes",
                columns: new[] { "AlunoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoSolicitacoes_DataSolicitacaoUtc",
                table: "DocumentoSolicitacoes",
                column: "DataSolicitacaoUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoSolicitacoes_DocumentoTipoId",
                table: "DocumentoSolicitacoes",
                column: "DocumentoTipoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoSolicitacoes_SolicitadoPorUsuarioId",
                table: "DocumentoSolicitacoes",
                column: "SolicitadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentoTipos_Nome",
                table: "DocumentoTipos",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosAlunoPortal_Ativo_Inicio",
                table: "EventosAlunoPortal",
                columns: new[] { "Ativo", "Inicio" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosAlunoPortal_Tipo",
                table: "EventosAlunoPortal",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_EventosAlunoPortalAlvos_AlunoId",
                table: "EventosAlunoPortalAlvos",
                column: "AlunoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosAlunoPortalAlvos_EventoAlunoPortalId_AlunoId_TurmaId_Todos",
                table: "EventosAlunoPortalAlvos",
                columns: new[] { "EventoAlunoPortalId", "AlunoId", "TurmaId", "Todos" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosAlunoPortalAlvos_TurmaId",
                table: "EventosAlunoPortalAlvos",
                column: "TurmaId");

            migrationBuilder.CreateIndex(
                name: "IX_FrequenciasAlunos_AlunoId_Status",
                table: "FrequenciasAlunos",
                columns: new[] { "AlunoId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FrequenciasAlunos_AulaId_AlunoId",
                table: "FrequenciasAlunos",
                columns: new[] { "AulaId", "AlunoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FrequenciasAlunos_RegistradoPorUsuarioId",
                table: "FrequenciasAlunos",
                column: "RegistradoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Insignias_Nome",
                table: "Insignias",
                column: "Nome",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TurmaHorarios_TurmaId_DiaSemana_HoraInicio",
                table: "TurmaHorarios",
                columns: new[] { "TurmaId", "DiaSemana", "HoraInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_TurmaInstrutores_TurmaId_UsuarioSistemaId_DataInicio",
                table: "TurmaInstrutores",
                columns: new[] { "TurmaId", "UsuarioSistemaId", "DataInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_TurmaInstrutores_UsuarioSistemaId",
                table: "TurmaInstrutores",
                column: "UsuarioSistemaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlunoInsignias");

            migrationBuilder.DropTable(
                name: "ComunicadosAlvos");

            migrationBuilder.DropTable(
                name: "ComunicadosLeituras");

            migrationBuilder.DropTable(
                name: "DocumentoEnvios");

            migrationBuilder.DropTable(
                name: "EventosAlunoPortalAlvos");

            migrationBuilder.DropTable(
                name: "FrequenciasAlunos");

            migrationBuilder.DropTable(
                name: "TurmaInstrutores");

            migrationBuilder.DropTable(
                name: "Insignias");

            migrationBuilder.DropTable(
                name: "Comunicados");

            migrationBuilder.DropTable(
                name: "DocumentoSolicitacoes");

            migrationBuilder.DropTable(
                name: "EventosAlunoPortal");

            migrationBuilder.DropTable(
                name: "Aulas");

            migrationBuilder.DropTable(
                name: "DocumentoTipos");

            migrationBuilder.DropTable(
                name: "TurmaHorarios");
        }
    }
}
