using System.Text.RegularExpressions;
using IkkonAdmin.Web.Helpers;

namespace IkkonAdmin.Tests;

public class AlunoPortalFrontendTests
{
    private static readonly string[] PortalViews =
    [
        "Index.cshtml",
        "Perfil.cshtml",
        "Turmas.cshtml",
        "Aulas.cshtml",
        "Frequencia.cshtml",
        "Financeiro.cshtml",
        "Documentos.cshtml",
        "Comunicados.cshtml",
        "Eventos.cshtml",
        "Conquistas.cshtml"
    ];

    [Fact]
    public void PortalPages_UseSharedPageIntroductionAndThreeLanguages()
    {
        foreach (var viewName in PortalViews)
        {
            var view = ReadRepoFile("IkkonAdmin.Web", "Views", "AlunoArea", viewName);

            Assert.Contains("_AlunoPageHeader", view, StringComparison.Ordinal);
            Assert.Contains("I18n[", view, StringComparison.Ordinal);
            Assert.DoesNotMatch(
                new Regex(
                    """I18n\[\s*"[^"\r\n]*"\s*,\s*"[^"\r\n]*"\s*\]""",
                    RegexOptions.CultureInvariant),
                view);
        }
    }

    [Fact]
    public void PortalShell_HasNoHeadingAndExposesAccessibleMobileNavigation()
    {
        var layout = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Shared",
            "_AlunoLayout.cshtml");
        var pageHeader = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Shared",
            "_AlunoPageHeader.cshtml");

        Assert.DoesNotContain("<h1", layout, StringComparison.OrdinalIgnoreCase);
        Assert.Single(
            Regex.Matches(pageHeader, "<h1", RegexOptions.IgnoreCase).Cast<Match>());
        Assert.Contains("id=\"alunoPortalSidebar\"", layout, StringComparison.Ordinal);
        Assert.Contains("aria-controls=\"alunoPortalSidebar\"", layout, StringComparison.Ordinal);
        Assert.Contains("data-close-label", layout, StringComparison.Ordinal);
        Assert.Contains("ja-JP", layout, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Index.cshtml")]
    [InlineData("Aulas.cshtml")]
    [InlineData("Frequencia.cshtml")]
    [InlineData("Financeiro.cshtml")]
    public void DataHeavyPages_ExposeResponsiveTableLabels(string viewName)
    {
        var view = ReadRepoFile("IkkonAdmin.Web", "Views", "AlunoArea", viewName);

        Assert.Contains("aluno-portal-responsive-table", view, StringComparison.Ordinal);
        Assert.Contains("data-label=", view, StringComparison.Ordinal);
    }

    [Fact]
    public void ExistingStudentActions_RemainProtectedAndAvailable()
    {
        var documentos = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "AlunoArea",
            "Documentos.cshtml");
        var comunicados = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "AlunoArea",
            "Comunicados.cshtml");

        Assert.Contains("asp-action=\"EnviarDocumento\"", documentos, StringComparison.Ordinal);
        Assert.Contains("Html.AntiForgeryToken", documentos, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"BaixarDocumento\"", documentos, StringComparison.Ordinal);
        Assert.Contains("asp-action=\"MarcarComunicadoLido\"", comunicados, StringComparison.Ordinal);
        Assert.Contains("Html.AntiForgeryToken", comunicados, StringComparison.Ordinal);
    }

    [Fact]
    public void SettingsFeedback_IsLocalizedAndStudentStylingIsIsolated()
    {
        var view = ReadRepoFile(
            "IkkonAdmin.Web",
            "Views",
            "Configuracoes",
            "Index.cshtml");
        var script = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "js",
            "configuracoes.js");
        var alunoAccountCss = ReadRepoFile(
            "IkkonAdmin.Web",
            "wwwroot",
            "css",
            "ikkon-aluno-account.css");

        Assert.Contains("data-save-success", view, StringComparison.Ordinal);
        Assert.Contains("data-network-error", view, StringComparison.Ordinal);
        Assert.Contains("feedback?.dataset.saveSuccess", script, StringComparison.Ordinal);
        Assert.Contains(".aluno-portal-body .configuracoes-v2-page", alunoAccountCss, StringComparison.Ordinal);
        Assert.DoesNotContain("body.admin-theme", alunoAccountCss, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("Ativo", "success")]
    [InlineData("Pago", "success")]
    [InlineData("Falta", "danger")]
    [InlineData("Recusado", "danger")]
    [InlineData("Pendente", "warning")]
    [InlineData("FaltaJustificada", "warning")]
    [InlineData("Agendada", "info")]
    [InlineData("EmAdmissao", "info")]
    [InlineData("Cancelada", "neutral")]
    [InlineData(null, "neutral")]
    public void StatusTone_ReturnsSemanticPresentation(string? status, string expected)
    {
        Assert.Equal(expected, AlunoPortalPresentation.StatusTone(status));
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
            "Não foi possível localizar a raiz do repositório.");
    }
}
