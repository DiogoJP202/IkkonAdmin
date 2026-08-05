using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace IkkonAdmin.Tests;

public class AreaAlunoAdminServiceTests
{
    [Fact]
    public async Task ExcluirAulaAsync_ComFrequencia_CancelaAula()
    {
        await using var dbContext = CriarDbContext();
        var turma = CriarTurma();
        var aluno = CriarAluno();
        var aula = new Aula
        {
            Turma = turma,
            Inicio = DateTime.Today.AddHours(19),
            Fim = DateTime.Today.AddHours(20),
            Status = StatusAulaEnum.Agendada
        };

        dbContext.AddRange(turma, aluno, aula);
        await dbContext.SaveChangesAsync();

        dbContext.FrequenciasAlunos.Add(new FrequenciaAluno
        {
            AulaId = aula.Id,
            AlunoId = aluno.Id,
            Status = StatusFrequenciaEnum.Presente
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ExcluirAulaAsync(aula.Id);

        Assert.True(resultado.Success);
        Assert.Equal(OperationResultStatus.Success, resultado.Status);
        Assert.Equal(StatusAulaEnum.Cancelada, (await dbContext.Aulas.FindAsync(aula.Id))!.Status);
    }

    [Fact]
    public async Task ExcluirAulaAsync_SemFrequencia_RemoveAula()
    {
        await using var dbContext = CriarDbContext();
        var aula = new Aula
        {
            Turma = CriarTurma(),
            Inicio = DateTime.Today.AddHours(19),
            Fim = DateTime.Today.AddHours(20),
            Status = StatusAulaEnum.Agendada
        };

        dbContext.Aulas.Add(aula);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ExcluirAulaAsync(aula.Id);

        Assert.True(resultado.Success);
        Assert.Equal(OperationResultStatus.Success, resultado.Status);
        Assert.Empty(dbContext.Aulas);
    }

    [Fact]
    public async Task ExcluirDocumentoTipoAsync_ComSolicitacao_DesativaTipo()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var tipo = new DocumentoTipo { Nome = "RG", Ativo = true };

        dbContext.AddRange(aluno, tipo);
        await dbContext.SaveChangesAsync();

        dbContext.DocumentoSolicitacoes.Add(new DocumentoSolicitacao
        {
            AlunoId = aluno.Id,
            DocumentoTipoId = tipo.Id,
            Status = DocumentoStatusEnum.Solicitado
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ExcluirDocumentoTipoAsync(tipo.Id);

        Assert.True(resultado.Success);
        Assert.Equal(OperationResultStatus.Success, resultado.Status);
        Assert.False((await dbContext.DocumentoTipos.FindAsync(tipo.Id))!.Ativo);
    }

    [Fact]
    public async Task ExcluirDocumentoSolicitacaoAsync_ComEnvio_BloqueiaExclusao()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var tipo = new DocumentoTipo { Nome = "Atestado", Ativo = true };

        dbContext.AddRange(aluno, tipo);
        await dbContext.SaveChangesAsync();

        var solicitacao = new DocumentoSolicitacao
        {
            AlunoId = aluno.Id,
            DocumentoTipoId = tipo.Id,
            Status = DocumentoStatusEnum.Enviado
        };

        dbContext.DocumentoSolicitacoes.Add(solicitacao);
        await dbContext.SaveChangesAsync();

        dbContext.DocumentoEnvios.Add(new DocumentoEnvio
        {
            DocumentoSolicitacaoId = solicitacao.Id,
            ArquivoUrl = "documentos/arquivo.pdf",
            NomeArquivoOriginal = "arquivo.pdf",
            ContentType = "application/pdf",
            TamanhoBytes = 128
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ExcluirDocumentoSolicitacaoAsync(solicitacao.Id);

        Assert.False(resultado.Success);
        Assert.Equal(OperationResultStatus.ValidationError, resultado.Status);
        Assert.NotNull(await dbContext.DocumentoSolicitacoes.FindAsync(solicitacao.Id));
    }

    [Fact]
    public async Task AtualizarComunicadoAsync_AlteraAlvoEReativa()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var comunicado = new Comunicado
        {
            Titulo = "Antigo",
            Conteudo = "Conteudo antigo",
            Ativo = false
        };

        comunicado.Alvos.Add(new ComunicadoAlvo { Todos = true });
        dbContext.AddRange(aluno, comunicado);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.AtualizarComunicadoAsync(
            comunicado.Id,
            new ComunicadoFormViewModel
            {
                Titulo = "Novo aviso",
                Conteudo = "Novo conteudo",
                AlvoTipo = ComunicadoAlvoTipoEnum.Aluno,
                AlunoId = aluno.Id,
                Importante = true,
                PublicadoEmUtc = DateTime.UtcNow
            });

        var atualizado = await dbContext.Comunicados
            .Include(x => x.Alvos)
            .FirstAsync(x => x.Id == comunicado.Id);

        Assert.True(resultado.Success);
        Assert.Equal(OperationResultStatus.Success, resultado.Status);
        Assert.True(atualizado.Ativo);
        Assert.True(atualizado.Importante);
        Assert.Single(atualizado.Alvos);
        Assert.Equal(aluno.Id, atualizado.Alvos.Single().AlunoId);
        Assert.False(atualizado.Alvos.Single().Todos);
    }

    [Fact]
    public async Task ExcluirComunicadoAsync_ComLeitura_DesativaComunicado()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var comunicado = new Comunicado
        {
            Titulo = "Aviso",
            Conteudo = "Conteudo",
            Ativo = true
        };

        dbContext.AddRange(aluno, comunicado);
        await dbContext.SaveChangesAsync();

        dbContext.ComunicadosLeituras.Add(new ComunicadoLeitura
        {
            AlunoId = aluno.Id,
            ComunicadoId = comunicado.Id
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ExcluirComunicadoAsync(comunicado.Id);

        Assert.True(resultado.Success);
        Assert.Equal(OperationResultStatus.Success, resultado.Status);
        Assert.False((await dbContext.Comunicados.FindAsync(comunicado.Id))!.Ativo);
    }

    [Fact]
    public async Task AtualizarEventoAsync_ComAlvoInvalido_Falha()
    {
        await using var dbContext = CriarDbContext();
        var evento = new EventoAlunoPortal
        {
            Titulo = "Evento",
            Inicio = DateTime.Today.AddDays(7),
            Fim = DateTime.Today.AddDays(7).AddHours(2),
            Ativo = true
        };

        evento.Alvos.Add(new EventoAlunoPortalAlvo { Todos = true });
        dbContext.EventosAlunoPortal.Add(evento);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.AtualizarEventoAsync(
            evento.Id,
            new EventoAlunoFormViewModel
            {
                Titulo = "Evento atualizado",
                Inicio = evento.Inicio,
                Fim = evento.Fim,
                AlvoTipo = ComunicadoAlvoTipoEnum.Turma,
                TurmaId = 999
            });

        Assert.False(resultado.Success);
        Assert.Equal(OperationResultStatus.ValidationError, resultado.Status);
        Assert.True((await dbContext.EventosAlunoPortal.FindAsync(evento.Id))!.Ativo);
    }

    [Fact]
    public async Task AtualizarAlunoInsigniaAsync_QuandoDuplicada_Falha()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var primeira = new Insignia { Nome = "Primeira", Ativa = true };
        var segunda = new Insignia { Nome = "Segunda", Ativa = true };

        dbContext.AddRange(aluno, primeira, segunda);
        await dbContext.SaveChangesAsync();

        var existente = new AlunoInsignia { AlunoId = aluno.Id, InsigniaId = primeira.Id };
        var alvo = new AlunoInsignia { AlunoId = aluno.Id, InsigniaId = segunda.Id };
        dbContext.AlunoInsignias.AddRange(existente, alvo);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.AtualizarAlunoInsigniaAsync(
            alvo.Id,
            new AlunoInsigniaFormViewModel
            {
                AlunoId = aluno.Id,
                InsigniaId = primeira.Id
            });

        Assert.False(resultado.Success);
        Assert.Equal(OperationResultStatus.ValidationError, resultado.Status);
        Assert.Equal(segunda.Id, (await dbContext.AlunoInsignias.FindAsync(alvo.Id))!.InsigniaId);
    }

    [Fact]
    public async Task ObterAulasAsync_AplicaFiltrosEPaginacaoPadrao()
    {
        await using var dbContext = CriarDbContext();
        var turmaAlvo = CriarTurma();
        var outraTurma = CriarTurma();
        dbContext.Turmas.AddRange(turmaAlvo, outraTurma);
        await dbContext.SaveChangesAsync();

        dbContext.Aulas.AddRange(Enumerable.Range(0, 21).Select(index => new Aula
        {
            TurmaId = turmaAlvo.Id,
            Inicio = DateTime.Today.AddDays(index).AddHours(19),
            Fim = DateTime.Today.AddDays(index).AddHours(20),
            Status = StatusAulaEnum.Agendada
        }));
        dbContext.Aulas.Add(new Aula
        {
            TurmaId = outraTurma.Id,
            Inicio = DateTime.Today.AddHours(18),
            Fim = DateTime.Today.AddHours(19),
            Status = StatusAulaEnum.Cancelada
        });
        await dbContext.SaveChangesAsync();

        var resultado = await CriarService(dbContext).ObterAulasAsync(new AulaAdminFilter
        {
            TurmaId = turmaAlvo.Id,
            Status = StatusAulaEnum.Agendada,
            Page = 2,
            PageSize = 20
        });

        Assert.Equal(21, resultado.Aulas.TotalCount);
        Assert.Equal(2, resultado.Aulas.Page);
        Assert.Single(resultado.Aulas);
        Assert.All(resultado.Aulas, x => Assert.Equal(turmaAlvo.Id, x.TurmaId));
    }

    [Fact]
    public async Task ObterFrequenciaAsync_DistingueAulasPreenchidas()
    {
        await using var dbContext = CriarDbContext();
        var turma = CriarTurma();
        var aluno = CriarAluno();
        var preenchida = new Aula
        {
            Turma = turma,
            Inicio = DateTime.Today.AddHours(18),
            Fim = DateTime.Today.AddHours(19)
        };
        var pendente = new Aula
        {
            Turma = turma,
            Inicio = DateTime.Today.AddHours(20),
            Fim = DateTime.Today.AddHours(21)
        };
        dbContext.AddRange(aluno, preenchida, pendente);
        await dbContext.SaveChangesAsync();
        dbContext.FrequenciasAlunos.Add(new FrequenciaAluno
        {
            AulaId = preenchida.Id,
            AlunoId = aluno.Id,
            Status = StatusFrequenciaEnum.Presente
        });
        await dbContext.SaveChangesAsync();

        var resultado = await CriarService(dbContext).ObterFrequenciaAsync(new FrequenciaAdminFilter
        {
            Preenchida = false
        });

        Assert.Single(resultado.Aulas);
        Assert.Equal(pendente.Id, resultado.Aulas[0].Id);
        Assert.True(resultado.Filtro.HasActiveFilters);
    }

    [Fact]
    public async Task ObterDocumentosAsync_FiltraPorStatusEEnvio()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var tipo = new DocumentoTipo { Nome = "RG", Ativo = true };
        var comEnvio = new DocumentoSolicitacao
        {
            Aluno = aluno,
            DocumentoTipo = tipo,
            Status = DocumentoStatusEnum.Enviado
        };
        var semEnvio = new DocumentoSolicitacao
        {
            Aluno = aluno,
            DocumentoTipo = tipo,
            Status = DocumentoStatusEnum.Solicitado
        };
        dbContext.AddRange(comEnvio, semEnvio);
        await dbContext.SaveChangesAsync();
        dbContext.DocumentoEnvios.Add(new DocumentoEnvio
        {
            DocumentoSolicitacaoId = comEnvio.Id,
            ArquivoUrl = "private/test.pdf",
            NomeArquivoOriginal = "test.pdf",
            TamanhoBytes = 10
        });
        await dbContext.SaveChangesAsync();

        var resultado = await CriarService(dbContext).ObterDocumentosAsync(new DocumentoAdminFilter
        {
            Status = DocumentoStatusEnum.Enviado,
            PossuiEnvio = true
        });

        Assert.Single(resultado.Solicitacoes);
        Assert.Equal(comEnvio.Id, resultado.Solicitacoes[0].SolicitacaoId);
    }

    [Fact]
    public async Task ObterComunicadosAsync_AplicaBuscaEImportancia()
    {
        await using var dbContext = CriarDbContext();
        dbContext.Comunicados.AddRange(
            new Comunicado { Titulo = "Treino especial", Conteudo = "Domingo", Importante = true },
            new Comunicado { Titulo = "Aviso comum", Conteudo = "Rotina", Importante = false });
        await dbContext.SaveChangesAsync();

        var resultado = await CriarService(dbContext).ObterComunicadosAsync(new ComunicadoAdminFilter
        {
            Busca = "Treino",
            Importante = true
        });

        Assert.Single(resultado.Comunicados);
        Assert.Equal("Treino especial", resultado.Comunicados[0].Titulo);
    }

    [Fact]
    public async Task ObterEventosAsync_FiltraTipoEProximos()
    {
        await using var dbContext = CriarDbContext();
        dbContext.EventosAlunoPortal.AddRange(
            new EventoAlunoPortal
            {
                Titulo = "Festival futuro",
                Inicio = new DateTime(2026, 7, 20, 10, 0, 0),
                Fim = new DateTime(2026, 7, 20, 12, 0, 0),
                Tipo = EventoAlunoTipoEnum.Apresentacao
            },
            new EventoAlunoPortal
            {
                Titulo = "Workshop encerrado",
                Inicio = new DateTime(2026, 6, 1, 10, 0, 0),
                Fim = new DateTime(2026, 6, 1, 12, 0, 0),
                Tipo = EventoAlunoTipoEnum.Exame
            });
        await dbContext.SaveChangesAsync();

        var resultado = await CriarService(dbContext).ObterEventosAsync(new EventoAdminFilter
        {
            Tipo = EventoAlunoTipoEnum.Apresentacao,
            Proximo = true
        });

        Assert.Single(resultado.Eventos);
        Assert.Equal("Festival futuro", resultado.Eventos[0].Titulo);
    }

    [Fact]
    public async Task ObterConquistasAsync_FiltraCategoriaEOrigem()
    {
        await using var dbContext = CriarDbContext();
        var aluno = CriarAluno();
        var frequencia = new Insignia { Nome = "Presença", Categoria = "Frequência" };
        var manual = new Insignia { Nome = "Especial", Categoria = "Eventos" };
        dbContext.AddRange(aluno, frequencia, manual);
        await dbContext.SaveChangesAsync();
        dbContext.AlunoInsignias.AddRange(
            new AlunoInsignia
            {
                AlunoId = aluno.Id,
                InsigniaId = frequencia.Id,
                Origem = InsigniaOrigemEnum.Automatica
            },
            new AlunoInsignia
            {
                AlunoId = aluno.Id,
                InsigniaId = manual.Id,
                Origem = InsigniaOrigemEnum.Manual
            });
        await dbContext.SaveChangesAsync();

        var resultado = await CriarService(dbContext).ObterConquistasAsync(new ConquistaAdminFilter
        {
            Categoria = "Frequência",
            Origem = InsigniaOrigemEnum.Automatica
        });

        Assert.Single(resultado.Conquistas);
        Assert.Equal(frequencia.Id, resultado.Conquistas[0].InsigniaId);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AreaAlunoAdminService CriarService(ApplicationDbContext dbContext)
    {
        var environment = new TestWebHostEnvironment();
        var clock = new TestClock();
        var currentUserService = new TestCurrentUserService();
        var auditLogger = new RecordingAuditLogger();
        var ruleEvaluator = new InsigniaRuleEvaluator(dbContext, clock);
        var privateFileStorageService = new LocalPrivateFileStorageService(environment);
        var aulasAdminService = new AreaAlunoAulasAdminService(
            dbContext,
            clock,
            auditLogger,
            currentUserService,
            ruleEvaluator);
        var documentoAdminService = new AreaAlunoDocumentoAdminService(
            dbContext,
            clock,
            privateFileStorageService,
            auditLogger,
            currentUserService);
        var comunicadoAdminService = new AreaAlunoComunicadoAdminService(
            dbContext,
            clock);
        var eventoAdminService = new AreaAlunoEventoAdminService(
            dbContext,
            clock);
        var conquistaAdminService = new AreaAlunoConquistaAdminService(
            dbContext,
            clock,
            ruleEvaluator);

        return new AreaAlunoAdminService(
            clock,
            currentUserService,
            aulasAdminService,
            documentoAdminService,
            comunicadoAdminService,
            eventoAdminService,
            conquistaAdminService);
    }

    private static Turma CriarTurma()
    {
        return new Turma
        {
            Nome = "Turma Teste",
            Modalidade = "Taiko",
            Ativa = true
        };
    }

    private static Aluno CriarAluno()
    {
        return new Aluno
        {
            NomeCompleto = "Aluno Teste",
            CPF = Guid.NewGuid().ToString("N")[..11],
            DataEntrada = DateOnly.FromDateTime(DateTime.Today),
            Status = StatusAlunoEnum.Ativo
        };
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "IkkonAdmin.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "ikkon-tests");
        public string EnvironmentName { get; set; } = "Testing";
        public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "ikkon-tests", "wwwroot");
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow { get; } = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);
        public DateTime Now { get; } = new(2026, 7, 13, 9, 0, 0, DateTimeKind.Local);
        public DateTime Today => Now.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public bool IsAuthenticated => true;
        public int? UserId => 1;
        public string? UserName => "admin.teste";
        public string? RemoteIpAddress => "127.0.0.1";
        public string? CorrelationId => "test-correlation-id";
        public bool IsInRole(string role) => role == IkkonAdmin.Web.Security.AppRoles.Admin;
        public bool HasClaim(string type, string value) => false;
        public string? FindFirstValue(string claimType) => null;
    }
}
