using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AreaAlunoAdminService(
    ApplicationDbContext dbContext,
    IWebHostEnvironment webHostEnvironment) : IAreaAlunoAdminService
{
    public async Task<AreaAlunoAdminDashboardViewModel> ObterDashboardAsync(CancellationToken cancellationToken = default)
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var proximoMes = inicioMes.AddMonths(1);
        var agoraUtc = DateTime.UtcNow;

        var proximasAulas = await ListarAulasAdminAsync(8, hoje, cancellationToken);
        var documentosRecentes = await ListarDocumentosAdminAsync(8, cancellationToken);
        var comunicadosRecentes = await ListarComunicadosAdminAsync(6, cancellationToken);

        return new AreaAlunoAdminDashboardViewModel
        {
            AulasProximas = await dbContext.Aulas.CountAsync(x => x.Inicio >= hoje && x.Status == StatusAulaEnum.Agendada, cancellationToken),
            FrequenciasRegistradasMes = await dbContext.FrequenciasAlunos.CountAsync(x => x.RegistradoEmUtc >= inicioMes && x.RegistradoEmUtc < proximoMes, cancellationToken),
            DocumentosPendentes = await dbContext.DocumentoSolicitacoes.CountAsync(x => x.Status != DocumentoStatusEnum.Aprovado, cancellationToken),
            ComunicadosAtivos = await dbContext.Comunicados.CountAsync(x => x.Ativo && x.PublicadoEmUtc <= agoraUtc && (!x.ExpiraEmUtc.HasValue || x.ExpiraEmUtc >= agoraUtc), cancellationToken),
            EventosProximos = await dbContext.EventosAlunoPortal.CountAsync(x => x.Ativo && x.Fim >= hoje, cancellationToken),
            ConquistasConcedidasMes = await dbContext.AlunoInsignias.CountAsync(x => x.ConcedidaEmUtc >= inicioMes && x.ConcedidaEmUtc < proximoMes, cancellationToken),
            ProximasAulas = proximasAulas,
            DocumentosRecentes = documentosRecentes,
            ComunicadosRecentes = comunicadosRecentes
        };
    }

    public async Task<AreaAlunoAulasAdminViewModel> ObterAulasAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoAulasAdminViewModel
        {
            Turmas = await ListarTurmasOpcoesAsync(cancellationToken),
            Instrutores = await ListarInstrutoresOpcoesAsync(cancellationToken),
            Horarios = await ListarHorariosAdminAsync(cancellationToken),
            TurmaInstrutores = await ListarInstrutoresTurmasAdminAsync(cancellationToken),
            Aulas = await ListarAulasAdminAsync(50, DateTime.Today.AddDays(-14), cancellationToken)
        };
    }

    public async Task<AreaAlunoOperacaoResult> CriarHorarioAsync(
        TurmaHorarioFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.HoraFim <= model.HoraInicio)
        {
            return AreaAlunoOperacaoResult.Falha("O horario final deve ser posterior ao inicial.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma nao encontrada.");
        }

        await dbContext.TurmaHorarios.AddAsync(new TurmaHorario
        {
            TurmaId = model.TurmaId,
            DiaSemana = model.DiaSemana,
            HoraInicio = model.HoraInicio,
            HoraFim = model.HoraFim,
            Local = LimparOpcional(model.Local),
            Ativo = true
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Horario cadastrado.");
    }

    public async Task<AreaAlunoOperacaoResult> VincularInstrutorAsync(
        TurmaInstrutorFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma nao encontrada.");
        }

        var instrutorValido = await dbContext.UsuariosSistema
            .AnyAsync(
                x => x.Id == model.UsuarioSistemaId &&
                     x.Ativo &&
                     x.TipoAcesso != TipoAcessoEnum.Aluno,
                cancellationToken);

        if (!instrutorValido)
        {
            return AreaAlunoOperacaoResult.Falha("Instrutor nao encontrado ou sem acesso interno.");
        }

        if (model.Principal)
        {
            var principais = await dbContext.TurmaInstrutores
                .Where(x => x.TurmaId == model.TurmaId && x.Principal && !x.DataFim.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var principal in principais)
            {
                principal.Principal = false;
            }
        }

        await dbContext.TurmaInstrutores.AddAsync(new TurmaInstrutor
        {
            TurmaId = model.TurmaId,
            UsuarioSistemaId = model.UsuarioSistemaId,
            Principal = model.Principal,
            DataInicio = model.DataInicio,
            DataFim = model.DataFim
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Instrutor vinculado.");
    }

    public async Task<AreaAlunoOperacaoResult> CriarAulaAsync(
        AulaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Fim <= model.Inicio)
        {
            return AreaAlunoOperacaoResult.Falha("O fim da aula deve ser posterior ao inicio.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma nao encontrada.");
        }

        if (model.TurmaHorarioId.HasValue)
        {
            var horarioValido = await dbContext.TurmaHorarios
                .AnyAsync(x => x.Id == model.TurmaHorarioId.Value && x.TurmaId == model.TurmaId, cancellationToken);

            if (!horarioValido)
            {
                return AreaAlunoOperacaoResult.Falha("Horario nao pertence a turma selecionada.");
            }
        }

        if (model.InstrutorUsuarioId.HasValue)
        {
            var instrutorValido = await dbContext.UsuariosSistema
                .AnyAsync(
                    x => x.Id == model.InstrutorUsuarioId.Value &&
                         x.Ativo &&
                         x.TipoAcesso != TipoAcessoEnum.Aluno,
                    cancellationToken);

            if (!instrutorValido)
            {
                return AreaAlunoOperacaoResult.Falha("Instrutor invalido.");
            }
        }

        await dbContext.Aulas.AddAsync(new Aula
        {
            TurmaId = model.TurmaId,
            TurmaHorarioId = model.TurmaHorarioId,
            InstrutorUsuarioId = model.InstrutorUsuarioId,
            Inicio = model.Inicio,
            Fim = model.Fim,
            Local = LimparOpcional(model.Local),
            Status = model.Status,
            Observacoes = LimparOpcional(model.Observacoes)
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Aula cadastrada.");
    }

    public async Task<AreaAlunoFrequenciaAdminViewModel> ObterFrequenciaAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoFrequenciaAdminViewModel
        {
            Aulas = await ListarAulasAdminAsync(80, DateTime.Today.AddMonths(-2), cancellationToken)
        };
    }

    public async Task<AreaAlunoRegistroFrequenciaViewModel?> ObterRegistroFrequenciaAsync(
        int aulaId,
        CancellationToken cancellationToken = default)
    {
        var aula = await dbContext.Aulas
            .AsNoTracking()
            .Include(x => x.Turma)
            .ThenInclude(x => x!.AlunoTurmas)
            .ThenInclude(x => x.Aluno)
            .Include(x => x.InstrutorUsuario)
            .Include(x => x.Frequencias)
            .FirstOrDefaultAsync(x => x.Id == aulaId, cancellationToken);

        if (aula is null || aula.Turma is null)
        {
            return null;
        }

        var frequencias = aula.Frequencias.ToDictionary(x => x.AlunoId);
        var alunos = aula.Turma.AlunoTurmas
            .Where(x => x.Aluno is not null && x.Aluno.Status != StatusAlunoEnum.Desligado)
            .OrderBy(x => x.Aluno!.NomeCompleto)
            .Select(x =>
            {
                frequencias.TryGetValue(x.AlunoId, out var frequencia);
                return new FrequenciaRegistroItemViewModel
                {
                    AlunoId = x.AlunoId,
                    AlunoNome = x.Aluno!.NomeCompleto,
                    Status = frequencia?.Status ?? StatusFrequenciaEnum.Presente,
                    Justificada = frequencia?.Justificada ?? false,
                    Justificativa = frequencia?.Justificativa
                };
            })
            .ToList();

        return new AreaAlunoRegistroFrequenciaViewModel
        {
            AulaId = aula.Id,
            Turma = aula.Turma.Nome,
            Inicio = aula.Inicio,
            Fim = aula.Fim,
            Instrutor = aula.InstrutorUsuario?.NomeExibicao,
            Alunos = alunos
        };
    }

    public async Task<AreaAlunoOperacaoResult> SalvarFrequenciaAsync(
        FrequenciaRegistroPostViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var aula = await dbContext.Aulas
            .Include(x => x.Turma)
            .ThenInclude(x => x!.AlunoTurmas)
            .Include(x => x.Frequencias)
            .FirstOrDefaultAsync(x => x.Id == model.AulaId, cancellationToken);

        if (aula is null || aula.Turma is null)
        {
            return AreaAlunoOperacaoResult.Falha("Aula nao encontrada.");
        }

        var alunosDaTurma = aula.Turma.AlunoTurmas.Select(x => x.AlunoId).ToHashSet();
        foreach (var item in model.Alunos.Where(x => alunosDaTurma.Contains(x.AlunoId)))
        {
            var frequencia = aula.Frequencias.FirstOrDefault(x => x.AlunoId == item.AlunoId);
            if (frequencia is null)
            {
                frequencia = new FrequenciaAluno
                {
                    AulaId = aula.Id,
                    AlunoId = item.AlunoId
                };

                dbContext.FrequenciasAlunos.Add(frequencia);
            }

            frequencia.Status = item.Status;
            frequencia.Justificada = item.Status == StatusFrequenciaEnum.FaltaJustificada || item.Justificada;
            frequencia.Justificativa = LimparOpcional(item.Justificativa);
            frequencia.RegistradoPorUsuarioId = usuarioId;
            frequencia.RegistradoEmUtc = DateTime.UtcNow;
        }

        if (aula.Status == StatusAulaEnum.Agendada)
        {
            aula.Status = StatusAulaEnum.Realizada;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Frequencia salva.");
    }

    public async Task<AreaAlunoDocumentosAdminViewModel> ObterDocumentosAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoDocumentosAdminViewModel
        {
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Tipos = await ListarDocumentoTiposAsync(cancellationToken),
            Solicitacoes = await ListarDocumentosAdminAsync(100, cancellationToken)
        };
    }

    public async Task<AreaAlunoOperacaoResult> CriarDocumentoTipoAsync(
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var nome = model.Nome.Trim();
        var existe = await dbContext.DocumentoTipos.AnyAsync(x => x.Nome == nome, cancellationToken);
        if (existe)
        {
            return AreaAlunoOperacaoResult.Falha("Ja existe um tipo de documento com este nome.");
        }

        await dbContext.DocumentoTipos.AddAsync(new DocumentoTipo
        {
            Nome = nome,
            Descricao = LimparOpcional(model.Descricao),
            Obrigatorio = model.Obrigatorio,
            Ativo = model.Ativo
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Tipo de documento criado.");
    }

    public async Task<AreaAlunoOperacaoResult> SolicitarDocumentoAsync(
        DocumentoSolicitacaoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var tipoExiste = await dbContext.DocumentoTipos.AnyAsync(x => x.Id == model.DocumentoTipoId && x.Ativo, cancellationToken);
        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);

        if (!tipoExiste || !alunoExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Tipo de documento ou aluno invalido.");
        }

        await dbContext.DocumentoSolicitacoes.AddAsync(new DocumentoSolicitacao
        {
            DocumentoTipoId = model.DocumentoTipoId,
            AlunoId = model.AlunoId,
            SolicitadoPorUsuarioId = usuarioId,
            Status = DocumentoStatusEnum.Solicitado,
            DataSolicitacaoUtc = DateTime.UtcNow,
            DataLimite = model.DataLimite,
            ObservacaoAdministrativa = LimparOpcional(model.ObservacaoAdministrativa)
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Documento solicitado.");
    }

    public async Task<AreaAlunoOperacaoResult> AvaliarDocumentoAsync(
        DocumentoAvaliacaoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == model.SolicitacaoId, cancellationToken);

        if (solicitacao is null)
        {
            return AreaAlunoOperacaoResult.Falha("Solicitacao nao encontrada.");
        }

        solicitacao.Status = model.Status;
        solicitacao.ObservacaoAdministrativa = LimparOpcional(model.ObservacaoAdministrativa);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AreaAlunoOperacaoResult.Ok("Documento atualizado.");
    }

    public async Task<AreaAlunoDocumentoDownload?> ObterDocumentoAdminDownloadAsync(
        int envioId,
        CancellationToken cancellationToken = default)
    {
        var envio = await dbContext.DocumentoEnvios
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == envioId, cancellationToken);

        if (envio is null)
        {
            return null;
        }

        var caminho = Path.Combine(ObterDocumentosPath(), envio.ArquivoUrl);
        return File.Exists(caminho)
            ? new AreaAlunoDocumentoDownload(caminho, envio.NomeArquivoOriginal, envio.ContentType)
            : null;
    }

    public async Task<AreaAlunoComunicadosAdminViewModel> ObterComunicadosAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoComunicadosAdminViewModel
        {
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Turmas = await ListarTurmasOpcoesAsync(cancellationToken),
            Comunicados = await ListarComunicadosAdminAsync(100, cancellationToken)
        };
    }

    public async Task<AreaAlunoOperacaoResult> CriarComunicadoAsync(
        ComunicadoFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alvoValidado = await ValidarAlvoAsync(model.AlvoTipo, model.AlunoId, model.TurmaId, cancellationToken);
        if (!alvoValidado.Sucesso)
        {
            return alvoValidado;
        }

        var comunicado = new Comunicado
        {
            Titulo = model.Titulo.Trim(),
            Conteudo = model.Conteudo.Trim(),
            Importante = model.Importante,
            Fixado = model.Fixado,
            PublicadoEmUtc = model.PublicadoEmUtc,
            ExpiraEmUtc = model.ExpiraEmUtc,
            Ativo = true,
            CriadoPorUsuarioId = usuarioId
        };

        comunicado.Alvos.Add(CriarComunicadoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));
        await dbContext.Comunicados.AddAsync(comunicado, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AreaAlunoOperacaoResult.Ok("Comunicado publicado.");
    }

    public async Task<AreaAlunoEventosAdminViewModel> ObterEventosAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoEventosAdminViewModel
        {
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Turmas = await ListarTurmasOpcoesAsync(cancellationToken),
            Eventos = await ListarEventosAdminAsync(100, cancellationToken)
        };
    }

    public async Task<AreaAlunoOperacaoResult> CriarEventoAsync(
        EventoAlunoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Fim <= model.Inicio)
        {
            return AreaAlunoOperacaoResult.Falha("O fim do evento deve ser posterior ao inicio.");
        }

        var alvoValidado = await ValidarAlvoAsync(model.AlvoTipo, model.AlunoId, model.TurmaId, cancellationToken);
        if (!alvoValidado.Sucesso)
        {
            return alvoValidado;
        }

        var evento = new EventoAlunoPortal
        {
            Titulo = model.Titulo.Trim(),
            Descricao = LimparOpcional(model.Descricao),
            Inicio = model.Inicio,
            Fim = model.Fim,
            Local = LimparOpcional(model.Local),
            Tipo = model.Tipo,
            Importante = model.Importante,
            Ativo = true,
            GoogleEventoId = LimparOpcional(model.GoogleEventoId)
        };

        evento.Alvos.Add(CriarEventoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));
        await dbContext.EventosAlunoPortal.AddAsync(evento, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return AreaAlunoOperacaoResult.Ok("Evento cadastrado.");
    }

    public async Task<AreaAlunoConquistasAdminViewModel> ObterConquistasAsync(CancellationToken cancellationToken = default)
    {
        return new AreaAlunoConquistasAdminViewModel
        {
            Alunos = await ListarAlunosOpcoesAsync(cancellationToken),
            Insignias = await ListarInsigniasAsync(cancellationToken),
            Conquistas = await ListarConquistasAdminAsync(100, cancellationToken)
        };
    }

    public async Task<AreaAlunoOperacaoResult> CriarInsigniaAsync(
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var nome = model.Nome.Trim();
        var existe = await dbContext.Insignias.AnyAsync(x => x.Nome == nome, cancellationToken);
        if (existe)
        {
            return AreaAlunoOperacaoResult.Falha("Ja existe uma insignia com este nome.");
        }

        await dbContext.Insignias.AddAsync(new Insignia
        {
            Nome = nome,
            Descricao = LimparOpcional(model.Descricao),
            Icone = LimparOpcional(model.Icone),
            Categoria = LimparOpcional(model.Categoria),
            RegraAutomatica = LimparOpcional(model.RegraAutomatica),
            Ativa = model.Ativa
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Insignia criada.");
    }

    public async Task<AreaAlunoOperacaoResult> AtribuirInsigniaAsync(
        AlunoInsigniaFormViewModel model,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);
        var insigniaExiste = await dbContext.Insignias.AnyAsync(x => x.Id == model.InsigniaId && x.Ativa, cancellationToken);

        if (!alunoExiste || !insigniaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Aluno ou insignia invalida.");
        }

        var jaPossui = await dbContext.AlunoInsignias
            .AnyAsync(x => x.AlunoId == model.AlunoId && x.InsigniaId == model.InsigniaId, cancellationToken);

        if (jaPossui)
        {
            return AreaAlunoOperacaoResult.Falha("Este aluno ja possui esta insignia.");
        }

        await dbContext.AlunoInsignias.AddAsync(new AlunoInsignia
        {
            AlunoId = model.AlunoId,
            InsigniaId = model.InsigniaId,
            Origem = InsigniaOrigemEnum.Manual,
            ConcedidaPorUsuarioId = usuarioId,
            ConcedidaEmUtc = DateTime.UtcNow,
            Observacao = LimparOpcional(model.Observacao)
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Insignia atribuida ao aluno.");
    }

    private async Task<IReadOnlyCollection<AreaAlunoOpcaoViewModel>> ListarAlunosOpcoesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Status != StatusAlunoEnum.Desligado)
            .OrderBy(x => x.NomeCompleto)
            .Select(x => new AreaAlunoOpcaoViewModel
            {
                Id = x.Id,
                Nome = x.NomeCompleto
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoOpcaoViewModel>> ListarTurmasOpcoesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Turmas
            .AsNoTracking()
            .Where(x => x.Ativa)
            .OrderBy(x => x.Nome)
            .Select(x => new AreaAlunoOpcaoViewModel
            {
                Id = x.Id,
                Nome = x.Nome
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoOpcaoViewModel>> ListarInstrutoresOpcoesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(x => x.Ativo && x.TipoAcesso != TipoAcessoEnum.Aluno)
            .OrderBy(x => x.NomeExibicao)
            .Select(x => new AreaAlunoOpcaoViewModel
            {
                Id = x.Id,
                Nome = x.NomeExibicao
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoHorarioAdminItemViewModel>> ListarHorariosAdminAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TurmaHorarios
            .AsNoTracking()
            .Include(x => x.Turma)
            .OrderBy(x => x.Turma!.Nome)
            .ThenBy(x => x.DiaSemana)
            .ThenBy(x => x.HoraInicio)
            .Select(x => new AreaAlunoHorarioAdminItemViewModel
            {
                Id = x.Id,
                Turma = x.Turma != null ? x.Turma.Nome : $"Turma #{x.TurmaId}",
                DiaSemana = x.DiaSemana,
                HoraInicio = x.HoraInicio,
                HoraFim = x.HoraFim,
                Local = x.Local,
                Ativo = x.Ativo
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoInstrutorAdminItemViewModel>> ListarInstrutoresTurmasAdminAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TurmaInstrutores
            .AsNoTracking()
            .Include(x => x.Turma)
            .Include(x => x.UsuarioSistema)
            .OrderBy(x => x.Turma!.Nome)
            .ThenByDescending(x => x.Principal)
            .Select(x => new AreaAlunoInstrutorAdminItemViewModel
            {
                Id = x.Id,
                Turma = x.Turma != null ? x.Turma.Nome : $"Turma #{x.TurmaId}",
                Instrutor = x.UsuarioSistema != null ? x.UsuarioSistema.NomeExibicao : $"Usuario #{x.UsuarioSistemaId}",
                Principal = x.Principal,
                DataInicio = x.DataInicio,
                DataFim = x.DataFim
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoAulaAdminItemViewModel>> ListarAulasAdminAsync(
        int limite,
        DateTime inicioMinimo,
        CancellationToken cancellationToken)
    {
        return await dbContext.Aulas
            .AsNoTracking()
            .Include(x => x.Turma)
            .ThenInclude(x => x!.AlunoTurmas)
            .Include(x => x.InstrutorUsuario)
            .Include(x => x.Frequencias)
            .Where(x => x.Inicio >= inicioMinimo)
            .OrderBy(x => x.Inicio)
            .Take(limite)
            .Select(x => new AreaAlunoAulaAdminItemViewModel
            {
                Id = x.Id,
                Turma = x.Turma != null ? x.Turma.Nome : $"Turma #{x.TurmaId}",
                Inicio = x.Inicio,
                Fim = x.Fim,
                Local = x.Local,
                Instrutor = x.InstrutorUsuario != null ? x.InstrutorUsuario.NomeExibicao : null,
                Status = x.Status,
                TotalAlunos = x.Turma != null ? x.Turma.AlunoTurmas.Count : 0,
                FrequenciasRegistradas = x.Frequencias.Count
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoDocumentoTipoItemViewModel>> ListarDocumentoTiposAsync(CancellationToken cancellationToken)
    {
        return await dbContext.DocumentoTipos
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new AreaAlunoDocumentoTipoItemViewModel
            {
                Id = x.Id,
                Nome = x.Nome,
                Descricao = x.Descricao,
                Obrigatorio = x.Obrigatorio,
                Ativo = x.Ativo
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoDocumentoAdminItemViewModel>> ListarDocumentosAdminAsync(
        int limite,
        CancellationToken cancellationToken)
    {
        var solicitacoes = await dbContext.DocumentoSolicitacoes
            .AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.DocumentoTipo)
            .Include(x => x.Envios)
            .OrderBy(x => x.Status == DocumentoStatusEnum.Aprovado)
            .ThenByDescending(x => x.DataSolicitacaoUtc)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return solicitacoes
            .Select(x =>
            {
                var ultimoEnvio = x.Envios.OrderByDescending(e => e.EnviadoEmUtc).FirstOrDefault();
                return new AreaAlunoDocumentoAdminItemViewModel
                {
                    SolicitacaoId = x.Id,
                    Aluno = x.Aluno?.NomeCompleto ?? $"Aluno #{x.AlunoId}",
                    Tipo = x.DocumentoTipo?.Nome ?? $"Documento #{x.DocumentoTipoId}",
                    Status = x.Status,
                    DataSolicitacaoUtc = x.DataSolicitacaoUtc,
                    DataLimite = x.DataLimite,
                    ObservacaoAdministrativa = x.ObservacaoAdministrativa,
                    Envios = x.Envios.Count,
                    UltimoEnvioId = ultimoEnvio?.Id,
                    NomeArquivoOriginal = ultimoEnvio?.NomeArquivoOriginal
                };
            })
            .ToList();
    }

    private async Task<IReadOnlyCollection<AreaAlunoComunicadoAdminItemViewModel>> ListarComunicadosAdminAsync(
        int limite,
        CancellationToken cancellationToken)
    {
        return await dbContext.Comunicados
            .AsNoTracking()
            .Include(x => x.Leituras)
            .OrderByDescending(x => x.Fixado)
            .ThenByDescending(x => x.PublicadoEmUtc)
            .Take(limite)
            .Select(x => new AreaAlunoComunicadoAdminItemViewModel
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Importante = x.Importante,
                Fixado = x.Fixado,
                Ativo = x.Ativo,
                PublicadoEmUtc = x.PublicadoEmUtc,
                ExpiraEmUtc = x.ExpiraEmUtc,
                Leituras = x.Leituras.Count
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoEventoAdminItemViewModel>> ListarEventosAdminAsync(
        int limite,
        CancellationToken cancellationToken)
    {
        return await dbContext.EventosAlunoPortal
            .AsNoTracking()
            .OrderBy(x => x.Inicio)
            .Take(limite)
            .Select(x => new AreaAlunoEventoAdminItemViewModel
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Inicio = x.Inicio,
                Fim = x.Fim,
                Local = x.Local,
                Tipo = x.Tipo,
                Importante = x.Importante,
                Ativo = x.Ativo
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoInsigniaItemViewModel>> ListarInsigniasAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Insignias
            .AsNoTracking()
            .OrderBy(x => x.Nome)
            .Select(x => new AreaAlunoInsigniaItemViewModel
            {
                Id = x.Id,
                Nome = x.Nome,
                Categoria = x.Categoria,
                Ativa = x.Ativa
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<AreaAlunoConquistaAdminItemViewModel>> ListarConquistasAdminAsync(
        int limite,
        CancellationToken cancellationToken)
    {
        return await dbContext.AlunoInsignias
            .AsNoTracking()
            .Include(x => x.Aluno)
            .Include(x => x.Insignia)
            .OrderByDescending(x => x.ConcedidaEmUtc)
            .Take(limite)
            .Select(x => new AreaAlunoConquistaAdminItemViewModel
            {
                Id = x.Id,
                Aluno = x.Aluno != null ? x.Aluno.NomeCompleto : $"Aluno #{x.AlunoId}",
                Insignia = x.Insignia != null ? x.Insignia.Nome : $"Insignia #{x.InsigniaId}",
                ConcedidaEmUtc = x.ConcedidaEmUtc,
                Origem = x.Origem
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<AreaAlunoOperacaoResult> ValidarAlvoAsync(
        ComunicadoAlvoTipoEnum alvoTipo,
        int? alunoId,
        int? turmaId,
        CancellationToken cancellationToken)
    {
        if (alvoTipo == ComunicadoAlvoTipoEnum.Todos)
        {
            return AreaAlunoOperacaoResult.Ok("Alvo valido.");
        }

        if (alvoTipo == ComunicadoAlvoTipoEnum.Aluno)
        {
            var alunoExiste = alunoId.HasValue &&
                              await dbContext.Alunos.AnyAsync(x => x.Id == alunoId.Value, cancellationToken);

            return alunoExiste
                ? AreaAlunoOperacaoResult.Ok("Alvo valido.")
                : AreaAlunoOperacaoResult.Falha("Selecione um aluno valido.");
        }

        var turmaExiste = turmaId.HasValue &&
                          await dbContext.Turmas.AnyAsync(x => x.Id == turmaId.Value, cancellationToken);

        return turmaExiste
            ? AreaAlunoOperacaoResult.Ok("Alvo valido.")
            : AreaAlunoOperacaoResult.Falha("Selecione uma turma valida.");
    }

    private static ComunicadoAlvo CriarComunicadoAlvo(
        ComunicadoAlvoTipoEnum alvoTipo,
        int? alunoId,
        int? turmaId)
    {
        return new ComunicadoAlvo
        {
            Todos = alvoTipo == ComunicadoAlvoTipoEnum.Todos,
            AlunoId = alvoTipo == ComunicadoAlvoTipoEnum.Aluno ? alunoId : null,
            TurmaId = alvoTipo == ComunicadoAlvoTipoEnum.Turma ? turmaId : null
        };
    }

    private static EventoAlunoPortalAlvo CriarEventoAlvo(
        ComunicadoAlvoTipoEnum alvoTipo,
        int? alunoId,
        int? turmaId)
    {
        return new EventoAlunoPortalAlvo
        {
            Todos = alvoTipo == ComunicadoAlvoTipoEnum.Todos,
            AlunoId = alvoTipo == ComunicadoAlvoTipoEnum.Aluno ? alunoId : null,
            TurmaId = alvoTipo == ComunicadoAlvoTipoEnum.Turma ? turmaId : null
        };
    }

    private string ObterDocumentosPath()
    {
        return Path.Combine(webHostEnvironment.ContentRootPath, "App_Data", "uploads", "documentos");
    }

    private static string? LimparOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
