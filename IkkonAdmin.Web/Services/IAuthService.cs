using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IAuthService
{
    Task<OperationResult<AuthSession>> AutenticarAsync(
        string loginOuEmail,
        string senha,
        TipoAcessoEnum tipoAcesso,
        string? enderecoIp = null,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AuthSession>> RecarregarSessaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default);
}

public sealed record AuthSession(
    UsuarioSistema Usuario,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissoes);
