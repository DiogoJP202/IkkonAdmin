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
            return AreaAlunoOperacaoResult.Falha("O horário final deve ser posterior ao inicial.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma não encontrada.");
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
        return AreaAlunoOperacaoResult.Ok("Horário cadastrado.");
    }

    public async Task<AreaAlunoOperacaoResult> AtualizarHorarioAsync(
        int id,
        TurmaHorarioFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.HoraFim <= model.HoraInicio)
        {
            return AreaAlunoOperacaoResult.Falha("O horário final deve ser posterior ao inicial.");
        }

        var horario = await dbContext.TurmaHorarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (horario is null)
        {
            return AreaAlunoOperacaoResult.Falha("Horário não encontrado.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma não encontrada.");
        }

        horario.TurmaId = model.TurmaId;
        horario.DiaSemana = model.DiaSemana;
        horario.HoraInicio = model.HoraInicio;
        horario.HoraFim = model.HoraFim;
        horario.Local = LimparOpcional(model.Local);
        horario.Ativo = true;

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Horário atualizado.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirHorarioAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var horario = await dbContext.TurmaHorarios
            .Include(x => x.Aulas)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (horario is null)
        {
            return AreaAlunoOperacaoResult.Falha("Horário não encontrado.");
        }

        if (horario.Aulas.Count > 0)
        {
            horario.Ativo = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return AreaAlunoOperacaoResult.Ok("Horário desativado porque possui aulas vinculadas.");
        }

        dbContext.TurmaHorarios.Remove(horario);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Horário excluído.");
    }

    public async Task<AreaAlunoOperacaoResult> VincularInstrutorAsync(
        TurmaInstrutorFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma não encontrada.");
        }

        var instrutorValido = await dbContext.UsuariosSistema
            .AnyAsync(
                x => x.Id == model.UsuarioSistemaId &&
                     x.Ativo &&
                     x.TipoAcesso != TipoAcessoEnum.Aluno,
                cancellationToken);

        if (!instrutorValido)
        {
            return AreaAlunoOperacaoResult.Falha("Instrutor não encontrado ou sem acesso interno.");
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

    public async Task<AreaAlunoOperacaoResult> AtualizarInstrutorAsync(
        int id,
        TurmaInstrutorFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await dbContext.TurmaInstrutores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (vinculo is null)
        {
            return AreaAlunoOperacaoResult.Falha("Vínculo de instrutor não encontrado.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma não encontrada.");
        }

        var instrutorValido = await dbContext.UsuariosSistema
            .AnyAsync(
                x => x.Id == model.UsuarioSistemaId &&
                     x.Ativo &&
                     x.TipoAcesso != TipoAcessoEnum.Aluno,
                cancellationToken);

        if (!instrutorValido)
        {
            return AreaAlunoOperacaoResult.Falha("Instrutor não encontrado ou sem acesso interno.");
        }

        if (model.Principal)
        {
            var principais = await dbContext.TurmaInstrutores
                .Where(x => x.Id != id && x.TurmaId == model.TurmaId && x.Principal && !x.DataFim.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var principal in principais)
            {
                principal.Principal = false;
            }
        }

        vinculo.TurmaId = model.TurmaId;
        vinculo.UsuarioSistemaId = model.UsuarioSistemaId;
        vinculo.Principal = model.Principal;
        vinculo.DataInicio = model.DataInicio;
        vinculo.DataFim = model.DataFim;

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Instrutor atualizado.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirInstrutorAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await dbContext.TurmaInstrutores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (vinculo is null)
        {
            return AreaAlunoOperacaoResult.Falha("Vínculo de instrutor não encontrado.");
        }

        dbContext.TurmaInstrutores.Remove(vinculo);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Instrutor removido da turma.");
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
            return AreaAlunoOperacaoResult.Falha("Turma não encontrada.");
        }

        if (model.TurmaHorarioId.HasValue)
        {
            var horarioValido = await dbContext.TurmaHorarios
                .AnyAsync(x => x.Id == model.TurmaHorarioId.Value && x.TurmaId == model.TurmaId, cancellationToken);

            if (!horarioValido)
            {
                return AreaAlunoOperacaoResult.Falha("Horário não pertence à turma selecionada.");
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
                return AreaAlunoOperacaoResult.Falha("Instrutor inválido.");
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

    public async Task<AreaAlunoOperacaoResult> AtualizarAulaAsync(
        int id,
        AulaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Fim <= model.Inicio)
        {
            return AreaAlunoOperacaoResult.Falha("O fim da aula deve ser posterior ao inicio.");
        }

        var aula = await dbContext.Aulas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (aula is null)
        {
            return AreaAlunoOperacaoResult.Falha("Aula não encontrada.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Turma não encontrada.");
        }

        if (model.TurmaHorarioId.HasValue)
        {
            var horarioValido = await dbContext.TurmaHorarios
                .AnyAsync(x => x.Id == model.TurmaHorarioId.Value && x.TurmaId == model.TurmaId, cancellationToken);

            if (!horarioValido)
            {
                return AreaAlunoOperacaoResult.Falha("Horário não pertence à turma selecionada.");
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
                return AreaAlunoOperacaoResult.Falha("Instrutor inválido.");
            }
        }

        aula.TurmaId = model.TurmaId;
        aula.TurmaHorarioId = model.TurmaHorarioId;
        aula.InstrutorUsuarioId = model.InstrutorUsuarioId;
        aula.Inicio = model.Inicio;
        aula.Fim = model.Fim;
        aula.Local = LimparOpcional(model.Local);
        aula.Status = model.Status;
        aula.Observacoes = LimparOpcional(model.Observacoes);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Aula atualizada.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirAulaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var aula = await dbContext.Aulas
            .Include(x => x.Frequencias)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (aula is null)
        {
            return AreaAlunoOperacaoResult.Falha("Aula não encontrada.");
        }

        if (aula.Frequencias.Count > 0)
        {
            aula.Status = StatusAulaEnum.Cancelada;
            await dbContext.SaveChangesAsync(cancellationToken);
            return AreaAlunoOperacaoResult.Ok("Aula cancelada porque possui frequência registrada.");
        }

        dbContext.Aulas.Remove(aula);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Aula excluida.");
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
            return AreaAlunoOperacaoResult.Falha("Aula não encontrada.");
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
        return AreaAlunoOperacaoResult.Ok("Frequência salva.");
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
            return AreaAlunoOperacaoResult.Falha("Já existe um tipo de documento com este nome.");
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

    public async Task<AreaAlunoOperacaoResult> AtualizarDocumentoTipoAsync(
        int id,
        DocumentoTipoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var tipo = await dbContext.DocumentoTipos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (tipo is null)
        {
            return AreaAlunoOperacaoResult.Falha("Tipo de documento não encontrado.");
        }

        var nome = model.Nome.Trim();
        var existe = await dbContext.DocumentoTipos
            .AnyAsync(x => x.Id != id && x.Nome == nome, cancellationToken);

        if (existe)
        {
            return AreaAlunoOperacaoResult.Falha("Já existe um tipo de documento com este nome.");
        }

        tipo.Nome = nome;
        tipo.Descricao = LimparOpcional(model.Descricao);
        tipo.Obrigatorio = model.Obrigatorio;
        tipo.Ativo = model.Ativo;

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Tipo de documento atualizado.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirDocumentoTipoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var tipo = await dbContext.DocumentoTipos
            .Include(x => x.Solicitacoes)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tipo is null)
        {
            return AreaAlunoOperacaoResult.Falha("Tipo de documento não encontrado.");
        }

        if (tipo.Solicitacoes.Count > 0)
        {
            tipo.Ativo = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return AreaAlunoOperacaoResult.Ok("Tipo desativado porque possui solicitações vinculadas.");
        }

        dbContext.DocumentoTipos.Remove(tipo);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Tipo de documento excluído.");
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
            return AreaAlunoOperacaoResult.Falha("Tipo de documento ou aluno inválido.");
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

    public async Task<AreaAlunoOperacaoResult> AtualizarDocumentoSolicitacaoAsync(
        int id,
        DocumentoSolicitacaoEdicaoViewModel model,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (solicitacao is null)
        {
            return AreaAlunoOperacaoResult.Falha("Solicitação não encontrada.");
        }

        var tipoExiste = await dbContext.DocumentoTipos.AnyAsync(x => x.Id == model.DocumentoTipoId, cancellationToken);
        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);

        if (!tipoExiste || !alunoExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Tipo de documento ou aluno inválido.");
        }

        solicitacao.DocumentoTipoId = model.DocumentoTipoId;
        solicitacao.AlunoId = model.AlunoId;
        solicitacao.Status = model.Status;
        solicitacao.DataLimite = model.DataLimite;
        solicitacao.ObservacaoAdministrativa = LimparOpcional(model.ObservacaoAdministrativa);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Solicitação atualizada.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirDocumentoSolicitacaoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await dbContext.DocumentoSolicitacoes
            .Include(x => x.Envios)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (solicitacao is null)
        {
            return AreaAlunoOperacaoResult.Falha("Solicitação não encontrada.");
        }

        if (solicitacao.Envios.Count > 0)
        {
            return AreaAlunoOperacaoResult.Falha("Não é possível excluir uma solicitação com arquivos enviados. Altere o status para recusado ou pendente.");
        }

        dbContext.DocumentoSolicitacoes.Remove(solicitacao);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Solicitação excluída.");
    }

    public async Task<AreaAlunoOperacaoResult> AvaliarDocumentoAsync(
        DocumentoAvaliacaoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var solicitacao = await dbContext.DocumentoSolicitacoes
            .FirstOrDefaultAsync(x => x.Id == model.SolicitacaoId, cancellationToken);

        if (solicitacao is null)
        {
            return AreaAlunoOperacaoResult.Falha("Solicitação não encontrada.");
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

    public async Task<AreaAlunoOperacaoResult> AtualizarComunicadoAsync(
        int id,
        ComunicadoFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var alvoValidado = await ValidarAlvoAsync(model.AlvoTipo, model.AlunoId, model.TurmaId, cancellationToken);
        if (!alvoValidado.Sucesso)
        {
            return alvoValidado;
        }

        var comunicado = await dbContext.Comunicados
            .Include(x => x.Alvos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (comunicado is null)
        {
            return AreaAlunoOperacaoResult.Falha("Comunicado não encontrado.");
        }

        comunicado.Titulo = model.Titulo.Trim();
        comunicado.Conteudo = model.Conteudo.Trim();
        comunicado.Importante = model.Importante;
        comunicado.Fixado = model.Fixado;
        comunicado.PublicadoEmUtc = model.PublicadoEmUtc;
        comunicado.ExpiraEmUtc = model.ExpiraEmUtc;
        comunicado.Ativo = true;

        dbContext.ComunicadosAlvos.RemoveRange(comunicado.Alvos);
        comunicado.Alvos.Add(CriarComunicadoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Comunicado atualizado.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirComunicadoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var comunicado = await dbContext.Comunicados
            .Include(x => x.Alvos)
            .Include(x => x.Leituras)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (comunicado is null)
        {
            return AreaAlunoOperacaoResult.Falha("Comunicado não encontrado.");
        }

        if (comunicado.Leituras.Count > 0)
        {
            comunicado.Ativo = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return AreaAlunoOperacaoResult.Ok("Comunicado desativado porque já possui leituras.");
        }

        dbContext.ComunicadosAlvos.RemoveRange(comunicado.Alvos);
        dbContext.Comunicados.Remove(comunicado);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Comunicado excluído.");
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

    public async Task<AreaAlunoOperacaoResult> AtualizarEventoAsync(
        int id,
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

        var evento = await dbContext.EventosAlunoPortal
            .Include(x => x.Alvos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (evento is null)
        {
            return AreaAlunoOperacaoResult.Falha("Evento não encontrado.");
        }

        evento.Titulo = model.Titulo.Trim();
        evento.Descricao = LimparOpcional(model.Descricao);
        evento.Inicio = model.Inicio;
        evento.Fim = model.Fim;
        evento.Local = LimparOpcional(model.Local);
        evento.Tipo = model.Tipo;
        evento.Importante = model.Importante;
        evento.GoogleEventoId = LimparOpcional(model.GoogleEventoId);
        evento.Ativo = true;

        dbContext.EventosAlunoPortalAlvos.RemoveRange(evento.Alvos);
        evento.Alvos.Add(CriarEventoAlvo(model.AlvoTipo, model.AlunoId, model.TurmaId));

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Evento atualizado.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirEventoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var evento = await dbContext.EventosAlunoPortal
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (evento is null)
        {
            return AreaAlunoOperacaoResult.Falha("Evento não encontrado.");
        }

        evento.Ativo = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Evento desativado.");
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
            return AreaAlunoOperacaoResult.Falha("Já existe uma insígnia com este nome.");
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
        return AreaAlunoOperacaoResult.Ok("Insígnia criada.");
    }

    public async Task<AreaAlunoOperacaoResult> AtualizarInsigniaAsync(
        int id,
        InsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var insignia = await dbContext.Insignias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (insignia is null)
        {
            return AreaAlunoOperacaoResult.Falha("Insígnia não encontrada.");
        }

        var nome = model.Nome.Trim();
        var existe = await dbContext.Insignias
            .AnyAsync(x => x.Id != id && x.Nome == nome, cancellationToken);

        if (existe)
        {
            return AreaAlunoOperacaoResult.Falha("Já existe uma insígnia com este nome.");
        }

        insignia.Nome = nome;
        insignia.Descricao = LimparOpcional(model.Descricao);
        insignia.Icone = LimparOpcional(model.Icone);
        insignia.Categoria = LimparOpcional(model.Categoria);
        insignia.RegraAutomatica = LimparOpcional(model.RegraAutomatica);
        insignia.Ativa = model.Ativa;

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Insígnia atualizada.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var insignia = await dbContext.Insignias
            .Include(x => x.Alunos)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (insignia is null)
        {
            return AreaAlunoOperacaoResult.Falha("Insígnia não encontrada.");
        }

        if (insignia.Alunos.Count > 0)
        {
            insignia.Ativa = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return AreaAlunoOperacaoResult.Ok("Insígnia desativada porque já foi atribuída.");
        }

        dbContext.Insignias.Remove(insignia);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Insígnia excluída.");
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
            return AreaAlunoOperacaoResult.Falha("Aluno ou insígnia inválida.");
        }

        var jaPossui = await dbContext.AlunoInsignias
            .AnyAsync(x => x.AlunoId == model.AlunoId && x.InsigniaId == model.InsigniaId, cancellationToken);

        if (jaPossui)
        {
            return AreaAlunoOperacaoResult.Falha("Este aluno já possui esta insígnia.");
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
        return AreaAlunoOperacaoResult.Ok("Insígnia atribuída ao aluno.");
    }

    public async Task<AreaAlunoOperacaoResult> AtualizarAlunoInsigniaAsync(
        int id,
        AlunoInsigniaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var conquista = await dbContext.AlunoInsignias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (conquista is null)
        {
            return AreaAlunoOperacaoResult.Falha("Conquista não encontrada.");
        }

        var alunoExiste = await dbContext.Alunos.AnyAsync(x => x.Id == model.AlunoId, cancellationToken);
        var insigniaExiste = await dbContext.Insignias.AnyAsync(x => x.Id == model.InsigniaId && x.Ativa, cancellationToken);

        if (!alunoExiste || !insigniaExiste)
        {
            return AreaAlunoOperacaoResult.Falha("Aluno ou insígnia inválida.");
        }

        var duplicada = await dbContext.AlunoInsignias
            .AnyAsync(x => x.Id != id && x.AlunoId == model.AlunoId && x.InsigniaId == model.InsigniaId, cancellationToken);

        if (duplicada)
        {
            return AreaAlunoOperacaoResult.Falha("Este aluno já possui esta insígnia.");
        }

        conquista.AlunoId = model.AlunoId;
        conquista.InsigniaId = model.InsigniaId;
        conquista.Observacao = LimparOpcional(model.Observacao);

        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Conquista atualizada.");
    }

    public async Task<AreaAlunoOperacaoResult> ExcluirAlunoInsigniaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var conquista = await dbContext.AlunoInsignias.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (conquista is null)
        {
            return AreaAlunoOperacaoResult.Falha("Conquista não encontrada.");
        }

        dbContext.AlunoInsignias.Remove(conquista);
        await dbContext.SaveChangesAsync(cancellationToken);
        return AreaAlunoOperacaoResult.Ok("Conquista removida do aluno.");
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
                TurmaId = x.TurmaId,
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
                TurmaId = x.TurmaId,
                Turma = x.Turma != null ? x.Turma.Nome : $"Turma #{x.TurmaId}",
                UsuarioSistemaId = x.UsuarioSistemaId,
                Instrutor = x.UsuarioSistema != null ? x.UsuarioSistema.NomeExibicao : $"Usuário #{x.UsuarioSistemaId}",
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
                TurmaId = x.TurmaId,
                Turma = x.Turma != null ? x.Turma.Nome : $"Turma #{x.TurmaId}",
                TurmaHorarioId = x.TurmaHorarioId,
                InstrutorUsuarioId = x.InstrutorUsuarioId,
                Inicio = x.Inicio,
                Fim = x.Fim,
                Local = x.Local,
                Instrutor = x.InstrutorUsuario != null ? x.InstrutorUsuario.NomeExibicao : null,
                Status = x.Status,
                Observacoes = x.Observacoes,
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
                    AlunoId = x.AlunoId,
                    Aluno = x.Aluno?.NomeCompleto ?? $"Aluno #{x.AlunoId}",
                    DocumentoTipoId = x.DocumentoTipoId,
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
        var comunicados = await dbContext.Comunicados
            .AsNoTracking()
            .Include(x => x.Alvos)
            .Include(x => x.Leituras)
            .OrderByDescending(x => x.Fixado)
            .ThenByDescending(x => x.PublicadoEmUtc)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return comunicados
            .Select(x =>
            {
                var alvo = x.Alvos.FirstOrDefault();
                return new AreaAlunoComunicadoAdminItemViewModel
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    Conteudo = x.Conteudo,
                    Importante = x.Importante,
                    Fixado = x.Fixado,
                    Ativo = x.Ativo,
                    PublicadoEmUtc = x.PublicadoEmUtc,
                    ExpiraEmUtc = x.ExpiraEmUtc,
                    AlvoTipo = ObterAlvoTipo(alvo?.Todos == true, alvo?.AlunoId, alvo?.TurmaId),
                    AlunoId = alvo?.AlunoId,
                    TurmaId = alvo?.TurmaId,
                    Leituras = x.Leituras.Count
                };
            })
            .ToList();
    }

    private async Task<IReadOnlyCollection<AreaAlunoEventoAdminItemViewModel>> ListarEventosAdminAsync(
        int limite,
        CancellationToken cancellationToken)
    {
        var eventos = await dbContext.EventosAlunoPortal
            .AsNoTracking()
            .Include(x => x.Alvos)
            .OrderBy(x => x.Inicio)
            .Take(limite)
            .ToListAsync(cancellationToken);

        return eventos
            .Select(x =>
            {
                var alvo = x.Alvos.FirstOrDefault();
                return new AreaAlunoEventoAdminItemViewModel
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    Descricao = x.Descricao,
                    Inicio = x.Inicio,
                    Fim = x.Fim,
                    Local = x.Local,
                    Tipo = x.Tipo,
                    Importante = x.Importante,
                    Ativo = x.Ativo,
                    GoogleEventoId = x.GoogleEventoId,
                    AlvoTipo = ObterAlvoTipo(alvo?.Todos == true, alvo?.AlunoId, alvo?.TurmaId),
                    AlunoId = alvo?.AlunoId,
                    TurmaId = alvo?.TurmaId
                };
            })
            .ToList();
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
                Descricao = x.Descricao,
                Icone = x.Icone,
                Categoria = x.Categoria,
                RegraAutomatica = x.RegraAutomatica,
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
                AlunoId = x.AlunoId,
                Aluno = x.Aluno != null ? x.Aluno.NomeCompleto : $"Aluno #{x.AlunoId}",
                InsigniaId = x.InsigniaId,
                Insignia = x.Insignia != null ? x.Insignia.Nome : $"Insígnia #{x.InsigniaId}",
                ConcedidaEmUtc = x.ConcedidaEmUtc,
                Origem = x.Origem,
                Observacao = x.Observacao
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
            : AreaAlunoOperacaoResult.Falha("Selecione uma turma válida.");
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

    private static ComunicadoAlvoTipoEnum ObterAlvoTipo(
        bool todos,
        int? alunoId,
        int? turmaId)
    {
        if (todos)
        {
            return ComunicadoAlvoTipoEnum.Todos;
        }

        if (alunoId.HasValue)
        {
            return ComunicadoAlvoTipoEnum.Aluno;
        }

        if (turmaId.HasValue)
        {
            return ComunicadoAlvoTipoEnum.Turma;
        }

        return ComunicadoAlvoTipoEnum.Todos;
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
