namespace IkkonAdmin.Web.Security;

public static class AppPermissions
{
    // Admin.
    public const string GerenciarUsuarios = "GERENCIAR_USUARIOS";
    public const string GerenciarCargos = "GERENCIAR_CARGOS";
    public const string EditarPermissoes = "EDITAR_PERMISSOES";
    public const string VisualizarDados = "VISUALIZAR_DADOS";
    public const string GerenciarSistema = "GERENCIAR_SISTEMA";

    // Operacional.
    public const string DashboardView = "DASHBOARD_VIEW";

    public const string AlunosView = "ALUNOS_VIEW";
    public const string AlunosCreate = "ALUNOS_CREATE";
    public const string AlunosEdit = "ALUNOS_EDIT";
    public const string AlunosDelete = "ALUNOS_DELETE";

    public const string TurmasView = "TURMAS_VIEW";
    public const string TurmasCreate = "TURMAS_CREATE";
    public const string TurmasEdit = "TURMAS_EDIT";
    public const string TurmasDelete = "TURMAS_DELETE";

    public const string FinanceiroView = "FINANCEIRO_VIEW";
    public const string FinanceiroCreate = "FINANCEIRO_CREATE";
    public const string FinanceiroEdit = "FINANCEIRO_EDIT";
    public const string FinanceiroDelete = "FINANCEIRO_DELETE";

    public const string AdmissoesView = "ADMISSOES_VIEW";
    public const string AdmissoesCreate = "ADMISSOES_CREATE";
    public const string AdmissoesEdit = "ADMISSOES_EDIT";
    public const string AdmissoesDelete = "ADMISSOES_DELETE";

    public const string DesligamentosView = "DESLIGAMENTOS_VIEW";
    public const string DesligamentosCreate = "DESLIGAMENTOS_CREATE";
    public const string DesligamentosEdit = "DESLIGAMENTOS_EDIT";
    public const string DesligamentosDelete = "DESLIGAMENTOS_DELETE";

    public const string GraduacoesView = "GRADUACOES_VIEW";
    public const string GraduacoesCreate = "GRADUACOES_CREATE";
    public const string GraduacoesEdit = "GRADUACOES_EDIT";
    public const string GraduacoesDelete = "GRADUACOES_DELETE";

    public const string ConfiguracoesView = "CONFIGURACOES_VIEW";
    public const string ConfiguracoesEdit = "CONFIGURACOES_EDIT";

    public const string GoogleAgendaView = "GOOGLE_AGENDA_VIEW";
    public const string GoogleAgendaCreate = "GOOGLE_AGENDA_CREATE";
    public const string GoogleAgendaEdit = "GOOGLE_AGENDA_EDIT";
    public const string GoogleAgendaDelete = "GOOGLE_AGENDA_DELETE";
    public const string GoogleAgendaManage = "GOOGLE_AGENDA_MANAGE";

    public const string InventarioView = "INVENTARIO_VIEW";
    public const string InventarioCreate = "INVENTARIO_CREATE";
    public const string InventarioEdit = "INVENTARIO_EDIT";
    public const string InventarioDelete = "INVENTARIO_DELETE";
    public const string InventarioManage = "INVENTARIO_MANAGE";

    public static IReadOnlyList<AppPermissionDefinition> Definicoes { get; } = new[]
    {
        new AppPermissionDefinition(GerenciarUsuarios, "Gerenciar usuarios", "Permite criar, editar, ativar e excluir usuarios do sistema.", "Administracao"),
        new AppPermissionDefinition(GerenciarCargos, "Gerenciar cargos", "Permite criar, editar e desativar cargos no painel administrativo.", "Administracao"),
        new AppPermissionDefinition(EditarPermissoes, "Editar permissoes", "Permite alterar permissoes diretas e por cargo.", "Administracao"),
        new AppPermissionDefinition(VisualizarDados, "Visualizar dados administrativos", "Permite consultar dashboards e dados administrativos sensiveis.", "Administracao"),
        new AppPermissionDefinition(GerenciarSistema, "Gerenciar sistema", "Permite alterar configuracoes globais da aplicacao.", "Administracao"),

        new AppPermissionDefinition(DashboardView, "Visualizar dashboard", "Permite acesso ao dashboard operacional interno.", "Dashboard"),

        new AppPermissionDefinition(AlunosView, "Visualizar alunos", "Permite listar e consultar dados de alunos.", "Alunos"),
        new AppPermissionDefinition(AlunosCreate, "Criar alunos", "Permite cadastrar novos alunos.", "Alunos"),
        new AppPermissionDefinition(AlunosEdit, "Editar alunos", "Permite editar dados e status de alunos.", "Alunos"),
        new AppPermissionDefinition(AlunosDelete, "Excluir alunos", "Permite excluir registros de alunos.", "Alunos"),

        new AppPermissionDefinition(TurmasView, "Visualizar turmas", "Permite listar e consultar turmas.", "Turmas"),
        new AppPermissionDefinition(TurmasCreate, "Criar turmas", "Permite cadastrar novas turmas.", "Turmas"),
        new AppPermissionDefinition(TurmasEdit, "Editar turmas", "Permite editar turmas e vinculos de alunos.", "Turmas"),
        new AppPermissionDefinition(TurmasDelete, "Excluir turmas", "Permite excluir turmas.", "Turmas"),

        new AppPermissionDefinition(FinanceiroView, "Visualizar financeiro", "Permite visualizar mensalidades, atrasos e historicos.", "Financeiro"),
        new AppPermissionDefinition(FinanceiroCreate, "Criar financeiro", "Permite gerar mensalidades e registrar pagamentos.", "Financeiro"),
        new AppPermissionDefinition(FinanceiroEdit, "Editar financeiro", "Permite alterar valores e status financeiros.", "Financeiro"),
        new AppPermissionDefinition(FinanceiroDelete, "Excluir financeiro", "Permite excluir registros financeiros.", "Financeiro"),

        new AppPermissionDefinition(AdmissoesView, "Visualizar admissoes", "Permite consultar processos de admissao.", "Admissoes"),
        new AppPermissionDefinition(AdmissoesCreate, "Criar admissoes", "Permite abrir novos processos de admissao.", "Admissoes"),
        new AppPermissionDefinition(AdmissoesEdit, "Editar admissoes", "Permite atualizar checklists e matriculas de admissao.", "Admissoes"),
        new AppPermissionDefinition(AdmissoesDelete, "Excluir admissoes", "Permite excluir processos de admissao.", "Admissoes"),

        new AppPermissionDefinition(DesligamentosView, "Visualizar desligamentos", "Permite consultar processos de desligamento.", "Desligamentos"),
        new AppPermissionDefinition(DesligamentosCreate, "Criar desligamentos", "Permite abrir solicitacoes de desligamento.", "Desligamentos"),
        new AppPermissionDefinition(DesligamentosEdit, "Editar desligamentos", "Permite atualizar dados e confirmar desligamentos.", "Desligamentos"),
        new AppPermissionDefinition(DesligamentosDelete, "Excluir desligamentos", "Permite excluir processos de desligamento.", "Desligamentos"),

        new AppPermissionDefinition(GraduacoesView, "Visualizar graduacoes", "Permite consultar exames e historico de graduacoes.", "Graduacoes"),
        new AppPermissionDefinition(GraduacoesCreate, "Criar graduacoes", "Permite criar exames e registrar resultados.", "Graduacoes"),
        new AppPermissionDefinition(GraduacoesEdit, "Editar graduacoes", "Permite editar registros de graduacao.", "Graduacoes"),
        new AppPermissionDefinition(GraduacoesDelete, "Excluir graduacoes", "Permite excluir registros de graduacao.", "Graduacoes"),

        new AppPermissionDefinition(ConfiguracoesView, "Visualizar configuracoes", "Permite acesso a area de configuracoes da conta.", "Configuracoes"),
        new AppPermissionDefinition(ConfiguracoesEdit, "Editar configuracoes", "Permite atualizar dados e preferencias da conta.", "Configuracoes"),

        new AppPermissionDefinition(GoogleAgendaView, "Visualizar Google Agenda", "Permite visualizar eventos sincronizados com o Google Agenda.", "Google Agenda"),
        new AppPermissionDefinition(GoogleAgendaCreate, "Criar eventos no Google Agenda", "Permite criar eventos no Google Agenda.", "Google Agenda"),
        new AppPermissionDefinition(GoogleAgendaEdit, "Editar eventos no Google Agenda", "Permite editar eventos existentes no Google Agenda.", "Google Agenda"),
        new AppPermissionDefinition(GoogleAgendaDelete, "Excluir eventos no Google Agenda", "Permite excluir ou cancelar eventos do Google Agenda.", "Google Agenda"),
        new AppPermissionDefinition(GoogleAgendaManage, "Gerenciar Google Agenda", "Permite executar todas as acoes da integracao com Google Agenda.", "Google Agenda"),

        new AppPermissionDefinition(InventarioView, "Visualizar inventario", "Permite visualizar itens do inventario.", "Inventario"),
        new AppPermissionDefinition(InventarioCreate, "Criar itens de inventario", "Permite cadastrar novos itens no inventario.", "Inventario"),
        new AppPermissionDefinition(InventarioEdit, "Editar itens de inventario", "Permite editar itens do inventario.", "Inventario"),
        new AppPermissionDefinition(InventarioDelete, "Excluir itens de inventario", "Permite baixar ou inativar itens do inventario.", "Inventario"),
        new AppPermissionDefinition(InventarioManage, "Gerenciar inventario", "Permite executar todas as acoes do inventario.", "Inventario")
    };

    public static IReadOnlyCollection<string> Todas { get; } = Definicoes
        .Select(x => x.Codigo)
        .ToArray();
}

public sealed record AppPermissionDefinition(
    string Codigo,
    string Nome,
    string Descricao,
    string Modulo);
