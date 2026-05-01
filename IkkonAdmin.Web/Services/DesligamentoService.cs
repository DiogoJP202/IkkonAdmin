using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class DesligamentoService(ApplicationDbContext dbContext) : IDesligamentoService
{
    public async Task<IReadOnlyList<Desligamento>> ListarAsync(
        string? busca = null,
        bool? confirmado = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Desligamentos
            .AsNoTracking()
            .Include(x => x.Aluno)
            .ThenInclude(x => x!.Turma)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var buscaTexto = busca.Trim();
            query = query.Where(x =>
                x.Aluno != null &&
                (x.Aluno.NomeCompleto.Contains(buscaTexto) ||
                 x.Aluno.CPF.Contains(buscaTexto)));
        }

        if (confirmado.HasValue)
        {
            query = confirmado.Value
                ? query.Where(x => x.DataConfirmacao.HasValue)
                : query.Where(x => !x.DataConfirmacao.HasValue);
        }

        return await query
            .OrderByDescending(x => x.DataSolicitacao)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Aluno>> ListarAlunosElegiveisAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Alunos
            .AsNoTracking()
            .Include(x => x.Turma)
            .Where(x => x.Status == StatusAlunoEnum.Ativo || x.Status == StatusAlunoEnum.Inativo)
            .OrderBy(x => x.NomeCompleto)
            .ToListAsync(cancellationToken);
    }

    public Task<Desligamento?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Desligamentos
            .AsNoTracking()
            .Include(x => x.Aluno)
            .ThenInclude(x => x!.Turma)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<decimal> CalcularPendenciasAsync(int alunoId, CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(DateTime.Today);

        var totalPendencias = await dbContext.Mensalidades
            .Where(x => x.AlunoId == alunoId &&
                        (x.Status == StatusMensalidadeEnum.Atrasado ||
                         (x.Status == StatusMensalidadeEnum.Pendente && x.DataVencimento <= hoje)))
            .SumAsync(x => (decimal?)x.ValorFinal, cancellationToken) ?? 0m;

        return decimal.Round(totalPendencias, 2);
    }

    public async Task<int> CriarAsync(
        Desligamento desligamento,
        CancellationToken cancellationToken = default)
    {
        desligamento.Motivo = desligamento.Motivo.Trim();
        desligamento.Observacoes = LimparOpcional(desligamento.Observacoes);

        var existeAberto = await dbContext.Desligamentos
            .AnyAsync(x => x.AlunoId == desligamento.AlunoId && !x.DataConfirmacao.HasValue, cancellationToken);

        if (existeAberto)
        {
            throw new InvalidOperationException("Ja existe um processo de desligamento em aberto para este aluno.");
        }

        await dbContext.Desligamentos.AddAsync(desligamento, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return desligamento.Id;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        string motivo,
        decimal pendenciaFinanceira,
        decimal multaRescisoria,
        bool requerimentoRecebido,
        bool acessosRemovidos,
        string? observacoes,
        CancellationToken cancellationToken = default)
    {
        var desligamento = await dbContext.Desligamentos
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (desligamento is null)
        {
            return false;
        }

        desligamento.Motivo = motivo.Trim();
        desligamento.PendenciaFinanceira = decimal.Round(pendenciaFinanceira, 2);
        desligamento.MultaRescisoria = decimal.Round(multaRescisoria, 2);
        desligamento.RequerimentoRecebido = requerimentoRecebido;
        desligamento.AcessosRemovidos = acessosRemovidos;
        desligamento.Observacoes = LimparOpcional(observacoes);

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DesligamentoConfirmacaoResultado> ConfirmarAsync(
        int id,
        bool encerrarCobrancasFuturas,
        CancellationToken cancellationToken = default)
    {
        var desligamento = await dbContext.Desligamentos
            .Include(x => x.Aluno)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (desligamento is null)
        {
            return new DesligamentoConfirmacaoResultado { Erro = "Desligamento nao encontrado." };
        }

        if (desligamento.DataConfirmacao.HasValue)
        {
            return new DesligamentoConfirmacaoResultado { Erro = "Desligamento ja confirmado.", AlunoId = desligamento.AlunoId };
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        desligamento.DataConfirmacao = DateOnly.FromDateTime(DateTime.Today);

        if (desligamento.Aluno is not null)
        {
            desligamento.Aluno.Status = StatusAlunoEnum.Desligado;
        }

        var cobrancasCanceladas = 0;
        if (encerrarCobrancasFuturas)
        {
            cobrancasCanceladas = await EncerrarCobrancasFuturasAsync(desligamento.AlunoId, cancellationToken);
        }

        dbContext.HistoricosAlunos.Add(new HistoricoAluno
        {
            AlunoId = desligamento.AlunoId,
            DataEvento = DateTime.Now,
            TipoEvento = "Desligamento",
            Descricao =
                $"Desligamento confirmado. Pendencia: {desligamento.PendenciaFinanceira:C}. Multa: {desligamento.MultaRescisoria:C}. Cobrancas futuras canceladas: {cobrancasCanceladas}."
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new DesligamentoConfirmacaoResultado
        {
            Sucesso = true,
            CobrancasCanceladas = cobrancasCanceladas,
            AlunoId = desligamento.AlunoId
        };
    }

    private async Task<int> EncerrarCobrancasFuturasAsync(int alunoId, CancellationToken cancellationToken)
    {
        var competenciaAtual = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

        var futuras = await dbContext.Mensalidades
            .Where(x => x.AlunoId == alunoId &&
                        x.Competencia > competenciaAtual &&
                        x.Status == StatusMensalidadeEnum.Pendente)
            .ToListAsync(cancellationToken);

        foreach (var mensalidade in futuras)
        {
            mensalidade.Status = StatusMensalidadeEnum.Cancelado;
            mensalidade.Observacoes = AppendObservacao(mensalidade.Observacoes, "Cancelada por desligamento do aluno.");
        }

        if (futuras.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return futuras.Count;
    }

    private static string? LimparOpcional(string? texto)
    {
        var valor = texto?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }

    private static string AppendObservacao(string? baseObservacao, string complemento)
    {
        if (string.IsNullOrWhiteSpace(baseObservacao))
        {
            return complemento;
        }

        return $"{baseObservacao.Trim()} {complemento}";
    }
}
