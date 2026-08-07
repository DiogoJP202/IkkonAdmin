using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IkkonAdmin.Web.Data;

public sealed class InitialAdminBootstrapOptions
{
    public const string SectionName = "InitialAdminBootstrap";

    public string? Login { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? Password { get; set; }
}

public sealed class InitialAdminBootstrap(
    ApplicationDbContext dbContext,
    IPasswordHasher<UsuarioSistema> passwordHasher,
    IOptions<InitialAdminBootstrapOptions> options,
    ILogger<InitialAdminBootstrap> logger)
{
    public void CreateOnlyWhenNoAdminExists()
    {
        if (dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .Any(x => x.TipoAcesso == TipoAcessoEnum.Admin && !x.Excluido))
        {
            return;
        }

        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Login) &&
            string.IsNullOrWhiteSpace(settings.Password) &&
            string.IsNullOrWhiteSpace(settings.Email))
        {
            logger.LogInformation("Bootstrap administrativo não solicitado.");
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Login) ||
            string.IsNullOrWhiteSpace(settings.Password) ||
            settings.Password.Length < 12)
        {
            throw new InvalidOperationException(
                "InitialAdminBootstrap exige Login e Password com pelo menos 12 caracteres.");
        }

        var normalizedLogin = settings.Login.Trim().ToUpperInvariant();
        if (dbContext.UsuariosSistema.IgnoreQueryFilters().Any(x => x.LoginNormalizado == normalizedLogin))
        {
            throw new InvalidOperationException("O login informado no bootstrap inicial já está em uso.");
        }

        var admin = new UsuarioSistema
        {
            Login = settings.Login.Trim(),
            LoginNormalizado = normalizedLogin,
            Email = NormalizeOptional(settings.Email),
            EmailNormalizado = NormalizeOptional(settings.Email)?.ToUpperInvariant(),
            NomeExibicao = string.IsNullOrWhiteSpace(settings.DisplayName)
                ? "Administrador inicial"
                : settings.DisplayName.Trim(),
            TipoAcesso = TipoAcessoEnum.Admin,
            Ativo = true,
            DataCriacaoUtc = DateTime.UtcNow
        };
        admin.SenhaHash = passwordHasher.HashPassword(admin, settings.Password);
        dbContext.UsuariosSistema.Add(admin);
        dbContext.SaveChanges();

        SeedData.InitializeStructural(dbContext);
        logger.LogWarning(
            "Administrador inicial {Login} criado. Remova imediatamente as variáveis de bootstrap.",
            admin.Login);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
