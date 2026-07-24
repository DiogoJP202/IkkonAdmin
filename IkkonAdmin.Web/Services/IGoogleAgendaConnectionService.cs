namespace IkkonAdmin.Web.Services;

public interface IGoogleAgendaConnectionService
{
    Task<bool> PossuiConexaoOAuthAsync(CancellationToken cancellationToken = default);

    Task<string?> ObterRefreshTokenAtivoAsync(CancellationToken cancellationToken = default);

    Task SubstituirConexaoAtivaAsync(
        string refreshToken,
        string? escopos,
        int? usuarioId,
        CancellationToken cancellationToken = default);

    Task DesconectarOAuthAsync(
        int? usuarioId,
        CancellationToken cancellationToken = default);
}
