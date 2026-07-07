namespace IkkonAdmin.Web.Security;

public static class AuthorizationPolicies
{
    public const string Funcionario = "POLICY_FUNCIONARIO";
    public const string Aluno = "POLICY_ALUNO";
    public const string Admin = "POLICY_ADMIN";

    public const string DashboardView = "POLICY_DASHBOARD_VIEW";

    public const string AlunosView = "POLICY_ALUNOS_VIEW";
    public const string AlunosCreate = "POLICY_ALUNOS_CREATE";
    public const string AlunosEdit = "POLICY_ALUNOS_EDIT";
    public const string AlunosDelete = "POLICY_ALUNOS_DELETE";

    public const string TurmasView = "POLICY_TURMAS_VIEW";
    public const string TurmasCreate = "POLICY_TURMAS_CREATE";
    public const string TurmasEdit = "POLICY_TURMAS_EDIT";
    public const string TurmasDelete = "POLICY_TURMAS_DELETE";

    public const string FinanceiroView = "POLICY_FINANCEIRO_VIEW";
    public const string FinanceiroCreate = "POLICY_FINANCEIRO_CREATE";
    public const string FinanceiroEdit = "POLICY_FINANCEIRO_EDIT";
    public const string FinanceiroDelete = "POLICY_FINANCEIRO_DELETE";

    public const string AdmissoesView = "POLICY_ADMISSOES_VIEW";
    public const string AdmissoesCreate = "POLICY_ADMISSOES_CREATE";
    public const string AdmissoesEdit = "POLICY_ADMISSOES_EDIT";
    public const string AdmissoesDelete = "POLICY_ADMISSOES_DELETE";

    public const string DesligamentosView = "POLICY_DESLIGAMENTOS_VIEW";
    public const string DesligamentosCreate = "POLICY_DESLIGAMENTOS_CREATE";
    public const string DesligamentosEdit = "POLICY_DESLIGAMENTOS_EDIT";
    public const string DesligamentosDelete = "POLICY_DESLIGAMENTOS_DELETE";

    public const string GraduacoesView = "POLICY_GRADUACOES_VIEW";
    public const string GraduacoesCreate = "POLICY_GRADUACOES_CREATE";
    public const string GraduacoesEdit = "POLICY_GRADUACOES_EDIT";
    public const string GraduacoesDelete = "POLICY_GRADUACOES_DELETE";

    public const string ConfiguracoesView = "POLICY_CONFIGURACOES_VIEW";
    public const string ConfiguracoesEdit = "POLICY_CONFIGURACOES_EDIT";

    public const string GoogleAgendaView = "POLICY_GOOGLE_AGENDA_VIEW";
    public const string GoogleAgendaCreate = "POLICY_GOOGLE_AGENDA_CREATE";
    public const string GoogleAgendaEdit = "POLICY_GOOGLE_AGENDA_EDIT";
    public const string GoogleAgendaDelete = "POLICY_GOOGLE_AGENDA_DELETE";
    public const string GoogleAgendaManage = "POLICY_GOOGLE_AGENDA_MANAGE";

    public const string InventarioView = "POLICY_INVENTARIO_VIEW";
    public const string InventarioCreate = "POLICY_INVENTARIO_CREATE";
    public const string InventarioEdit = "POLICY_INVENTARIO_EDIT";
    public const string InventarioDelete = "POLICY_INVENTARIO_DELETE";
    public const string InventarioManage = "POLICY_INVENTARIO_MANAGE";

    public const string BlogView = "POLICY_BLOG_VIEW";
    public const string BlogCreate = "POLICY_BLOG_CREATE";
    public const string BlogEdit = "POLICY_BLOG_EDIT";
    public const string BlogPublish = "POLICY_BLOG_PUBLISH";
    public const string BlogArchive = "POLICY_BLOG_ARCHIVE";
    public const string BlogDelete = "POLICY_BLOG_DELETE";
    public const string BlogFeature = "POLICY_BLOG_FEATURE";
    public const string BlogCategoryManage = "POLICY_BLOG_CATEGORY_MANAGE";
    public const string BlogTagManage = "POLICY_BLOG_TAG_MANAGE";

    public const string AreaAlunoView = "POLICY_AREA_ALUNO_VIEW";
    public const string AreaAlunoManage = "POLICY_AREA_ALUNO_MANAGE";
    public const string FrequenciaView = "POLICY_FREQUENCIA_VIEW";
    public const string FrequenciaCreate = "POLICY_FREQUENCIA_CREATE";
    public const string FrequenciaEdit = "POLICY_FREQUENCIA_EDIT";
    public const string DocumentosView = "POLICY_DOCUMENTOS_VIEW";
    public const string DocumentosCreate = "POLICY_DOCUMENTOS_CREATE";
    public const string DocumentosEdit = "POLICY_DOCUMENTOS_EDIT";
    public const string DocumentosApprove = "POLICY_DOCUMENTOS_APPROVE";
    public const string ComunicadosView = "POLICY_COMUNICADOS_VIEW";
    public const string ComunicadosCreate = "POLICY_COMUNICADOS_CREATE";
    public const string ComunicadosEdit = "POLICY_COMUNICADOS_EDIT";
    public const string ComunicadosDelete = "POLICY_COMUNICADOS_DELETE";
    public const string EventosAlunoView = "POLICY_EVENTOS_ALUNO_VIEW";
    public const string EventosAlunoCreate = "POLICY_EVENTOS_ALUNO_CREATE";
    public const string EventosAlunoEdit = "POLICY_EVENTOS_ALUNO_EDIT";
    public const string EventosAlunoDelete = "POLICY_EVENTOS_ALUNO_DELETE";
    public const string ConquistasView = "POLICY_CONQUISTAS_VIEW";
    public const string ConquistasCreate = "POLICY_CONQUISTAS_CREATE";
    public const string ConquistasEdit = "POLICY_CONQUISTAS_EDIT";
    public const string AulasView = "POLICY_AULAS_VIEW";
    public const string AulasCreate = "POLICY_AULAS_CREATE";
    public const string AulasEdit = "POLICY_AULAS_EDIT";

    public const string AdminGerenciarUsuarios = "POLICY_ADMIN_GERENCIAR_USUARIOS";
    public const string AdminGerenciarCargos = "POLICY_ADMIN_GERENCIAR_CARGOS";
    public const string AdminEditarPermissoes = "POLICY_ADMIN_EDITAR_PERMISSOES";
    public const string AdminVisualizarDados = "POLICY_ADMIN_VISUALIZAR_DADOS";
    public const string AdminGerenciarSistema = "POLICY_ADMIN_GERENCIAR_SISTEMA";
}
