using System.Security.Claims;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task AutenticarAsync_ComCredenciaisValidas_RetornaSessaoRolesPermissoesEAudita()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<UsuarioSistema>();
        var usuario = CriarUsuario(TipoAcessoEnum.Admin, "admin", "senha-segura", hasher);
        var auditLogger = new TestAuditLogger();

        dbContext.UsuariosSistema.Add(usuario);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext, hasher, auditLogger);

        var result = await service.AutenticarAsync(
            " admin ",
            "senha-segura",
            TipoAcessoEnum.Admin,
            "127.0.0.1");

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(usuario.Id, result.Value.Usuario.Id);
        Assert.Contains(AppRoles.Admin, result.Value.Roles);
        Assert.Contains(AppPermissions.AlunosView, result.Value.Permissoes);
        Assert.Equal(new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc), usuario.UltimoLoginUtc);

        var auditEntry = Assert.Single(auditLogger.Entries);
        Assert.Equal("LOGIN_SUCESSO", auditEntry.Acao);
        Assert.Equal(usuario.Id, auditEntry.UsuarioResponsavelId);
        Assert.Equal("127.0.0.1", auditEntry.EnderecoIp);
    }

    [Fact]
    public async Task AutenticarAsync_ComSenhaInvalida_RetornaValidationErrorSemAuditoria()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<UsuarioSistema>();
        var usuario = CriarUsuario(TipoAcessoEnum.Funcionario, "funcionario", "senha-correta", hasher);
        var auditLogger = new TestAuditLogger();

        dbContext.UsuariosSistema.Add(usuario);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext, hasher, auditLogger);

        var result = await service.AutenticarAsync(
            "funcionario",
            "senha-errada",
            TipoAcessoEnum.Funcionario);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        Assert.Null(result.Value);
        Assert.Empty(auditLogger.Entries);
    }

    [Fact]
    public async Task RecarregarSessaoAsync_ComUsuarioInexistente_RetornaNotFound()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.RecarregarSessaoAsync(999);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

    [Fact]
    public void AuthClaimsFactory_CriaClaimsDaSessao()
    {
        var usuario = new UsuarioSistema
        {
            Id = 15,
            Login = "aluno",
            LoginNormalizado = "ALUNO",
            NomeExibicao = "Aluno Teste",
            TipoAcesso = TipoAcessoEnum.Aluno,
            AlunoId = 9,
            Email = "aluno@ikkon.test",
            FotoPerfilUrl = "/uploads/perfil/aluno.jpg"
        };
        var session = new AuthSession(
            usuario,
            [AppRoles.Aluno],
            [AppPermissions.ConfiguracoesView]);

        var principal = AuthClaimsFactory.CriarPrincipal(session);

        Assert.Equal("15", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal("Aluno Teste", principal.Identity?.Name);
        Assert.Equal("9", principal.FindFirstValue(AppClaimTypes.AlunoId));
        Assert.Equal("/uploads/perfil/aluno.jpg", principal.FindFirstValue(AppClaimTypes.FotoPerfilUrl));
        Assert.True(principal.IsInRole(AppRoles.Aluno));
        Assert.Contains(
            principal.Claims,
            x => x.Type == AppClaimTypes.Permissao &&
                 x.Value == AppPermissions.ConfiguracoesView);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AuthService CriarService(
        ApplicationDbContext dbContext,
        IPasswordHasher<UsuarioSistema>? passwordHasher = null,
        TestAuditLogger? auditLogger = null)
    {
        return new AuthService(
            dbContext,
            passwordHasher ?? new PasswordHasher<UsuarioSistema>(),
            new TestClock(),
            auditLogger ?? new TestAuditLogger());
    }

    private static UsuarioSistema CriarUsuario(
        TipoAcessoEnum tipoAcesso,
        string login,
        string senha,
        IPasswordHasher<UsuarioSistema> passwordHasher)
    {
        var usuario = new UsuarioSistema
        {
            Login = login,
            LoginNormalizado = login.ToUpperInvariant(),
            Email = $"{login}@ikkon.test",
            EmailNormalizado = $"{login}@ikkon.test".ToUpperInvariant(),
            NomeExibicao = login,
            TipoAcesso = tipoAcesso,
            Ativo = true
        };

        usuario.SenhaHash = passwordHasher.HashPassword(usuario, senha);
        return usuario;
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow { get; } = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);
        public DateTime Now { get; } = new(2026, 7, 13, 9, 0, 0, DateTimeKind.Local);
        public DateTime Today => Now.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }

    private sealed class TestAuditLogger : IAuditLogger
    {
        public List<AuditLogEntry> Entries { get; } = [];

        public Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }
    }
}
