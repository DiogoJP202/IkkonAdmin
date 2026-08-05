using IkkonAdmin.Web.Helpers;

namespace IkkonAdmin.Tests;

public class CssArchitectureTests
{
    [Fact]
    public void PublicLayout_LoadsDedicatedFoundationInsteadOfGlobalSiteCss()
    {
        var layout = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Shared",
            "_PublicLayout.cshtml");

        Assert.Contains(
            "~/css/ikkon-public-foundation.css",
            layout,
            StringComparison.Ordinal);
        Assert.DoesNotContain("~/css/site.css", layout, StringComparison.Ordinal);
    }

    [Fact]
    public void LegacyGlobalSiteCss_IsNotUsedOrKept()
    {
        var repositoryRoot = FindRepositoryRoot();
        var legacyCssPath = Path.Combine(
            repositoryRoot,
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "site.css");
        var viewsDirectory = Path.Combine(repositoryRoot, "IkkonAdmin.Web", "Views");
        var layoutReferences = Directory
            .EnumerateFiles(viewsDirectory, "*.cshtml", SearchOption.AllDirectories)
            .Select(File.ReadAllText);

        Assert.False(File.Exists(legacyCssPath));
        Assert.All(
            layoutReferences,
            view => Assert.DoesNotContain("~/css/site.css", view, StringComparison.Ordinal));
    }

    [Fact]
    public void LegacyMonolithicAdminCss_IsNotUsedOrKept()
    {
        var repositoryRoot = FindRepositoryRoot();
        var legacyAdminCssPath = Path.Combine(
            repositoryRoot,
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-admin.css");
        var viewsDirectory = Path.Combine(repositoryRoot, "IkkonAdmin.Web", "Views");
        var viewContents = Directory
            .EnumerateFiles(viewsDirectory, "*.cshtml", SearchOption.AllDirectories)
            .Select(File.ReadAllText);

        Assert.False(File.Exists(legacyAdminCssPath));
        Assert.All(
            viewContents,
            view => Assert.DoesNotContain(
                "~/css/ikkon-admin.css",
                view,
                StringComparison.Ordinal));
    }

    [Fact]
    public void PublicFoundation_KeepsInstitutionalAndBlogRules()
    {
        var foundationCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-public-foundation.css");

        Assert.Contains(
            "/* Institucional landing */",
            foundationCss,
            StringComparison.Ordinal);
        Assert.Contains(
            "/* Public blog */",
            foundationCss,
            StringComparison.Ordinal);
        Assert.Contains(".institucional-page", foundationCss, StringComparison.Ordinal);
        Assert.Contains(".public-blog-page", foundationCss, StringComparison.Ordinal);
    }

    [Fact]
    public void InternalLayouts_LoadOnlyTheirRequiredCssModules()
    {
        var adminLayout = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Shared",
            "_Layout.cshtml");
        var authLayout = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Shared",
            "_AuthLayout.cshtml");
        var alunoLayout = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Shared",
            "_AlunoLayout.cshtml");

        AssertCssLinks(
            adminLayout,
            "ikkon-internal-foundation.css",
            "ikkon-admin-core.css",
            "ikkon-internal-themes.css");
        Assert.Contains("AdminCssModuleResolver.Resolve", adminLayout, StringComparison.Ordinal);
        Assert.Contains("~/css/@cssModule", adminLayout, StringComparison.Ordinal);
        Assert.DoesNotContain("ikkon-auth.css", adminLayout, StringComparison.Ordinal);
        Assert.DoesNotContain("ikkon-aluno.css", adminLayout, StringComparison.Ordinal);

        AssertCssLinks(
            authLayout,
            "ikkon-internal-foundation.css",
            "ikkon-auth.css");
        Assert.DoesNotContain("ikkon-admin.css", authLayout, StringComparison.Ordinal);
        Assert.DoesNotContain("ikkon-aluno.css", authLayout, StringComparison.Ordinal);
        Assert.DoesNotContain("ikkon-account.css", authLayout, StringComparison.Ordinal);
        Assert.DoesNotContain("ikkon-internal-themes.css", authLayout, StringComparison.Ordinal);

        AssertCssLinks(
            alunoLayout,
            "ikkon-tokens.css",
            "ikkon-internal-foundation.css",
            "ikkon-aluno.css",
            "ikkon-account.css",
            "ikkon-aluno-account.css",
            "ikkon-internal-themes.css");
        Assert.Contains("isAccountPage", alunoLayout, StringComparison.Ordinal);
        Assert.Contains("ja-JP", alunoLayout, StringComparison.Ordinal);
        Assert.DoesNotContain("ikkon-auth.css", alunoLayout, StringComparison.Ordinal);
        Assert.DoesNotContain("ikkon-admin-core.css", alunoLayout, StringComparison.Ordinal);
    }

    [Fact]
    public void InternalCssModules_KeepClearResponsibilityBoundaries()
    {
        var authCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-auth.css");
        var alunoCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-aluno.css");
        var accountCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-account.css");
        var alunoAccountCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-aluno-account.css");
        var adminCoreCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-admin-core.css");
        var dashboardCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-admin-dashboard.css");
        var configuracoesCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-admin-configuracoes.css");
        var themeCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-internal-themes.css");

        Assert.Contains(".auth-page", authCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".aluno-portal-", authCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".admin-shell", authCss, StringComparison.Ordinal);

        Assert.Contains(".aluno-portal-shell", alunoCss, StringComparison.Ordinal);
        Assert.Contains(".aluno-portal-page-header", alunoCss, StringComparison.Ordinal);
        Assert.Contains(".aluno-portal-responsive-table", alunoCss, StringComparison.Ordinal);
        Assert.Contains("@media (max-width: 767.98px)", alunoCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".auth-page", alunoCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".admin-shell", alunoCss, StringComparison.Ordinal);

        Assert.Contains(".configuracoes-v2-page", accountCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".auth-page", accountCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".aluno-portal-", accountCss, StringComparison.Ordinal);

        Assert.Contains(
            ".aluno-portal-body .configuracoes-v2-page",
            alunoAccountCss,
            StringComparison.Ordinal);
        Assert.Contains(
            ".aluno-portal-body .configuracoes-v2-table",
            alunoAccountCss,
            StringComparison.Ordinal);
        Assert.DoesNotContain(".admin-shell", alunoAccountCss, StringComparison.Ordinal);

        Assert.Contains(".admin-shell", adminCoreCss, StringComparison.Ordinal);
        Assert.Contains("@media (max-width: 768px)", adminCoreCss, StringComparison.Ordinal);
        Assert.Contains("body.admin-sidebar-open .admin-sidebar", adminCoreCss, StringComparison.Ordinal);
        Assert.Contains("@keyframes alunos-fade-up", adminCoreCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".dashboard-v2-page", adminCoreCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".auth-page", adminCoreCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".aluno-portal-", adminCoreCss, StringComparison.Ordinal);

        Assert.Contains(".dashboard-v2-page", dashboardCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".alunos-v2-page", dashboardCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".configuracoes-v2-page", dashboardCss, StringComparison.Ordinal);

        Assert.Contains(".configuracoes-page", configuracoesCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".admin-shell", configuracoesCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".alunos-", configuracoesCss, StringComparison.Ordinal);
        Assert.DoesNotContain(".financeiro-", configuracoesCss, StringComparison.Ordinal);

        Assert.Contains("body.admin-theme-dark", themeCss, StringComparison.Ordinal);
        Assert.Contains("body.aluno-theme-dark", themeCss, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminCssModuleResolver_MapsControllersToExistingModules()
    {
        var expectedModules = new Dictionary<string, string[]>
        {
            ["Home"] = ["ikkon-admin-dashboard.css"],
            ["Alunos"] = ["ikkon-admin-alunos.css"],
            ["Turmas"] = ["ikkon-admin-turmas.css"],
            ["Financeiro"] = ["ikkon-admin-financeiro.css"],
            ["Admissoes"] = ["ikkon-admin-admissoes.css"],
            ["Desligamentos"] = ["ikkon-admin-desligamentos.css"],
            ["Graduacoes"] = ["ikkon-admin-graduacoes.css"],
            ["GoogleAgenda"] =
            [
                "ikkon-admin-resources.css",
                "ikkon-admin-agenda.css"
            ],
            ["Inventario"] =
            [
                "ikkon-admin-resources.css",
                "ikkon-admin-inventario.css"
            ],
            ["PainelAdmin"] = ["ikkon-admin-panel.css"],
            ["AreaAlunoAdmin"] = ["ikkon-admin-panel.css"],
            ["BlogAdmin"] =
            [
                "ikkon-admin-panel.css",
                "ikkon-admin-blog.css"
            ],
            ["BlogCategorias"] =
            [
                "ikkon-admin-panel.css",
                "ikkon-admin-blog.css"
            ],
            ["Configuracoes"] =
            [
                "ikkon-admin-configuracoes.css",
                "ikkon-account.css"
            ]
        };
        var repositoryRoot = FindRepositoryRoot();
        var cssDirectory = Path.Combine(
            repositoryRoot,
            "IkkonAdmin.Web",
            "wwwroot",
            "css");

        foreach (var (controller, expected) in expectedModules)
        {
            var actual = AdminCssModuleResolver.Resolve(controller);

            Assert.Equal(expected, actual);
            Assert.Equal(actual.Count, actual.Distinct(StringComparer.Ordinal).Count());
            Assert.All(
                actual,
                fileName => Assert.True(
                    File.Exists(Path.Combine(cssDirectory, fileName)),
                    $"O módulo CSS {fileName} não existe."));
        }

        Assert.Empty(AdminCssModuleResolver.Resolve(null));
        Assert.Empty(AdminCssModuleResolver.Resolve(""));
        Assert.Empty(AdminCssModuleResolver.Resolve("ControllerDesconhecido"));
        Assert.Equal(
            expectedModules["Alunos"],
            AdminCssModuleResolver.Resolve("alunos"));
    }

    private static void AssertCssLinks(string layout, params string[] fileNames)
    {
        var previousIndex = -1;
        foreach (var fileName in fileNames)
        {
            var currentIndex = layout.IndexOf(fileName, StringComparison.Ordinal);
            Assert.True(currentIndex >= 0, $"O layout não referencia {fileName}.");
            Assert.True(
                currentIndex > previousIndex,
                $"{fileName} está fora da ordem de cascata esperada.");
            previousIndex = currentIndex;
        }
    }

    private static string ReadRepoFile(params string[] pathSegments)
    {
        var repositoryRoot = FindRepositoryRoot();
        return File.ReadAllText(Path.Combine([repositoryRoot, .. pathSegments]));
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "IkkonAdmin.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Não foi possível localizar a raiz do repositório para validar a arquitetura CSS.");
    }
}
