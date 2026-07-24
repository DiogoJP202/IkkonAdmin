using System.Security.Claims;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Authorization;

namespace IkkonAdmin.Tests;

public class AuthorizationPolicyRegistrationTests
{
    [Fact]
    public void AddIkkonPolicies_RegistraPoliciesPrincipais()
    {
        var options = new AuthorizationOptions();

        options.AddIkkonPolicies();

        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.Funcionario));
        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.Aluno));
        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.Admin));
        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.BlogPublish));
        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.DocumentosApprove));
        Assert.NotNull(options.GetPolicy(AuthorizationPolicies.AdminGerenciarSistema));
    }

    [Fact]
    public void HasFuncionarioPermission_AdminSempreAcessa()
    {
        var principal = CriarPrincipal(AppRoles.Admin);

        var autorizado = AppPermissionEvaluator.HasFuncionarioPermission(
            principal,
            [AppPermissions.FinanceiroView]);

        Assert.True(autorizado);
    }

    [Fact]
    public void HasFuncionarioPermission_FuncionarioPrecisaTerPermissao()
    {
        var principal = CriarPrincipal(AppRoles.Funcionario, AppPermissions.AlunosView);

        Assert.True(AppPermissionEvaluator.HasFuncionarioPermission(principal, [AppPermissions.AlunosView]));
        Assert.False(AppPermissionEvaluator.HasFuncionarioPermission(principal, [AppPermissions.FinanceiroView]));
    }

    [Fact]
    public void HasFuncionarioPermission_AceitaPermissaoAlternativaDeManage()
    {
        var principal = CriarPrincipal(AppRoles.Funcionario, AppPermissions.InventarioManage);

        var autorizado = AppPermissionEvaluator.HasFuncionarioPermission(
            principal,
            [AppPermissions.InventarioView, AppPermissions.InventarioManage]);

        Assert.True(autorizado);
    }

    [Fact]
    public void HasAuthenticatedPermission_AceitaAlunoComPermissaoDeConfiguracoes()
    {
        var principal = CriarPrincipal(AppRoles.Aluno, AppPermissions.ConfiguracoesView);

        var autorizado = AppPermissionEvaluator.HasAuthenticatedPermission(
            principal,
            [AppPermissions.ConfiguracoesView]);

        Assert.True(autorizado);
    }

    private static ClaimsPrincipal CriarPrincipal(string role, params string[] permissoes)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, role)
        };

        claims.AddRange(permissoes.Select(permissao => new Claim(AppClaimTypes.Permissao, permissao)));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType: "Test"));
    }
}
