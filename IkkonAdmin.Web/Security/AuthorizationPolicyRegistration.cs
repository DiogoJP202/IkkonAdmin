using Microsoft.AspNetCore.Authorization;

namespace IkkonAdmin.Web.Security;

public static class AuthorizationPolicyRegistration
{
    public static IReadOnlyCollection<PermissionPolicyDefinition> PermissionPolicies { get; } =
    [
        Funcionario(AuthorizationPolicies.DashboardView, AppPermissions.DashboardView),

        Funcionario(AuthorizationPolicies.AlunosView, AppPermissions.AlunosView),
        Funcionario(AuthorizationPolicies.AlunosCreate, AppPermissions.AlunosCreate),
        Funcionario(AuthorizationPolicies.AlunosEdit, AppPermissions.AlunosEdit),
        Funcionario(AuthorizationPolicies.AlunosDelete, AppPermissions.AlunosDelete),

        Funcionario(AuthorizationPolicies.TurmasView, AppPermissions.TurmasView),
        Funcionario(AuthorizationPolicies.TurmasCreate, AppPermissions.TurmasCreate),
        Funcionario(AuthorizationPolicies.TurmasEdit, AppPermissions.TurmasEdit),
        Funcionario(AuthorizationPolicies.TurmasDelete, AppPermissions.TurmasDelete),

        Funcionario(AuthorizationPolicies.FinanceiroView, AppPermissions.FinanceiroView),
        Funcionario(AuthorizationPolicies.FinanceiroCreate, AppPermissions.FinanceiroCreate),
        Funcionario(AuthorizationPolicies.FinanceiroEdit, AppPermissions.FinanceiroEdit),
        Funcionario(AuthorizationPolicies.FinanceiroDelete, AppPermissions.FinanceiroDelete),

        Funcionario(AuthorizationPolicies.AdmissoesView, AppPermissions.AdmissoesView),
        Funcionario(AuthorizationPolicies.AdmissoesCreate, AppPermissions.AdmissoesCreate),
        Funcionario(AuthorizationPolicies.AdmissoesEdit, AppPermissions.AdmissoesEdit),
        Funcionario(AuthorizationPolicies.AdmissoesDelete, AppPermissions.AdmissoesDelete),

        Funcionario(AuthorizationPolicies.DesligamentosView, AppPermissions.DesligamentosView),
        Funcionario(AuthorizationPolicies.DesligamentosCreate, AppPermissions.DesligamentosCreate),
        Funcionario(AuthorizationPolicies.DesligamentosEdit, AppPermissions.DesligamentosEdit),
        Funcionario(AuthorizationPolicies.DesligamentosDelete, AppPermissions.DesligamentosDelete),

        Funcionario(AuthorizationPolicies.GraduacoesView, AppPermissions.GraduacoesView),
        Funcionario(AuthorizationPolicies.GraduacoesCreate, AppPermissions.GraduacoesCreate),
        Funcionario(AuthorizationPolicies.GraduacoesEdit, AppPermissions.GraduacoesEdit),
        Funcionario(AuthorizationPolicies.GraduacoesDelete, AppPermissions.GraduacoesDelete),

        Authenticated(AuthorizationPolicies.ConfiguracoesView, AppPermissions.ConfiguracoesView),
        Authenticated(AuthorizationPolicies.ConfiguracoesEdit, AppPermissions.ConfiguracoesEdit),

        Funcionario(AuthorizationPolicies.GoogleAgendaView, AppPermissions.GoogleAgendaView, AppPermissions.GoogleAgendaManage),
        Funcionario(AuthorizationPolicies.GoogleAgendaCreate, AppPermissions.GoogleAgendaCreate, AppPermissions.GoogleAgendaManage),
        Funcionario(AuthorizationPolicies.GoogleAgendaEdit, AppPermissions.GoogleAgendaEdit, AppPermissions.GoogleAgendaManage),
        Funcionario(AuthorizationPolicies.GoogleAgendaDelete, AppPermissions.GoogleAgendaDelete, AppPermissions.GoogleAgendaManage),
        Funcionario(AuthorizationPolicies.GoogleAgendaManage, AppPermissions.GoogleAgendaManage),

        Funcionario(AuthorizationPolicies.InventarioView, AppPermissions.InventarioView, AppPermissions.InventarioManage),
        Funcionario(AuthorizationPolicies.InventarioCreate, AppPermissions.InventarioCreate, AppPermissions.InventarioManage),
        Funcionario(AuthorizationPolicies.InventarioEdit, AppPermissions.InventarioEdit, AppPermissions.InventarioManage),
        Funcionario(AuthorizationPolicies.InventarioDelete, AppPermissions.InventarioDelete, AppPermissions.InventarioManage),
        Funcionario(AuthorizationPolicies.InventarioManage, AppPermissions.InventarioManage),

        Funcionario(AuthorizationPolicies.BlogView, AppPermissions.BlogView),
        Funcionario(AuthorizationPolicies.BlogCreate, AppPermissions.BlogCreate),
        Funcionario(AuthorizationPolicies.BlogEdit, AppPermissions.BlogEdit),
        Funcionario(AuthorizationPolicies.BlogPublish, AppPermissions.BlogPublish),
        Funcionario(AuthorizationPolicies.BlogArchive, AppPermissions.BlogArchive),
        Funcionario(AuthorizationPolicies.BlogDelete, AppPermissions.BlogDelete),
        Funcionario(AuthorizationPolicies.BlogFeature, AppPermissions.BlogFeature),
        Funcionario(AuthorizationPolicies.BlogCategoryManage, AppPermissions.BlogCategoryManage),
        Funcionario(AuthorizationPolicies.BlogTagManage, AppPermissions.BlogTagManage),

        Funcionario(AuthorizationPolicies.AreaAlunoView, AppPermissions.AreaAlunoView),
        Funcionario(AuthorizationPolicies.AreaAlunoManage, AppPermissions.AreaAlunoManage),
        Funcionario(AuthorizationPolicies.FrequenciaView, AppPermissions.FrequenciaView),
        Funcionario(AuthorizationPolicies.FrequenciaCreate, AppPermissions.FrequenciaCreate),
        Funcionario(AuthorizationPolicies.FrequenciaEdit, AppPermissions.FrequenciaEdit),
        Funcionario(AuthorizationPolicies.DocumentosView, AppPermissions.DocumentosView),
        Funcionario(AuthorizationPolicies.DocumentosCreate, AppPermissions.DocumentosCreate),
        Funcionario(AuthorizationPolicies.DocumentosEdit, AppPermissions.DocumentosEdit),
        Funcionario(AuthorizationPolicies.DocumentosApprove, AppPermissions.DocumentosApprove),
        Funcionario(AuthorizationPolicies.ComunicadosView, AppPermissions.ComunicadosView),
        Funcionario(AuthorizationPolicies.ComunicadosCreate, AppPermissions.ComunicadosCreate),
        Funcionario(AuthorizationPolicies.ComunicadosEdit, AppPermissions.ComunicadosEdit),
        Funcionario(AuthorizationPolicies.ComunicadosDelete, AppPermissions.ComunicadosDelete),
        Funcionario(AuthorizationPolicies.EventosAlunoView, AppPermissions.EventosAlunoView),
        Funcionario(AuthorizationPolicies.EventosAlunoCreate, AppPermissions.EventosAlunoCreate),
        Funcionario(AuthorizationPolicies.EventosAlunoEdit, AppPermissions.EventosAlunoEdit),
        Funcionario(AuthorizationPolicies.EventosAlunoDelete, AppPermissions.EventosAlunoDelete),
        Funcionario(AuthorizationPolicies.ConquistasView, AppPermissions.ConquistasView),
        Funcionario(AuthorizationPolicies.ConquistasCreate, AppPermissions.ConquistasCreate),
        Funcionario(AuthorizationPolicies.ConquistasEdit, AppPermissions.ConquistasEdit),
        Funcionario(AuthorizationPolicies.AulasView, AppPermissions.AulasView),
        Funcionario(AuthorizationPolicies.AulasCreate, AppPermissions.AulasCreate),
        Funcionario(AuthorizationPolicies.AulasEdit, AppPermissions.AulasEdit),

        AdminOnly(AuthorizationPolicies.AdminGerenciarUsuarios, AppPermissions.GerenciarUsuarios),
        AdminOnly(AuthorizationPolicies.AdminGerenciarCargos, AppPermissions.GerenciarCargos),
        AdminOnly(AuthorizationPolicies.AdminEditarPermissoes, AppPermissions.EditarPermissoes),
        AdminOnly(AuthorizationPolicies.AdminVisualizarDados, AppPermissions.VisualizarDados),
        AdminOnly(AuthorizationPolicies.AdminGerenciarSistema, AppPermissions.GerenciarSistema)
    ];

    public static void AddIkkonPolicies(this AuthorizationOptions options)
    {
        options.AddPolicy(
            AuthorizationPolicies.Funcionario,
            policy => policy.RequireRole(AppRoles.Funcionario, AppRoles.Admin));

        options.AddPolicy(
            AuthorizationPolicies.Aluno,
            policy => policy.RequireRole(AppRoles.Aluno));

        options.AddPolicy(
            AuthorizationPolicies.Admin,
            policy => policy.RequireRole(AppRoles.Admin));

        foreach (var definition in PermissionPolicies)
        {
            options.AddPolicy(
                definition.PolicyName,
                policy => ConfigurePermissionPolicy(policy, definition));
        }
    }

    private static void ConfigurePermissionPolicy(
        AuthorizationPolicyBuilder policy,
        PermissionPolicyDefinition definition)
    {
        switch (definition.Scope)
        {
            case PermissionPolicyScope.Funcionario:
                policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        AppPermissionEvaluator.HasFuncionarioPermission(context.User, definition.Permissions));
                break;

            case PermissionPolicyScope.Authenticated:
                policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        AppPermissionEvaluator.HasAuthenticatedPermission(context.User, definition.Permissions));
                break;

            case PermissionPolicyScope.AdminOnly:
                policy.RequireRole(AppRoles.Admin);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(definition), definition.Scope, "Escopo de policy inválido.");
        }
    }

    private static PermissionPolicyDefinition Funcionario(string policyName, params string[] permissions)
    {
        return new PermissionPolicyDefinition(policyName, PermissionPolicyScope.Funcionario, permissions);
    }

    private static PermissionPolicyDefinition Authenticated(string policyName, params string[] permissions)
    {
        return new PermissionPolicyDefinition(policyName, PermissionPolicyScope.Authenticated, permissions);
    }

    private static PermissionPolicyDefinition AdminOnly(string policyName, params string[] permissions)
    {
        return new PermissionPolicyDefinition(policyName, PermissionPolicyScope.AdminOnly, permissions);
    }
}
