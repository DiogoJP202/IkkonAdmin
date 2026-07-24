using System.Security.Claims;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;

namespace IkkonAdmin.Tests;

public class BlogPostActionAuthorizerTests
{
    private readonly BlogPostActionAuthorizer authorizer = new();

    [Fact]
    public void CanSubmit_BloqueiaPublicacaoSemPermissao()
    {
        var principal = CriarPrincipal(AppRoles.Funcionario, AppPermissions.BlogCreate);
        var model = new BlogPostFormViewModel { SubmissionAction = "publish" };

        Assert.False(authorizer.CanSubmit(principal, model));
    }

    [Fact]
    public void CanSubmit_PermitePublicacaoComPermissao()
    {
        var principal = CriarPrincipal(AppRoles.Funcionario, AppPermissions.BlogPublish);
        var model = new BlogPostFormViewModel { SubmissionAction = "publish" };

        Assert.True(authorizer.CanSubmit(principal, model));
    }

    [Fact]
    public void CanSubmit_BloqueiaDestaqueSemPermissao()
    {
        var principal = CriarPrincipal(AppRoles.Funcionario, AppPermissions.BlogPublish);
        var model = new BlogPostFormViewModel
        {
            SubmissionAction = "publish",
            IsFeatured = true
        };

        Assert.False(authorizer.CanSubmit(principal, model));
    }

    [Fact]
    public void CanUploadContentImage_PermiteCriadorOuEditor()
    {
        var principal = CriarPrincipal(AppRoles.Funcionario, AppPermissions.BlogEdit);

        Assert.True(authorizer.CanUploadContentImage(principal));
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
