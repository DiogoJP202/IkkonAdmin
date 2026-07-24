using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class AdminPainelQueryServiceTests
{
    [Fact]
    public async Task ListarUsuariosAsync_AplicaFiltrosEIncluiCargoAtual()
    {
        await using var dbContext = CriarDbContext();
        var adminRole = CriarRole(AppRoles.Admin, "Administrador", TipoAcessoEnum.Admin);
        var funcionarioRole = CriarRole(AppRoles.Funcionario, "Funcionario", TipoAcessoEnum.Funcionario);
        var admin = CriarUsuario("ana.admin", "Ana Admin", TipoAcessoEnum.Admin);
        var funcionario = CriarUsuario("ana.func", "Ana Funcionario", TipoAcessoEnum.Funcionario);
        var excluido = CriarUsuario("ana.old", "Ana Arquivada", TipoAcessoEnum.Funcionario, excluido: true);

        dbContext.AddRange(adminRole, funcionarioRole, admin, funcionario, excluido);
        await dbContext.SaveChangesAsync();

        dbContext.UsuariosRoles.AddRange(
            new UsuarioRole { UsuarioId = admin.Id, RoleId = adminRole.Id, DataVinculoUtc = DateTime.UtcNow.AddDays(-2) },
            new UsuarioRole { UsuarioId = funcionario.Id, RoleId = funcionarioRole.Id, DataVinculoUtc = DateTime.UtcNow.AddDays(-1) });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ListarUsuariosAsync(
            "Ana",
            TipoAcessoEnum.Funcionario,
            true,
            incluirExcluidos: false,
            pagina: 1,
            tamanhoPagina: 20);

        var usuario = Assert.Single(resultado.Usuarios);
        Assert.Equal(funcionario.Id, usuario.Id);
        Assert.Equal(funcionarioRole.Id, usuario.RoleId);
        Assert.Equal("Funcionario", usuario.RoleNome);
        Assert.Equal(1, resultado.TotalRegistros);
    }

    [Fact]
    public async Task ObterAcessosAsync_SeparaPermissoesHerdadasEDiretas()
    {
        await using var dbContext = CriarDbContext();
        var role = CriarRole(AppRoles.Funcionario, "Funcionario", TipoAcessoEnum.Funcionario);
        var usuario = CriarUsuario("operador", "Operador", TipoAcessoEnum.Funcionario);
        var permissaoHerdada = CriarPermissao(AppPermissions.FinanceiroView, "Financeiro");
        var permissaoDireta = CriarPermissao(AppPermissions.BlogEdit, "Editar blog");

        dbContext.AddRange(role, usuario, permissaoHerdada, permissaoDireta);
        await dbContext.SaveChangesAsync();

        dbContext.UsuariosRoles.Add(new UsuarioRole
        {
            UsuarioId = usuario.Id,
            RoleId = role.Id,
            DataVinculoUtc = DateTime.UtcNow
        });
        dbContext.RolesPermissoes.Add(new RolePermissao
        {
            RoleId = role.Id,
            PermissaoId = permissaoHerdada.Id,
            DataVinculoUtc = DateTime.UtcNow
        });
        dbContext.UsuariosPermissoes.Add(new UsuarioPermissao
        {
            UsuarioId = usuario.Id,
            PermissaoId = permissaoDireta.Id,
            DataConcessaoUtc = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var acessos = await service.ObterAcessosAsync(usuario.Id);

        Assert.NotNull(acessos);
        Assert.Equal(role.Id, acessos.RoleSelecionadaId);

        var herdada = Assert.Single(acessos.PermissoesDisponiveis, x => x.Codigo == AppPermissions.FinanceiroView);
        Assert.True(herdada.HerdadaDaRole);
        Assert.False(herdada.Concedida);

        var direta = Assert.Single(acessos.PermissoesDisponiveis, x => x.Codigo == AppPermissions.BlogEdit);
        Assert.False(direta.HerdadaDaRole);
        Assert.True(direta.Concedida);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static AdminPainelQueryService CriarService(ApplicationDbContext dbContext)
    {
        return new AdminPainelQueryService(dbContext, new TestClock());
    }

    private static UsuarioSistema CriarUsuario(
        string login,
        string nome,
        TipoAcessoEnum tipoAcesso,
        bool ativo = true,
        bool excluido = false)
    {
        return new UsuarioSistema
        {
            Login = login,
            LoginNormalizado = login.ToUpperInvariant(),
            Email = $"{login}@ikkon.local",
            EmailNormalizado = $"{login}@ikkon.local".ToUpperInvariant(),
            NomeExibicao = nome,
            SenhaHash = "hash",
            TipoAcesso = tipoAcesso,
            Ativo = ativo,
            Excluido = excluido
        };
    }

    private static RoleSistema CriarRole(string codigo, string nome, TipoAcessoEnum tipoAcesso)
    {
        return new RoleSistema
        {
            Codigo = codigo,
            Nome = nome,
            TipoAcesso = tipoAcesso,
            Ativo = true,
            IsSistema = true
        };
    }

    private static PermissaoSistema CriarPermissao(string codigo, string nome)
    {
        return new PermissaoSistema
        {
            Codigo = codigo,
            Nome = nome,
            Ativo = true,
            IsSistema = true
        };
    }

    private sealed class TestClock : IClock
    {
        private static readonly DateTime FixedUtcNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedUtcNow.ToLocalTime();
        public DateTime Today => FixedUtcNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
