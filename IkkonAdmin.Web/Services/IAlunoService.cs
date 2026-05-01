using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IAlunoService
{
    Task<(IReadOnlyList<Aluno> Itens, int TotalRegistros)> ListarAsync(
        string? busca = null,
        StatusAlunoEnum? status = null,
        int? turmaId = null,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Turma>> ListarTurmasAsync(CancellationToken cancellationToken = default);
    Task<Aluno?> ObterParaEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<Aluno?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteCpfAsync(string cpf, int? ignorarAlunoId = null, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Aluno aluno, CancellationToken cancellationToken = default);
    Task SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
    Task<bool> AlterarStatusAsync(int id, StatusAlunoEnum status, CancellationToken cancellationToken = default);
}
