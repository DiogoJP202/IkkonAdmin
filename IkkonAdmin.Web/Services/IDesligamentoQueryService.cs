using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IDesligamentoQueryService
{
    Task<IReadOnlyList<Desligamento>> ListarAsync(
        string? busca = null,
        bool? confirmado = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aluno>> ListarAlunosElegiveisAsync(CancellationToken cancellationToken = default);
    Task<Desligamento?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default);
    Task<decimal> CalcularPendenciasAsync(int alunoId, CancellationToken cancellationToken = default);
}
