namespace IkkonAdmin.Web.Helpers;

public static class AdminCssModuleResolver
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> ModulesByController =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase)
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

    public static IReadOnlyList<string> Resolve(string? controllerName)
    {
        if (string.IsNullOrWhiteSpace(controllerName))
        {
            return [];
        }

        return ModulesByController.TryGetValue(controllerName, out var modules)
            ? modules
            : [];
    }
}
