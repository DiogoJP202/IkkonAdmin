namespace IkkonAdmin.Tests;

public class QueryCommandArchitectureTests
{
    [Fact]
    public void Controllers_Modulares_UsamQueryServiceParaLeituras()
    {
        var controllersRoot = Path.Combine(FindSourceRoot(), "Controllers");
        var expectedQueries = new Dictionary<string, string>
        {
            ["AdmissoesController.cs"] = "IAdmissaoQueryService",
            ["AlunosController.cs"] = "IAlunoQueryService",
            ["BlogAdminController.cs"] = "IBlogAdminQueryService",
            ["ConfiguracoesController.cs"] = "IUserSettingsQueryService",
            ["DesligamentosController.cs"] = "IDesligamentoQueryService",
            ["FinanceiroController.cs"] = "IFinanceiroQueryService",
            ["GraduacoesController.cs"] = "IGraduacaoQueryService",
            ["HomeController.cs"] = "IDashboardQueryService",
            ["InventarioController.cs"] = "IInventarioQueryService",
            ["PainelAdminController.cs"] = "IAdminPainelQueryService",
            ["TurmasController.cs"] = "ITurmaQueryService"
        };

        foreach (var expected in expectedQueries)
        {
            var source = File.ReadAllText(Path.Combine(controllersRoot, expected.Key));
            Assert.Contains(expected.Value, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void QueryServices_NaoExecutamComandosDeManutencao()
    {
        var servicesRoot = Path.Combine(FindSourceRoot(), "Services");
        var queryFiles = Directory.EnumerateFiles(servicesRoot, "*QueryService.cs");

        foreach (var file in queryFiles.Append(Path.Combine(servicesRoot, "BlogPublicService.cs")))
        {
            var source = File.ReadAllText(file);
            Assert.DoesNotContain("PromoteScheduledPostsAsync", source, StringComparison.Ordinal);
            Assert.DoesNotContain("AtualizarAtrasosAsync", source, StringComparison.Ordinal);
            Assert.DoesNotContain("SaveChangesAsync", source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Dashboard_NaoReintroduzFachadaPuramenteDelegadora()
    {
        var servicesRoot = Path.Combine(FindSourceRoot(), "Services");

        Assert.False(File.Exists(Path.Combine(servicesRoot, "DashboardService.cs")));
        Assert.False(File.Exists(Path.Combine(servicesRoot, "IDashboardService.cs")));
    }

    private static string FindSourceRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "IkkonAdmin.Web");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Diretório IkkonAdmin.Web não encontrado.");
    }
}
