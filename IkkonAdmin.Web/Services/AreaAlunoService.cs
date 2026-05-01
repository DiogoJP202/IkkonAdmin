using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AreaAlunoService(ApplicationDbContext dbContext) : IAreaAlunoService
{
    public async Task<AreaAlunoDashboardViewModel?> ObterDashboardAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var perfil = await ObterPerfilBaseAsync(alunoId.Value, cancellationToken);
        if (perfil is null)
        {
            return null;
        }

        var mensalidades = await ListarMensalidadesAsync(alunoId.Value, 6, cancellationToken);
        var turmas = await ListarTurmasAsync(alunoId.Value, cancellationToken);
        var resumoFinanceiro = await ObterResumoFinanceiroAsync(alunoId.Value, cancellationToken);

        return new AreaAlunoDashboardViewModel
        {
            NomeCompleto = perfil.NomeCompleto,
            Email = perfil.Email,
            Celular = perfil.Celular,
            Status = perfil.Status,
            TurmaPrincipal = perfil.TurmaPrincipal,
            DataEntrada = perfil.DataEntrada,
            TotalEmAberto = resumoFinanceiro.TotalEmAberto,
            MensalidadesAtrasadas = resumoFinanceiro.MensalidadesAtrasadas,
            Turmas = turmas,
            MensalidadesRecentes = mensalidades
        };
    }

    public async Task<AreaAlunoPerfilViewModel?> ObterPerfilAsync(int usuarioId, CancellationToken cancellationToken = default)
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

    public async Task<AreaAlunoFinanceiroViewModel?> ObterFinanceiroAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var mensalidades = await ListarMensalidadesAsync(alunoId.Value, 18, cancellationToken);
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

    public async Task<AreaAlunoTurmasViewModel?> ObterTurmasAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var alunoId = await ObterAlunoIdVinculadoAsync(usuarioId, cancellationToken);
        if (!alunoId.HasValue)
        {
            return null;
        }

        var turmaPrincipal = await dbContext.Alunos
            .AsNoTracking()
            .Where(x => x.Id == alunoId.Value)
            .Select(x => x.Turma != null ? x.Turma.Nome : null)
            .FirstOrDefaultAsync(cancellationToken);

        return new AreaAlunoTurmasViewModel
        {
            TurmaPrincipal = turmaPrincipal,
            Turmas = await ListarTurmasAsync(alunoId.Value, cancellationToken)
        };
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
        return await dbContext.Mensalidades
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId)
            .OrderByDescending(x => x.Competencia)
            .Take(limite)
            .Select(x => new AreaAlunoMensalidadeItemViewModel
            {
                Competencia = x.Competencia,
                DataVencimento = x.DataVencimento,
                ValorFinal = x.ValorFinal,
                Status = x.Status,
                DataPagamento = x.DataPagamento
            })
            .ToListAsync(cancellationToken);
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

    private async Task<List<AreaAlunoTurmaItemViewModel>> ListarTurmasAsync(int alunoId, CancellationToken cancellationToken)
    {
        return await dbContext.AlunosTurmas
            .AsNoTracking()
            .Where(x => x.AlunoId == alunoId && x.Turma != null)
            .OrderBy(x => x.Turma.Nome)
            .Select(x => new AreaAlunoTurmaItemViewModel
            {
                Nome = x.Turma.Nome,
                Modalidade = x.Turma.Modalidade,
                Horario = x.Turma.Horario,
                DataVinculo = x.DataVinculo
            })
            .ToListAsync(cancellationToken);
    }

    private sealed class PerfilBase
    {
        public string NomeCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Celular { get; set; }
        public StatusAlunoEnum Status { get; set; }
        public string? TurmaPrincipal { get; set; }
        public DateOnly DataEntrada { get; set; }
    }
}
