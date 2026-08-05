namespace IkkonAdmin.Tests;

public class OperationResultArchitectureTests
{
    [Fact]
    public void Frontend_NaoDeclaraResultadosLegados()
    {
        var sourceRoot = FindSourceRoot();
        var forbiddenNames = new[]
        {
            "BlogOperationResult",
            "AdminOperationResult",
            "UserSettingsOperationResult"
        };

        var files = Directory.EnumerateFiles(sourceRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}") &&
                           !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"));

        foreach (var file in files)
        {
            var source = File.ReadAllText(file);
            foreach (var forbiddenName in forbiddenNames)
            {
                Assert.DoesNotContain(forbiddenName, source, StringComparison.Ordinal);
            }
        }
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
