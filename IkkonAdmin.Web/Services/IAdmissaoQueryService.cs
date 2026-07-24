using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IAdmissaoQueryService
{
    Task<IReadOnlyList<Admissao>> ListarAsync(
        string? busca = null,
        StatusAdmissaoEnum? status = null,
        CancellationToken cancellationToken = default);

    Task<Admissao?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Turma>> ListarTurmasAsync(CancellationToken cancellationToken = default);
}
