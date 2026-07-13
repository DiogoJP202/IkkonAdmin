using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AreaAlunoService(
    ApplicationDbContext dbContext,
    IWebHostEnvironment webHostEnvironment) : IAreaAlunoService
{
    private static readonly HashSet<string> DocumentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024;

    public async Task<AreaAlunoDashboardViewModel?> ObterDashboardAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        var perfil = await ObterPerfilBaseAsync(contexto.AlunoId, cancellationToken);
        if (perfil is null)
        {
            return null;
        }

        var mensalidades = await ListarMensalidadesAsync(contexto.AlunoId, 6, cancellationToken);
        var turmas = await ListarTurmasAsync(contexto.AlunoId, cancellationToken);
        var resumoFinanceiro = await ObterResumoFinanceiroAsync(contexto.AlunoId, cancellationToken);
        var proximasAulas = await ListarProximasAulasAsync(contexto.TurmaIds, 5, cancellationToken);
        var eventos = await ListarEventosAsync(contexto.AlunoId, contexto.TurmaIds, 5, cancellationToken);
        var documentos = await ListarDocumentosAsync(contexto.AlunoId, 5, cancellationToken);
        var comunicados = await ListarComunicadosAsync(contexto.AlunoId, contexto.TurmaIds, 5, cancellationToken);
        var frequenciaResumo = await ObterResumoFrequenciaAsync(contexto.AlunoId, cancellationToken);
        var faltasRecentes = await ListarFaltasRecentesAsync(contexto.AlunoId, 5, cancellationToken);
        var conquistasRecentes = await ListarConquistasAsync(contexto.AlunoId, 4, cancellationToken);

        var alertas = MontarAlertas(
            resumoFinanceiro.MensalidadesAtrasadas,
            resumoFinanceiro.TotalEmAberto,
            documentos,
            comunicados,
            proximasAulas,
            eventos);

        return new AreaAlunoDashboardViewModel
        {
            AlunoId = contexto.AlunoId,
            NomeCompleto = perfil.NomeCompleto,
            Email = perfil.Email,
            Celular = perfil.Celular,
            FotoPerfilUrl = contexto.FotoPerfilUrl,
            Status = perfil.Status,
            TurmaPrincipal = perfil.TurmaPrincipal,
            DataEntrada = perfil.DataEntrada,
            TotalEmAberto = resumoFinanceiro.TotalEmAberto,
            MensalidadesAtrasadas = resumoFinanceiro.MensalidadesAtrasadas,
            DocumentosPendentes = documentos.Count(x => x.Status is DocumentoStatusEnum.Solicitado or DocumentoStatusEnum.Pendente or DocumentoStatusEnum.Recusado),
            ComunicadosNaoLidos = comunicados.Count(x => !x.Lido),
            FaltasNaoJustificadas = frequenciaResumo.FaltasNaoJustificadas,
            PercentualPresenca = frequenciaResumo.PercentualPresenca,
            Turmas = turmas,
            MensalidadesRecentes = mensalidades,
            ProximasAulas = proximasAulas,
            ProximosEventos = eventos,
            DocumentosRecentes = documentos,
            ComunicadosRecentes = comunicados,
            FaltasRecentes = faltasRecentes,
            ConquistasRecentes = conquistasRecentes,
            Alertas = alertas
        };
    }

    public async Task<AreaAlunoPerfilViewModel?> ObterPerfilAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        return await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId.Value)
            .Select(x => new AreaAlunoPerfilViewModel
            {
                NomeCompleto = x.NomeCompleto,
                CPF = x.CPF,
                RG = x.RG,
                DataNascimento = x.DataNascimento,
                Email = x.Email,
                Celular = x.Celular,
                Endereco = x.Endereco,
                ContatoEmergencia = x.ContatoEmergencia,
                DataEntrada = x.DataEntrada,
                Status = x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AreaAlunoFinanceiroViewModel?> ObterFinanceiroAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var mensalidades = await ListarMensalidadesAsync(alunoId.Value, 36, cancellationToken);
        var resumoFinanceiro = await ObterResumoFinanceiroAsync(alunoId.Value, cancellationToken);
        var totalPago = await dbContext.Pagamentos
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId.Value)
            .SumAsync(x => (decimal?)x.ValorPago, cancellationToken) ?? 0m;

        return new AreaAlunoFinanceiroViewModel
        {
            TotalPago = totalPago,
            TotalEmAberto = resumoFinanceiro.TotalEmAberto,
            MensalidadesAtrasadas = resumoFinanceiro.MensalidadesAtrasadas,
            Mensalidades = mensalidades
        };
    }

    public async Task<AreaAlunoTurmasViewModel?> ObterTurmasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        var turmaPrincipal = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == contexto.AlunoId)
            .Select(x => x.Turma != null ? x.Turma.Nome : null)
            .FirstOrDefaultAsync(cancellationToken);

        return new AreaAlunoTurmasViewModel
        {
            TurmaPrincipal = turmaPrincipal,
            Turmas = await ListarTurmasAsync(contexto.AlunoId, cancellationToken),
            ProximasAulas = await ListarProximasAulasAsync(contexto.TurmaIds, 12, cancellationToken)
        };
    }

    public async Task<AreaAlunoAulasViewModel?> ObterAulasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        return new AreaAlunoAulasViewModel
        {
            Turmas = await ListarTurmasAsync(contexto.AlunoId, cancellationToken),
            ProximasAulas = await ListarProximasAulasAsync(contexto.TurmaIds, 30, cancellationToken)
        };
    }

    public async Task<AreaAlunoFrequenciaViewModel?> ObterFrequenciaAsync(
        int usuarioId,
        DateOnly? inicio,
        DateOnly? fim,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var dataFim = fim ?? DateOnly.FromDateTime(DateTime.Today);
        var dataInicio = inicio ?? dataFim.AddMonths(-6);
        if (dataInicio > dataFim)
        {
            (dataInicio, dataFim) = (dataFim, dataInicio);
        }

        var inicioDateTime = dataInicio.ToDateTime(TimeOnly.MinValue);
        var fimDateTime = dataFim.ToDateTime(TimeOnly.MaxValue);

        var registros = await dbContext.FrequenciasAlunos
            .AsNoTracking()
            .Include(x => x.Aula)
            .ThenInclude(x => x!.Turma)
            .Include(x => x.Aula)
            .ThenInclude(x => x!.InstrutorUsuario)
            .Where(x => x.AlunoId == alunoId.Value &&
                        x.Aula != null &&
                        x.Aula.Inicio >= inicioDateTime &&
                        x.Aula.Inicio <= fimDateTime)
            .OrderByDescending(x => x.Aula!.Inicio)
            .ToListAsync(cancellationToken);

        var itens = registros
            .Select(MapearFrequencia)
            .ToList();

        var contabilizados = itens
            .Where(x => x.Status != StatusFrequenciaEnum.Cancelada)
            .ToList();

        var presencas = contabilizados.Count(x => x.Status == StatusFrequenciaEnum.Presente);
        var faltasJustificadas = contabilizados.Count(x => x.Status == StatusFrequenciaEnum.FaltaJustificada || x.Justificada);
        var faltasNaoJustificadas = contabilizados.Count(x => x.Status == StatusFrequenciaEnum.Falta && !x.Justificada);

        return new AreaAlunoFrequenciaViewModel
        {
            Inicio = dataInicio,
            Fim = dataFim,
            TotalRegistros = contabilizados.Count,
            Presencas = presencas,
            FaltasJustificadas = faltasJustificadas,
            FaltasNaoJustificadas = faltasNaoJustificadas,
            PercentualPresenca = CalcularPercentualPresenca(contabilizados.Count, presencas),
            Registros = itens
        };
    }

    public async Task<AreaAlunoEventosViewModel?> ObterEventosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        return new AreaAlunoEventosViewModel
        {
            Eventos = await ListarEventosAsync(contexto.AlunoId, contexto.TurmaIds, 100, cancellationToken)
        };
    }

    public async Task<AreaAlunoDocumentosViewModel?> ObterDocumentosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        return new AreaAlunoDocumentosViewModel
        {
            Documentos = await ListarDocumentosAsync(alunoId.Value, 100, cancellationToken)
        };
    }

    public async Task<AreaAlunoOperacaoResult> EnviarDocumentoAsync(
        int usuarioId,
        int solicitacaoId,
        IFormFile arquivo,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return AreaAlunoOperacaoResult.Falha("Conta de aluno não encontrada.");
        }

        if (arquivo.Length <= 0)
        {
            return AreaAlunoOperacaoResult.Falha("Selecione um arquivo para envio.");
        }

        if (arquivo.Length > MaxDocumentSizeBytes)
        {
            return AreaAlunoOperacaoResult.Falha("O arquivo deve ter no máximo 10 MB.");
        }

        var extension = Path.GetExtension(arquivo.FileName ?? string.Empty);
        if (!DocumentExtensions.Contains(extension))
        {
            return AreaAlunoOperacaoResult.Falha("Formato inválido. Use PDF, JPG, PNG ou WEBP.");
        }

        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == solicitacaoId && x.AlunoId == alunoId.Value, cancellationToken);

        if (solicitacao is null)
        {
            return AreaAlunoOperacaoResult.Falha("Solicitação de documento não encontrada.");
        }

        if (solicitacao.Status == DocumentoStatusEnum.Aprovado)
        {
            return AreaAlunoOperacaoResult.Falha("Este documento já foi aprovado.");
        }

        var uploadsPath = ObterDocumentosPath();
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"{alunoId.Value}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await arquivo.CopyToAsync(stream, cancellationToken);
        }

        dbContext.DocumentoEnvios.Add(new DocumentoEnvio
        {
            DocumentoSolicitacaoId = solicitacao.Id,
            ArquivoUrl = fileName,
            NomeArquivoOriginal = string.IsNullOrWhiteSpace(arquivo.FileName)
                ? fileName
                : Path.GetFileName(arquivo.FileName),
            ContentType = arquivo.ContentType,
            TamanhoBytes = arquivo.Length,
            EnviadoEmUtc = DateTime.UtcNow,
            EnviadoPorUsuarioId = usuarioId
        });

        solicitacao.Status = DocumentoStatusEnum.Enviado;
        await dbContext.SaveChangesAsync(cancellationToken);

        return AreaAlunoOperacaoResult.Ok("Documento enviado para analise.");
    }

    public async Task<AreaAlunoDocumentoDownload?> ObterDocumentoParaDownloadAsync(
        int usuarioId,
        int envioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var envio = await dbContext.DocumentoEnvios
            .AsNoTracking()
            .Include(x => x.DocumentoSolicitacao)
            .FirstOrDefaultAsync(
                x => x.Id == envioId &&
                     x.DocumentoSolicitacao != null &&
                     x.DocumentoSolicitacao.AlunoId == alunoId.Value,
                cancellationToken);

        if (envio is null)
        {
            return null;
        }

        var caminho = Path.Combine(ObterDocumentosPath(), envio.ArquivoUrl);
        return File.Exists(caminho)
            ? new AreaAlunoDocumentoDownload(caminho, envio.NomeArquivoOriginal, envio.ContentType)
            : null;
    }

    public async Task<AreaAlunoComunicadosViewModel?> ObterComunicadosAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return null;
        }

        return new AreaAlunoComunicadosViewModel
        {
            Comunicados = await ListarComunicadosAsync(contexto.AlunoId, contexto.TurmaIds, 100, cancellationToken)
        };
    }

    public async Task<bool> MarcarComunicadoComoLidoAsync(
        int usuarioId,
        int comunicadoId,
        CancellationToken cancellationToken = default)
    {
        var contexto = await ObterContextoAsync(usuarioId, cancellationToken);
        if (contexto is null)
        {
            return false;
        }

        var podeLer = await ComunicadoPertenceAoAlunoAsync(
            comunicadoId,
            contexto.AlunoId,
            contexto.TurmaIds,
            cancellationToken);

        if (!podeLer)
        {
            return false;
        }

        var jaLido = await dbContext.ComunicadosLeituras
            .AnyAsync(x => x.ComunicadoId == comunicadoId && x.AlunoId == contexto.AlunoId, cancellationToken);

        if (!jaLido)
        {
            dbContext.ComunicadosLeituras.Add(new ComunicadoLeitura
            {
                ComunicadoId = comunicadoId,
                AlunoId = contexto.AlunoId,
                LidoEmUtc = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    public async Task<AreaAlunoConquistasViewModel?> ObterConquistasAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        await GarantirConquistasAutomaticasAsync(alunoId.Value, cancellationToken);

        return new AreaAlunoConquistasViewModel
        {
            Conquistas = await ListarConquistasAsync(alunoId.Value, 100, cancellationToken)
        };
    }

    private async Task<AlunoPortalContexto?> ObterContextoAsync(int usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(x => x.Id == usuarioId && x.Ativo && x.TipoAcesso == TipoAcessoEnum.Aluno)
            .Select(x => new
            {
                x.AlunoId,
                x.FotoPerfilUrl
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (usuario?.AlunoId is not int alunoId)
        {
            return null;
        }

        var turmaIds = await dbContext.AlunosTurmas
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId)
            .Select(x => x.TurmaId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var turmaPrincipalId = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId && x.TurmaId.HasValue)
            .Select(x => x.TurmaId!.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (turmaPrincipalId > 0 && !turmaIds.Contains(turmaPrincipalId))
        {
            turmaIds.Add(turmaPrincipalId);
        }

        return new AlunoPortalContexto(alunoId, turmaIds, usuario.FotoPerfilUrl);
    }

    private async Task<int?> ObterAlunoIdVinculadoAsync(int usuarioId, CancellationToken cancellationToken)
    {
        return await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(x => x.Id == usuarioId && x.Ativo && x.TipoAcesso == TipoAcessoEnum.Aluno)
            .Select(x => x.AlunoId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<PerfilBase?> ObterPerfilBaseAsync(int alunoId, CancellationToken cancellationToken)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId)
            .Select(x => new PerfilBase
            {
                AlunoId = x.Id,
                NomeCompleto = x.NomeCompleto,
                Email = x.Email,
                Celular = x.Celular,
                Status = x.Status,
                TurmaPrincipal = x.Turma != null ? x.Turma.Nome : null,
                DataEntrada = x.DataEntrada
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<List<AreaAlunoMensalidadeItemViewModel>> ListarMensalidadesAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken)
    {
        var mensalidades = await dbContext.Mensalidades
            .AsNoTracking()
            .Include(x => x.Pagamentos)
            .Where(x => x.AlunoId == alunoId)
            .OrderByDescending(x => x.Competencia)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return mensalidades
            .Select(x =>
            {
                var ultimoPagamento = x.Pagamentos
                    .OrderByDescending(p => p.DataPagamento)
                    .FirstOrDefault();

                return new AreaAlunoMensalidadeItemViewModel
                {
                    Id = x.Id,
                    Competencia = x.Competencia,
                    DataVencimento = x.DataVencimento,
                    ValorFinal = x.ValorFinal,
                    Status = x.Status,
                    DataPagamento = x.DataPagamento,
                    FormaPagamento = ultimoPagamento?.FormaPagamento,
                    Comprovante = ultimoPagamento?.Comprovante
                };
            })
            .ToList();
    }

    private async Task<(decimal TotalEmAberto, int MensalidadesAtrasadas)> ObterResumoFinanceiroAsync(
        int alunoId,
        CancellationToken cancellationToken)
    {
        var totalEmAberto = await dbContext.Mensalidades
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId &&
                        (x.Status == StatusMensalidadeEnum.Pendente || x.Status == StatusMensalidadeEnum.Atrasado))
            .SumAsync(x => (decimal?)x.ValorFinal, cancellationToken) ?? 0m;

        var mensalidadesAtrasadas = await dbContext.Mensalidades
            .AsNoTracking()
            .CountAsync(x => x.AlunoId == alunoId && x.Status == StatusMensalidadeEnum.Atrasado, cancellationToken);

        return (totalEmAberto, mensalidadesAtrasadas);
    }

    private async Task<List<AreaAlunoTurmaItemViewModel>> ListarTurmasAsync(
        int alunoId,
        CancellationToken cancellationToken)
    {
        var vinculos = await dbContext.AlunosTurmas
            .AsNoTracking()
            .Include(x => x.Turma)
            .ThenInclude(x => x!.Horarios)
            .Include(x => x.Turma)
            .ThenInclude(x => x!.Instrutores)
            .ThenInclude(x => x.UsuarioSistema)
            .Where(x => x.AlunoId == alunoId && x.Turma != null)
            .OrderBy(x => x.Turma!.Nome)
            .ToListAsync(cancellationToken);

        return vinculos
            .Select(x =>
            {
                var instrutor = x.Turma!.Instrutores
                    .Where(i => !i.DataFim.HasValue || i.DataFim.Value >= DateOnly.FromDateTime(DateTime.Today))
                    .OrderByDescending(i => i.Principal)
                    .ThenBy(i => i.DataInicio)
                    .Select(i => i.UsuarioSistema!.NomeExibicao)
                    .FirstOrDefault();

                var horarios = x.Turma.Horarios
                    .Where(h => h.Ativo)
                    .OrderBy(h => h.DiaSemana)
                    .ThenBy(h => h.HoraInicio)
                    .Select(h => new AreaAlunoHorarioItemViewModel
                    {
                        DiaSemana = h.DiaSemana,
                        HoraInicio = h.HoraInicio,
                        HoraFim = h.HoraFim,
                        Local = h.Local
                    })
                    .ToList();

                return new AreaAlunoTurmaItemViewModel
                {
                    Nome = x.Turma.Nome,
                    Modalidade = x.Turma.Modalidade,
                    Horario = x.Turma.Horario,
                    Local = horarios.FirstOrDefault()?.Local,
                    Instrutor = instrutor,
                    Horarios = horarios,
                    DataVinculo = x.DataVinculo
                };
            })
            .ToList();
    }

    private async Task<List<AreaAlunoAulaItemViewModel>> ListarProximasAulasAsync(
        IReadOnlyCollection<int> turmaIds,
        int limite,
        CancellationToken cancellationToken)
    {
        if (turmaIds.Count == 0)
        {
            return [];
        }

        var agora = DateTime.Now;
        return await dbContext.Aulas
            .AsNoTracking()
            .Include(x => x.Turma)
            .Include(x => x.InstrutorUsuario)
            .Where(x => turmaIds.Contains(x.TurmaId) &&
                        x.Status != StatusAulaEnum.Cancelada &&
                        x.Fim >= agora)
            .OrderBy(x => x.Inicio)
            .Take(limite)
            .Select(x => new AreaAlunoAulaItemViewModel
            {
                Id = x.Id,
                Turma = x.Turma != null ? x.Turma.Nome : $"Turma #{x.TurmaId}",
                Modalidade = x.Turma != null ? x.Turma.Modalidade : string.Empty,
                Inicio = x.Inicio,
                Fim = x.Fim,
                Local = x.Local,
                Instrutor = x.InstrutorUsuario != null ? x.InstrutorUsuario.NomeExibicao : null,
                Status = x.Status
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<AreaAlunoEventoItemViewModel>> ListarEventosAsync(
        int alunoId,
        IReadOnlyCollection<int> turmaIds,
        int limite,
        CancellationToken cancellationToken)
    {
        var agora = DateTime.Now;
        return await dbContext.EventosAlunoPortal
            .AsNoTracking()
            .Where(x => x.Ativo &&
                        x.Fim >= agora &&
                        x.Alvos.Any(a =>
                            a.Todos ||
                            a.AlunoId == alunoId ||
                            (a.TurmaId.HasValue && turmaIds.Contains(a.TurmaId.Value))))
            .OrderByDescending(x => x.Importante)
            .ThenBy(x => x.Inicio)
            .Take(limite)
            .Select(x => new AreaAlunoEventoItemViewModel
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Descricao = x.Descricao,
                Inicio = x.Inicio,
                Fim = x.Fim,
                Local = x.Local,
                Tipo = x.Tipo,
                Importante = x.Importante
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<AreaAlunoDocumentoItemViewModel>> ListarDocumentosAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken)
    {
        var solicitacoes = await dbContext.DocumentoSolicitacoes
            .AsNoTracking()
            .Include(x => x.DocumentoTipo)
            .Include(x => x.Envios)
            .Where(x => x.AlunoId == alunoId)
            .OrderBy(x => x.Status == DocumentoStatusEnum.Aprovado)
            .ThenByDescending(x => x.DataSolicitacaoUtc)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return solicitacoes
            .Select(x =>
            {
                var ultimoEnvio = x.Envios
                    .OrderByDescending(e => e.EnviadoEmUtc)
                    .FirstOrDefault();

                return new AreaAlunoDocumentoItemViewModel
                {
                    SolicitacaoId = x.Id,
                    Tipo = x.DocumentoTipo?.Nome ?? $"Documento #{x.DocumentoTipoId}",
                    Descricao = x.DocumentoTipo?.Descricao,
                    Status = x.Status,
                    DataSolicitacaoUtc = x.DataSolicitacaoUtc,
                    DataLimite = x.DataLimite,
                    ObservacaoAdministrativa = x.ObservacaoAdministrativa,
                    UltimoEnvioId = ultimoEnvio?.Id,
                    NomeArquivoOriginal = ultimoEnvio?.NomeArquivoOriginal,
                    EnviadoEmUtc = ultimoEnvio?.EnviadoEmUtc
                };
            })
            .ToList();
    }

    private async Task<List<AreaAlunoComunicadoItemViewModel>> ListarComunicadosAsync(
        int alunoId,
        IReadOnlyCollection<int> turmaIds,
        int limite,
        CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        return await dbContext.Comunicados
            .AsNoTracking()
            .Where(x => x.Ativo &&
                        x.PublicadoEmUtc <= agora &&
                        (!x.ExpiraEmUtc.HasValue || x.ExpiraEmUtc.Value >= agora) &&
                        x.Alvos.Any(a =>
                            a.Todos ||
                            a.AlunoId == alunoId ||
                            (a.TurmaId.HasValue && turmaIds.Contains(a.TurmaId.Value))))
            .OrderByDescending(x => x.Fixado)
            .ThenByDescending(x => x.Importante)
            .ThenByDescending(x => x.PublicadoEmUtc)
            .Take(limite)
            .Select(x => new AreaAlunoComunicadoItemViewModel
            {
                Id = x.Id,
                Titulo = x.Titulo,
                Conteudo = x.Conteudo,
                Importante = x.Importante,
                Fixado = x.Fixado,
                PublicadoEmUtc = x.PublicadoEmUtc,
                ExpiraEmUtc = x.ExpiraEmUtc,
                Lido = x.Leituras.Any(l => l.AlunoId == alunoId)
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<bool> ComunicadoPertenceAoAlunoAsync(
        int comunicadoId,
        int alunoId,
        IReadOnlyCollection<int> turmaIds,
        CancellationToken cancellationToken)
    {
        var agora = DateTime.UtcNow;
        return await dbContext.Comunicados
            .AsNoTracking()
            .AnyAsync(x => x.Id == comunicadoId &&
                           x.Ativo &&
                           x.PublicadoEmUtc <= agora &&
                           (!x.ExpiraEmUtc.HasValue || x.ExpiraEmUtc.Value >= agora) &&
                           x.Alvos.Any(a =>
                               a.Todos ||
                               a.AlunoId == alunoId ||
                               (a.TurmaId.HasValue && turmaIds.Contains(a.TurmaId.Value))),
                cancellationToken);
    }

    private async Task<FrequenciaResumo> ObterResumoFrequenciaAsync(
        int alunoId,
        CancellationToken cancellationToken)
    {
        var inicio = DateTime.Today.AddMonths(-6);
        var registros = await dbContext.FrequenciasAlunos
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId &&
                        x.Status != StatusFrequenciaEnum.Cancelada &&
                        x.Aula != null &&
                        x.Aula.Inicio >= inicio)
            .Select(x => new { x.Status, x.Justificada })
            .ToListAsync(cancellationToken);

        var presencas = registros.Count(x => x.Status == StatusFrequenciaEnum.Presente);
        var faltasNaoJustificadas = registros.Count(x => x.Status == StatusFrequenciaEnum.Falta && !x.Justificada);

        return new FrequenciaResumo(
            registros.Count,
            presencas,
            faltasNaoJustificadas,
            CalcularPercentualPresenca(registros.Count, presencas));
    }

    private async Task<List<AreaAlunoFrequenciaItemViewModel>> ListarFaltasRecentesAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken)
    {
        var faltas = await dbContext.FrequenciasAlunos
            .AsNoTracking()
            .Include(x => x.Aula)
            .ThenInclude(x => x!.Turma)
            .Include(x => x.Aula)
            .ThenInclude(x => x!.InstrutorUsuario)
            .Where(x => x.AlunoId == alunoId &&
                        (x.Status == StatusFrequenciaEnum.Falta ||
                         x.Status == StatusFrequenciaEnum.FaltaJustificada))
            .OrderByDescending(x => x.Aula!.Inicio)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return faltas.Select(MapearFrequencia).ToList();
    }

    private async Task<List<AreaAlunoConquistaItemViewModel>> ListarConquistasAsync(
        int alunoId,
        int limite,
        CancellationToken cancellationToken)
    {
        return await dbContext.AlunoInsignias
            .AsNoTracking()
            .Include(x => x.Insignia)
            .Where(x => x.AlunoId == alunoId && x.Insignia != null && x.Insignia.Ativa)
            .OrderByDescending(x => x.ConcedidaEmUtc)
            .Take(limite)
            .Select(x => new AreaAlunoConquistaItemViewModel
            {
                Id = x.Id,
                Nome = x.Insignia!.Nome,
                Descricao = x.Insignia.Descricao,
                Icone = x.Insignia.Icone,
                Categoria = x.Insignia.Categoria,
                ConcedidaEmUtc = x.ConcedidaEmUtc,
                Origem = x.Origem,
                Observacao = x.Observacao
            })
            .ToListAsync(cancellationToken);
    }

    private async Task GarantirConquistasAutomaticasAsync(
        int alunoId,
        CancellationToken cancellationToken)
    {
        var aluno = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId)
            .Select(x => new { x.Id, x.DataEntrada })
            .FirstOrDefaultAsync(cancellationToken);

        if (aluno is null)
        {
            return;
        }

        var anos = DateOnly.FromDateTime(DateTime.Today).Year - aluno.DataEntrada.Year;
        if (DateOnly.FromDateTime(DateTime.Today) < aluno.DataEntrada.AddYears(Math.Max(anos, 0)))
        {
            anos--;
        }

        if (anos >= 1)
        {
            await GarantirInsigniaAutomaticaAsync(
                aluno.Id,
                "1 ano de jornada",
                "Primeiro ano de participacao na escola.",
                "Tempo",
                "tempo-1-ano",
                cancellationToken);
        }

        var possuiGraduacaoAprovada = await dbContext.Graduacoes
            .AsNoTracking()
            .AnyAsync(x => x.AlunoId == aluno.Id && x.ResultadoAprovado, cancellationToken);

        if (possuiGraduacaoAprovada)
        {
            await GarantirInsigniaAutomaticaAsync(
                aluno.Id,
                "Graduação conquistada",
                "Resultado aprovado em exame de graduação.",
                "Evolução",
                "graduacao-aprovada",
                cancellationToken);
        }
    }

    private async Task GarantirInsigniaAutomaticaAsync(
        int alunoId,
        string nome,
        string descricao,
        string categoria,
        string regra,
        CancellationToken cancellationToken)
    {
        var insignia = await dbContext.Insignias
            .FirstOrDefaultAsync(x => x.RegraAutomatica == regra, cancellationToken);

        if (insignia is null)
        {
            insignia = new Insignia
            {
                Nome = nome,
                Descricao = descricao,
                Categoria = categoria,
                Icone = "star",
                Ativa = true,
                RegraAutomatica = regra
            };

            await dbContext.Insignias.AddAsync(insignia, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var jaPossui = await dbContext.AlunoInsignias
            .AnyAsync(x => x.AlunoId == alunoId && x.InsigniaId == insignia.Id, cancellationToken);

        if (jaPossui)
        {
            return;
        }

        await dbContext.AlunoInsignias.AddAsync(new AlunoInsignia
        {
            AlunoId = alunoId,
            InsigniaId = insignia.Id,
            Origem = InsigniaOrigemEnum.Automatica,
            ConcedidaEmUtc = DateTime.UtcNow
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private string ObterDocumentosPath()
    {
        return Path.Combine(webHostEnvironment.ContentRootPath, "App_Data", "uploads", "documentos");
    }

    private static AreaAlunoFrequenciaItemViewModel MapearFrequencia(FrequenciaAluno frequencia)
    {
        return new AreaAlunoFrequenciaItemViewModel
        {
            AulaId = frequencia.AulaId,
            Inicio = frequencia.Aula?.Inicio ?? DateTime.MinValue,
            Turma = frequencia.Aula?.Turma?.Nome ?? "Aula",
            Instrutor = frequencia.Aula?.InstrutorUsuario?.NomeExibicao,
            Status = frequencia.Status,
            Justificada = frequencia.Justificada,
            Justificativa = frequencia.Justificativa
        };
    }

    private static decimal CalcularPercentualPresenca(int total, int presencas)
    {
        return total == 0 ? 0m : decimal.Round((decimal)presencas / total * 100m, 1);
    }

    private static List<AreaAlunoAlertaViewModel> MontarAlertas(
        int mensalidadesAtrasadas,
        decimal totalEmAberto,
        IReadOnlyCollection<AreaAlunoDocumentoItemViewModel> documentos,
        IReadOnlyCollection<AreaAlunoComunicadoItemViewModel> comunicados,
        IReadOnlyCollection<AreaAlunoAulaItemViewModel> aulas,
        IReadOnlyCollection<AreaAlunoEventoItemViewModel> eventos)
    {
        var alertas = new List<AreaAlunoAlertaViewModel>();

        if (mensalidadesAtrasadas > 0)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "danger",
                Titulo = "Pendência financeira",
                Descricao = $"{mensalidadesAtrasadas} mensalidade(s) em atraso. Total em aberto: {totalEmAberto:C}.",
                Url = "/area-do-aluno/financeiro"
            });
        }

        var documentosPendentes = documentos.Count(x => x.Status is DocumentoStatusEnum.Solicitado or DocumentoStatusEnum.Pendente or DocumentoStatusEnum.Recusado);
        if (documentosPendentes > 0)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "warning",
                Titulo = "Documentos pendentes",
                Descricao = $"{documentosPendentes} documento(s) aguardam envio ou revisão.",
                Url = "/area-do-aluno/documentos"
            });
        }

        var comunicadosNaoLidos = comunicados.Count(x => !x.Lido);
        if (comunicadosNaoLidos > 0)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "info",
                Titulo = "Comunicados novos",
                Descricao = $"{comunicadosNaoLidos} comunicado(s) ainda não foram lidos.",
                Url = "/area-do-aluno/comunicados"
            });
        }

        var proximaAula = aulas.FirstOrDefault();
        if (proximaAula is not null)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "success",
                Titulo = "Próxima aula",
                Descricao = $"{proximaAula.Turma} em {proximaAula.Inicio:dd/MM HH:mm}.",
                Url = "/area-do-aluno/aulas"
            });
        }

        var eventoImportante = eventos.FirstOrDefault(x => x.Importante);
        if (eventoImportante is not null)
        {
            alertas.Add(new AreaAlunoAlertaViewModel
            {
                Tipo = "primary",
                Titulo = "Evento importante",
                Descricao = $"{eventoImportante.Titulo} em {eventoImportante.Inicio:dd/MM HH:mm}.",
                Url = "/area-do-aluno/eventos"
            });
        }

        return alertas.Take(5).ToList();
    }

    private sealed record AlunoPortalContexto(int AlunoId, IReadOnlyCollection<int> TurmaIds, string? FotoPerfilUrl);

    private sealed class PerfilBase
    {
        public int AlunoId { get; set; }
        public string NomeCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Celular { get; set; }
        public StatusAlunoEnum Status { get; set; }
        public string? TurmaPrincipal { get; set; }
        public DateOnly DataEntrada { get; set; }
    }

    private sealed record FrequenciaResumo(
        int Total,
        int Presencas,
        int FaltasNaoJustificadas,
        decimal PercentualPresenca);
}
