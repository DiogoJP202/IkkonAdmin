using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IGraduacaoQueryService
{
    Task<IReadOnlyList<Graduacao>> ListarAsync(
        string? busca = null,
        bool? somenteAprovados = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Graduacao>> ListarHistoricoAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Aluno>> ListarAlunosAptosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExameGraduacao>> ListarExamesAsync(CancellationToken cancellationToken = default);
    Task<Graduacao?> ObterDetalhesAsync(int id, CancellationToken cancellationToken = default);
    Task<NivelGraduacaoEnum> ObterNivelAtualAsync(int alunoId, CancellationToken cancellationToken = default);
}
