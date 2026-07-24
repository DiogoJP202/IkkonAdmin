using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface ITurmaQueryService
{
    Task<IReadOnlyList<Turma>> ListarAsync(
        string? busca = null,
        bool? ativa = null,
        CancellationToken cancellationToken = default);

    Task<Turma?> ObterComAlunosAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Aluno>> ListarAlunosVinculaveisAsync(
        int? turmaIdAtual = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNomeAsync(
        string nome,
        int? ignorarTurmaId = null,
        CancellationToken cancellationToken = default);
}
