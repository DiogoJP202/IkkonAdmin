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

    public const string BlogView = "BLOG_VIEW";
    public const string BlogCreate = "BLOG_CREATE";
    public const string BlogEdit = "BLOG_EDIT";
    public const string BlogPublish = "BLOG_PUBLISH";
    public const string BlogArchive = "BLOG_ARCHIVE";
    public const string BlogDelete = "BLOG_DELETE";
    public const string BlogFeature = "BLOG_FEATURE";
    public const string BlogCategoryManage = "BLOG_CATEGORY_MANAGE";
    public const string BlogTagManage = "BLOG_TAG_MANAGE";

    public const string AreaAlunoView = "AREA_ALUNO_VIEW";
    public const string AreaAlunoManage = "AREA_ALUNO_MANAGE";
    public const string FrequenciaView = "FREQUENCIA_VIEW";
    public const string FrequenciaCreate = "FREQUENCIA_CREATE";
    public const string FrequenciaEdit = "FREQUENCIA_EDIT";
    public const string DocumentosView = "DOCUMENTOS_VIEW";
    public const string DocumentosCreate = "DOCUMENTOS_CREATE";
    public const string DocumentosEdit = "DOCUMENTOS_EDIT";
    public const string DocumentosApprove = "DOCUMENTOS_APPROVE";
    public const string ComunicadosView = "COMUNICADOS_VIEW";
    public const string ComunicadosCreate = "COMUNICADOS_CREATE";
    public const string ComunicadosEdit = "COMUNICADOS_EDIT";
    public const string ComunicadosDelete = "COMUNICADOS_DELETE";
    public const string EventosAlunoView = "EVENTOS_ALUNO_VIEW";
    public const string EventosAlunoCreate = "EVENTOS_ALUNO_CREATE";
    public const string EventosAlunoEdit = "EVENTOS_ALUNO_EDIT";
    public const string EventosAlunoDelete = "EVENTOS_ALUNO_DELETE";
    public const string ConquistasView = "CONQUISTAS_VIEW";
    public const string ConquistasCreate = "CONQUISTAS_CREATE";
    public const string ConquistasEdit = "CONQUISTAS_EDIT";
    public const string AulasView = "AULAS_VIEW";
    public const string AulasCreate = "AULAS_CREATE";
    public const string AulasEdit = "AULAS_EDIT";

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
        new AppPermissionDefinition(InventarioManage, "Gerenciar inventario", "Permite executar todas as acoes do inventario.", "Inventario"),

        new AppPermissionDefinition(BlogView, "Visualizar blog", "Permite listar e consultar posts do blog no painel.", "Blog"),
        new AppPermissionDefinition(BlogCreate, "Criar posts do blog", "Permite criar novos posts do blog.", "Blog"),
        new AppPermissionDefinition(BlogEdit, "Editar posts do blog", "Permite editar posts existentes do blog.", "Blog"),
        new AppPermissionDefinition(BlogPublish, "Publicar posts do blog", "Permite publicar e agendar posts do blog.", "Blog"),
        new AppPermissionDefinition(BlogArchive, "Arquivar posts do blog", "Permite arquivar posts do blog.", "Blog"),
        new AppPermissionDefinition(BlogDelete, "Excluir posts do blog", "Permite excluir posts do blog com exclusao logica.", "Blog"),
        new AppPermissionDefinition(BlogFeature, "Destacar posts do blog", "Permite marcar posts como destaque e blog da semana.", "Blog"),
        new AppPermissionDefinition(BlogCategoryManage, "Gerenciar categorias do blog", "Permite criar, editar e ativar categorias do blog.", "Blog"),
        new AppPermissionDefinition(BlogTagManage, "Gerenciar tags do blog", "Permite criar e associar tags no modulo de blog.", "Blog"),

        new AppPermissionDefinition(AreaAlunoView, "Visualizar area do aluno", "Permite consultar a operacao administrativa do portal do aluno.", "Area do Aluno"),
        new AppPermissionDefinition(AreaAlunoManage, "Gerenciar area do aluno", "Permite configurar recursos gerais do portal do aluno.", "Area do Aluno"),
        new AppPermissionDefinition(FrequenciaView, "Visualizar frequencia", "Permite consultar aulas e registros de frequencia.", "Area do Aluno"),
        new AppPermissionDefinition(FrequenciaCreate, "Criar frequencia", "Permite registrar presencas e faltas.", "Area do Aluno"),
        new AppPermissionDefinition(FrequenciaEdit, "Editar frequencia", "Permite corrigir registros de frequencia.", "Area do Aluno"),
        new AppPermissionDefinition(DocumentosView, "Visualizar documentos", "Permite consultar documentos solicitados e enviados por alunos.", "Area do Aluno"),
        new AppPermissionDefinition(DocumentosCreate, "Solicitar documentos", "Permite criar tipos e solicitacoes de documentos.", "Area do Aluno"),
        new AppPermissionDefinition(DocumentosEdit, "Editar documentos", "Permite atualizar solicitacoes de documentos.", "Area do Aluno"),
        new AppPermissionDefinition(DocumentosApprove, "Aprovar documentos", "Permite aprovar ou recusar documentos enviados.", "Area do Aluno"),
        new AppPermissionDefinition(ComunicadosView, "Visualizar comunicados", "Permite consultar comunicados internos do portal.", "Area do Aluno"),
        new AppPermissionDefinition(ComunicadosCreate, "Criar comunicados", "Permite publicar comunicados para alunos e turmas.", "Area do Aluno"),
        new AppPermissionDefinition(ComunicadosEdit, "Editar comunicados", "Permite atualizar comunicados publicados.", "Area do Aluno"),
        new AppPermissionDefinition(ComunicadosDelete, "Excluir comunicados", "Permite remover comunicados do portal.", "Area do Aluno"),
        new AppPermissionDefinition(EventosAlunoView, "Visualizar eventos dos alunos", "Permite consultar eventos internos do portal do aluno.", "Area do Aluno"),
        new AppPermissionDefinition(EventosAlunoCreate, "Criar eventos dos alunos", "Permite cadastrar eventos para alunos, turmas ou todos.", "Area do Aluno"),
        new AppPermissionDefinition(EventosAlunoEdit, "Editar eventos dos alunos", "Permite atualizar eventos internos do portal.", "Area do Aluno"),
        new AppPermissionDefinition(EventosAlunoDelete, "Excluir eventos dos alunos", "Permite remover eventos internos do portal.", "Area do Aluno"),
        new AppPermissionDefinition(ConquistasView, "Visualizar conquistas", "Permite consultar insignias e conquistas dos alunos.", "Area do Aluno"),
        new AppPermissionDefinition(ConquistasCreate, "Criar conquistas", "Permite criar insignias e atribui-las a alunos.", "Area do Aluno"),
        new AppPermissionDefinition(ConquistasEdit, "Editar conquistas", "Permite atualizar insignias e conquistas.", "Area do Aluno"),
        new AppPermissionDefinition(AulasView, "Visualizar aulas", "Permite consultar aulas e horarios estruturados.", "Area do Aluno"),
        new AppPermissionDefinition(AulasCreate, "Criar aulas", "Permite cadastrar horarios e aulas.", "Area do Aluno"),
        new AppPermissionDefinition(AulasEdit, "Editar aulas", "Permite atualizar horarios e aulas.", "Area do Aluno")
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
