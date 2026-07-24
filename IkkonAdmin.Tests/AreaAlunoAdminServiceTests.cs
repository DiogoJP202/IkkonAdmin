using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Infrastructure.Operations;
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
        var fileStorageService = new LocalFileStorageService(environment);
        var aulasAdminService = new AreaAlunoAulasAdminService(
            dbContext,
            clock);
        var documentoAdminService = new AreaAlunoDocumentoAdminService(
            dbContext,
            clock,
            fileStorageService);
        var comunicadoAdminService = new AreaAlunoComunicadoAdminService(
            dbContext,
            clock);
        var eventoAdminService = new AreaAlunoEventoAdminService(
            dbContext,
            clock);
        var conquistaAdminService = new AreaAlunoConquistaAdminService(
            dbContext,
            clock);

        return new AreaAlunoAdminService(
            clock,
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
}
