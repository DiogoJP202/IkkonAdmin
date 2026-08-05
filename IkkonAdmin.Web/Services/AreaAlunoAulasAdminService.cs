using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Security;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Pagination;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public sealed class AreaAlunoAulasAdminService(
    ApplicationDbContext dbContext,
    IClock clock,
    IAuditLogger auditLogger,
    ICurrentUserService currentUserService,
    IInsigniaRuleEvaluator insigniaRuleEvaluator) : IAreaAlunoAulasAdminService
{
    public async Task<AreaAlunoAulasAdminViewModel> ObterAulasAsync(
        AulaAdminFilter filter,
        CancellationToken cancellationToken = default)
    {
        var aulasQuery = ApplyLessonFilters(dbContext.Aulas.AsNoTracking(), filter);
        aulasQuery = ApplyLessonSort(aulasQuery, filter.Sort);

        return new AreaAlunoAulasAdminViewModel
        {
            Filtro = filter,
            Turmas = await ListarTurmasOpcoesAsync(cancellationToken),
            Instrutores = await ListarInstrutoresOpcoesAsync(cancellationToken),
            Horarios = await ListarHorariosAdminAsync(cancellationToken),
            TurmaInstrutores = await ListarInstrutoresTurmasAdminAsync(cancellationToken),
            Aulas = await ProjetarAulasAdmin(aulasQuery).ToPagedResultAsync(filter, cancellationToken)
        };
    }

    public async Task<OperationResult> CriarHorarioAsync(
        TurmaHorarioFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.HoraFim <= model.HoraInicio)
        {
            return OperationResult.Fail("O horário final deve ser posterior ao inicial.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return OperationResult.Fail("Turma não encontrada.");
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
        return OperationResult.Ok("Horário cadastrado.");
    }

    public async Task<OperationResult> AtualizarHorarioAsync(
        int id,
        TurmaHorarioFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.HoraFim <= model.HoraInicio)
        {
            return OperationResult.Fail("O horário final deve ser posterior ao inicial.");
        }

        var horario = await dbContext.TurmaHorarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (horario is null)
        {
            return OperationResult.Fail("Horário não encontrado.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return OperationResult.Fail("Turma não encontrada.");
        }

        horario.TurmaId = model.TurmaId;
        horario.DiaSemana = model.DiaSemana;
        horario.HoraInicio = model.HoraInicio;
        horario.HoraFim = model.HoraFim;
        horario.Local = LimparOpcional(model.Local);
        horario.Ativo = true;

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Horário atualizado.");
    }

    public async Task<OperationResult> ExcluirHorarioAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var horario = await dbContext.TurmaHorarios
            .Include(x => x.Aulas)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (horario is null)
        {
            return OperationResult.Fail("Horário não encontrado.");
        }

        if (horario.Aulas.Count > 0)
        {
            horario.Ativo = false;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Ok("Horário desativado porque possui aulas vinculadas.");
        }

        dbContext.TurmaHorarios.Remove(horario);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Horário excluído.");
    }

    public async Task<OperationResult> VincularInstrutorAsync(
        TurmaInstrutorFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return OperationResult.Fail("Turma não encontrada.");
        }

        var instrutorValido = await dbContext.UsuariosSistema
            .AnyAsync(
                x => x.Id == model.UsuarioSistemaId &&
                     x.Ativo &&
                     x.TipoAcesso != TipoAcessoEnum.Aluno,
                cancellationToken);

        if (!instrutorValido)
        {
            return OperationResult.Fail("Instrutor não encontrado ou sem acesso interno.");
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
        return OperationResult.Ok("Instrutor vinculado.");
    }

    public async Task<OperationResult> AtualizarInstrutorAsync(
        int id,
        TurmaInstrutorFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await dbContext.TurmaInstrutores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (vinculo is null)
        {
            return OperationResult.Fail("Vínculo de instrutor não encontrado.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return OperationResult.Fail("Turma não encontrada.");
        }

        var instrutorValido = await dbContext.UsuariosSistema
            .AnyAsync(
                x => x.Id == model.UsuarioSistemaId &&
                     x.Ativo &&
                     x.TipoAcesso != TipoAcessoEnum.Aluno,
                cancellationToken);

        if (!instrutorValido)
        {
            return OperationResult.Fail("Instrutor não encontrado ou sem acesso interno.");
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
        return OperationResult.Ok("Instrutor atualizado.");
    }

    public async Task<OperationResult> ExcluirInstrutorAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var vinculo = await dbContext.TurmaInstrutores.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (vinculo is null)
        {
            return OperationResult.Fail("Vínculo de instrutor não encontrado.");
        }

        dbContext.TurmaInstrutores.Remove(vinculo);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Instrutor removido da turma.");
    }

    public async Task<OperationResult> CriarAulaAsync(
        AulaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Fim <= model.Inicio)
        {
            return OperationResult.Fail("O fim da aula deve ser posterior ao início.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return OperationResult.Fail("Turma não encontrada.");
        }

        if (model.TurmaHorarioId.HasValue)
        {
            var horarioValido = await dbContext.TurmaHorarios
                .AnyAsync(x => x.Id == model.TurmaHorarioId.Value && x.TurmaId == model.TurmaId, cancellationToken);

            if (!horarioValido)
            {
                return OperationResult.Fail("Horário não pertence à turma selecionada.");
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
                return OperationResult.Fail("Instrutor inválido.");
            }
        }

        await dbContext.Aulas.AddAsync(new Aula
        {
            TurmaId = model.TurmaId,
            TurmaHorarioId = model.TurmaHorarioId,
            DataOcorrenciaRecorrencia = model.TurmaHorarioId.HasValue
                ? DateOnly.FromDateTime(model.Inicio)
                : null,
            InstrutorUsuarioId = model.InstrutorUsuarioId,
            Inicio = model.Inicio,
            Fim = model.Fim,
            Local = LimparOpcional(model.Local),
            Status = model.Status,
            Observacoes = LimparOpcional(model.Observacoes)
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Aula cadastrada.");
    }

    public async Task<OperationResult> AtualizarAulaAsync(
        int id,
        AulaFormViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.Fim <= model.Inicio)
        {
            return OperationResult.Fail("O fim da aula deve ser posterior ao início.");
        }

        var aula = await dbContext.Aulas.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (aula is null)
        {
            return OperationResult.Fail("Aula não encontrada.");
        }

        var turmaExiste = await dbContext.Turmas.AnyAsync(x => x.Id == model.TurmaId, cancellationToken);
        if (!turmaExiste)
        {
            return OperationResult.Fail("Turma não encontrada.");
        }

        if (model.TurmaHorarioId.HasValue)
        {
            var horarioValido = await dbContext.TurmaHorarios
                .AnyAsync(x => x.Id == model.TurmaHorarioId.Value && x.TurmaId == model.TurmaId, cancellationToken);

            if (!horarioValido)
            {
                return OperationResult.Fail("Horário não pertence à turma selecionada.");
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
                return OperationResult.Fail("Instrutor inválido.");
            }
        }

        aula.TurmaId = model.TurmaId;
        aula.TurmaHorarioId = model.TurmaHorarioId;
        aula.DataOcorrenciaRecorrencia ??= model.TurmaHorarioId.HasValue
            ? DateOnly.FromDateTime(model.Inicio)
            : null;
        aula.InstrutorUsuarioId = model.InstrutorUsuarioId;
        aula.Inicio = model.Inicio;
        aula.Fim = model.Fim;
        aula.Local = LimparOpcional(model.Local);
        aula.Status = model.Status;
        aula.Observacoes = LimparOpcional(model.Observacoes);

        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Aula atualizada.");
    }

    public async Task<OperationResult> ExcluirAulaAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var aula = await dbContext.Aulas
            .Include(x => x.Frequencias)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (aula is null)
        {
            return OperationResult.Fail("Aula não encontrada.");
        }

        if (aula.Frequencias.Count > 0)
        {
            aula.Status = StatusAulaEnum.Cancelada;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Ok("Aula cancelada porque possui frequência registrada.");
        }

        dbContext.Aulas.Remove(aula);
        await dbContext.SaveChangesAsync(cancellationToken);
        return OperationResult.Ok("Aula excluída.");
    }

    public async Task<AreaAlunoFrequenciaAdminViewModel> ObterFrequenciaAsync(
        FrequenciaAdminFilter filter,
        AulaAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        var aulasQuery = ApplyAccessScope(dbContext.Aulas.AsNoTracking(), accessScope);
        aulasQuery = ApplyAttendanceFilters(aulasQuery, filter);
        aulasQuery = ApplyLessonSort(aulasQuery, filter.Sort);

        return new AreaAlunoFrequenciaAdminViewModel
        {
            Filtro = filter,
            Turmas = await ListarTurmasOpcoesAsync(cancellationToken),
            Instrutores = await ListarInstrutoresOpcoesAsync(cancellationToken),
            Aulas = await ProjetarAulasAdmin(aulasQuery).ToPagedResultAsync(filter, cancellationToken)
        };
    }

    public async Task<AreaAlunoRegistroFrequenciaViewModel?> ObterRegistroFrequenciaAsync(
        int aulaId,
        AulaAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        var aula = await ApplyAccessScope(dbContext.Aulas.AsQueryable(), accessScope)
            .AsNoTracking()
            .Include(x => x.Turma)
            .ThenInclude(x => x!.AlunoTurmas)
            .ThenInclude(x => x.Aluno)
            .Include(x => x.InstrutorUsuario)
            .Include(x => x.Frequencias)
            .FirstOrDefaultAsync(x => x.Id == aulaId, cancellationToken);

        if (aula is null || aula.Turma is null)
        {
            await LogDeniedAttendanceAccessAsync(aulaId, accessScope.UserId, cancellationToken);
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

    public async Task<OperationResult> SalvarFrequenciaAsync(
        FrequenciaRegistroPostViewModel model,
        AulaAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        var aula = await ApplyAccessScope(dbContext.Aulas.AsQueryable(), accessScope)
            .Include(x => x.Turma)
            .ThenInclude(x => x!.AlunoTurmas)
            .Include(x => x.Frequencias)
            .FirstOrDefaultAsync(x => x.Id == model.AulaId, cancellationToken);

        if (aula is null || aula.Turma is null)
        {
            await LogDeniedAttendanceAccessAsync(model.AulaId, accessScope.UserId, cancellationToken);
            return OperationResult.NotFound("Aula não encontrada.");
        }

        var hadExistingAttendance = aula.Frequencias.Count > 0;
        var before = AuditJson.Serialize(aula.Frequencias.Select(x => new
        {
            x.AlunoId,
            x.Status,
            x.Justificada
        }));
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
            frequencia.RegistradoPorUsuarioId = accessScope.UserId;
            frequencia.RegistradoEmUtc = clock.UtcNow;
        }

        if (aula.Status == StatusAulaEnum.Agendada)
        {
            aula.Status = StatusAulaEnum.Realizada;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = accessScope.UserId,
            Acao = hadExistingAttendance
                ? AuditEventCodes.AttendanceCorrected
                : AuditEventCodes.AttendanceRecorded,
            Entidade = nameof(Aula),
            EntidadeId = aula.Id.ToString(),
            Descricao = hadExistingAttendance
                ? "Frequência de aula corrigida."
                : "Frequência de aula registrada.",
            DadosAntesJson = before,
            DadosDepoisJson = AuditJson.Serialize(aula.Frequencias.Select(x => new
            {
                x.AlunoId,
                x.Status,
                x.Justificada
            })),
            EnderecoIp = currentUserService.RemoteIpAddress,
            CorrelationId = currentUserService.CorrelationId
        }, cancellationToken);

        var affectedStudentIds = model.Alunos
            .Where(x => alunosDaTurma.Contains(x.AlunoId))
            .Select(x => x.AlunoId)
            .Distinct()
            .ToArray();
        if (affectedStudentIds.Length > 0)
        {
            await insigniaRuleEvaluator.EvaluateAsync(affectedStudentIds, cancellationToken);
        }

        return OperationResult.Ok("Frequência salva.");
    }

    public Task<int> ContarAulasProximasAsync(
        DateTime inicioMinimo,
        AulaAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        return ApplyAccessScope(dbContext.Aulas.AsNoTracking(), accessScope)
            .CountAsync(x => x.Inicio >= inicioMinimo && x.Status == StatusAulaEnum.Agendada, cancellationToken);
    }

    public Task<int> ContarFrequenciasRegistradasAsync(
        DateTime inicio,
        DateTime fim,
        AulaAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        var aulasPermitidas = ApplyAccessScope(dbContext.Aulas.AsNoTracking(), accessScope).Select(x => x.Id);
        return dbContext.FrequenciasAlunos.CountAsync(
            x => aulasPermitidas.Contains(x.AulaId) && x.RegistradoEmUtc >= inicio && x.RegistradoEmUtc < fim,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<AreaAlunoAulaAdminItemViewModel>> ListarAulasAdminAsync(
        int limite,
        DateTime inicioMinimo,
        AulaAccessScope accessScope,
        CancellationToken cancellationToken = default)
    {
        return await ProjetarAulasAdmin(
                ApplyAccessScope(dbContext.Aulas.AsNoTracking(), accessScope)
                    .Where(x => x.Inicio >= inicioMinimo)
                    .OrderBy(x => x.Inicio)
                    .Take(limite))
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<AreaAlunoAulaAdminItemViewModel> ProjetarAulasAdmin(
        IQueryable<Aula> query)
    {
        return query
            .Include(x => x.Turma)
            .ThenInclude(x => x!.AlunoTurmas)
            .Include(x => x.InstrutorUsuario)
            .Include(x => x.Frequencias)
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
            });
    }

    private static IQueryable<Aula> ApplyLessonFilters(IQueryable<Aula> query, AulaAdminFilter filter)
    {
        filter.Normalize();
        query = ApplyDateAndRelationshipFilters(
            query,
            filter.Inicio,
            filter.Fim,
            filter.TurmaId,
            filter.InstrutorId);

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        return query;
    }

    private static IQueryable<Aula> ApplyAttendanceFilters(
        IQueryable<Aula> query,
        FrequenciaAdminFilter filter)
    {
        filter.Normalize();
        query = ApplyDateAndRelationshipFilters(
            query,
            filter.Inicio,
            filter.Fim,
            filter.TurmaId,
            filter.InstrutorId);

        if (filter.Preenchida.HasValue)
        {
            query = filter.Preenchida.Value
                ? query.Where(x => x.Frequencias.Any())
                : query.Where(x => !x.Frequencias.Any());
        }

        return query;
    }

    private static IQueryable<Aula> ApplyDateAndRelationshipFilters(
        IQueryable<Aula> query,
        DateOnly? start,
        DateOnly? end,
        int? classroomId,
        int? instructorId)
    {
        if (start.HasValue)
        {
            var startDate = start.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.Inicio >= startDate);
        }

        if (end.HasValue)
        {
            var endExclusive = end.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(x => x.Inicio < endExclusive);
        }

        if (classroomId.HasValue)
        {
            query = query.Where(x => x.TurmaId == classroomId.Value);
        }

        if (instructorId.HasValue)
        {
            query = query.Where(x => x.InstrutorUsuarioId == instructorId.Value);
        }

        return query;
    }

    private static IQueryable<Aula> ApplyLessonSort(IQueryable<Aula> query, string? sort)
    {
        return sort switch
        {
            "data-desc" => query.OrderByDescending(x => x.Inicio).ThenByDescending(x => x.Id),
            "turma" => query.OrderBy(x => x.Turma!.Nome).ThenBy(x => x.Inicio),
            "status" => query.OrderBy(x => x.Status).ThenBy(x => x.Inicio),
            _ => query.OrderBy(x => x.Inicio).ThenBy(x => x.Id)
        };
    }

    private IQueryable<Aula> ApplyAccessScope(IQueryable<Aula> query, AulaAccessScope accessScope)
    {
        if (accessScope.HasGlobalAccess)
        {
            return query;
        }

        if (!accessScope.UserId.HasValue)
        {
            return query.Where(_ => false);
        }

        var userId = accessScope.UserId.Value;
        return query.Where(aula =>
            aula.InstrutorUsuarioId == userId ||
            (aula.InstrutorUsuarioId == null &&
             dbContext.TurmaInstrutores.Any(vinculo =>
                 vinculo.TurmaId == aula.TurmaId &&
                 vinculo.UsuarioSistemaId == userId &&
                 vinculo.UsuarioSistema != null &&
                 vinculo.UsuarioSistema.Ativo &&
                 vinculo.DataInicio <= DateOnly.FromDateTime(aula.Inicio) &&
                 (!vinculo.DataFim.HasValue || vinculo.DataFim.Value >= DateOnly.FromDateTime(aula.Inicio)))));
    }

    private Task LogDeniedAttendanceAccessAsync(
        int aulaId,
        int? userId,
        CancellationToken cancellationToken)
    {
        return auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = userId,
            Acao = AuditEventCodes.SensitiveAccessDenied,
            Entidade = nameof(Aula),
            EntidadeId = aulaId.ToString(),
            Descricao = "Tentativa negada de acesso ao registro de frequência.",
            EnderecoIp = currentUserService.RemoteIpAddress,
            CorrelationId = currentUserService.CorrelationId
        }, cancellationToken);
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

    private static string? LimparOpcional(string? valor)
    {
        var texto = valor?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }
}
