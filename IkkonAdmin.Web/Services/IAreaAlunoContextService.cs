namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoContextService
{
    Task<AreaAlunoPortalContexto?> ObterContextoAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<int?> ObterAlunoIdVinculadoAsync(int usuarioId, CancellationToken cancellationToken = default);
}
