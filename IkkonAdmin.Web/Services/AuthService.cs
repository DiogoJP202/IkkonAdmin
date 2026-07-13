using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AuthService(ApplicationDbContext dbContext, IPasswordHasher<UsuarioSistema> passwordHasher) : IAuthService
{
    public async Task<AuthResult> AutenticarAsync(
        string loginOuEmail,
        string senha,
        TipoAcessoEnum tipoAcesso,
        string? enderecoIp = null,
        CancellationToken cancellationToken = default)
    {
        var loginNormalizado = Normalizar(loginOuEmail);
        if (string.IsNullOrWhiteSpace(loginNormalizado) || string.IsNullOrWhiteSpace(senha))
        {
            return AuthResult.Falha();
        }

        var usuario = await dbContext.UsuariosSistema
            .FirstOrDefaultAsync(
                x => x.Ativo
                     && x.TipoAcesso == tipoAcesso
                     && (x.LoginNormalizado == loginNormalizado || x.EmailNormalizado == loginNormalizado),
                cancellationToken);

        if (usuario is null)
        {
            return AuthResult.Falha();
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, senha);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return AuthResult.Falha();
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.SenhaHash = passwordHasher.HashPassword(usuario, senha);
        }

        usuario.UltimoLoginUtc = DateTime.UtcNow;

        await dbContext.AuditoriaLogs.AddAsync(new AuditoriaLog
        {
            UsuarioResponsavelId = usuario.Id,
            UsuarioAfetadoId = usuario.Id,
            Acao = "LOGIN_SUCESSO",
            Entidade = nameof(UsuarioSistema),
            EntidadeId = usuario.Id.ToString(),
            Descricao = "Login realizado com sucesso.",
            EnderecoIp = LimparIp(enderecoIp),
            DataEventoUtc = DateTime.UtcNow
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        var roles = await ObterRolesAsync(usuario, cancellationToken);
        var permissoes = await ObterPermissoesAsync(usuario.Id, roles, cancellationToken);

        return AuthResult.Ok(usuario, roles, permissoes);
    }

    public async Task<AuthResult> RecarregarSessaoAsync(
        int usuarioId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .FirstOrDefaultAsync(x => x.Id == usuarioId && x.Ativo, cancellationToken);

        if (usuario is null)
        {
            return AuthResult.Falha();
        }

        var roles = await ObterRolesAsync(usuario, cancellationToken);
        var permissoes = await ObterPermissoesAsync(usuario.Id, roles, cancellationToken);

        return AuthResult.Ok(usuario, roles, permissoes);
    }

    private static string? Normalizar(string? valor)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.Trim().ToUpperInvariant();
    }

    private static string? LimparIp(string? ip)
    {
        var valor = ip?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }

    private async Task<IReadOnlyCollection<string>> ObterRolesAsync(
        UsuarioSistema usuario,
        CancellationToken cancellationToken)
    {
        var roles = await dbContext.UsuariosRoles
            .Where(x => x.UsuarioId == usuario.Id && x.Role != null && x.Role.Ativo)
            .Select(x => x.Role!.Codigo)
            .Distinct()
            .ToListAsync(cancellationToken);

        var rolePadrao = AppRoles.FromTipoAcesso(usuario.TipoAcesso);
        if (!roles.Contains(rolePadrao, StringComparer.OrdinalIgnoreCase))
        {
            roles.Add(rolePadrao);
        }

        return roles
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyCollection<string>> ObterPermissoesAsync(
        int usuarioId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken)
    {
        if (roles.Contains(AppRoles.Admin, StringComparer.OrdinalIgnoreCase))
        {
            return AppPermissions.Todas
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        var permissoesPorRole = await dbContext.RolesPermissoes
            .Where(x => x.Role != null
                        && x.Permissao != null
                        && x.Role.Ativo
                        && x.Permissao.Ativo
                        && roles.Contains(x.Role.Codigo))
            .Select(x => x.Permissao!.Codigo)
            .ToListAsync(cancellationToken);

        var permissoesDiretas = await dbContext.UsuariosPermissoes
            .Where(x => x.UsuarioId == usuarioId && x.Permissao != null && x.Permissao.Ativo)
            .Select(x => x.Permissao!.Codigo)
            .ToListAsync(cancellationToken);

        return permissoesPorRole
            .Concat(permissoesDiretas)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
