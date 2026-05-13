# Módulos existentes

## Dashboard

- Função: visão operacional do sistema interno.
- Controller: `HomeController`.
- Service: `DashboardService`.
- View: `Views/Home/Index.cshtml`.
- Dados: alunos ativos, mensalidades pendentes/atrasadas, receita do mês, próximos vencimentos, inadimplentes e atividades recentes.
- Permissão: `AuthorizationPolicies.DashboardView`.

## Alunos

- Função: CRUD administrativo de alunos.
- Controller: `AlunosController`.
- Service: `AlunoService`.
- Views: `Index`, `Create`, `Edit`, `Details`.
- Entidades: `Aluno`, `AlunoTurma`, `Turma`, `Mensalidade`, `Pagamento`, `HistoricoAluno`.
- Regras importantes: CPF único, status por `StatusAlunoEnum`, turma principal via `Aluno.TurmaId` e vínculo múltiplo via `AlunoTurma`.
- Observação: a modelagem suporta aluno em múltiplas turmas; a UX administrativa já foi evoluída em Turmas, mas cada tela deve ser revisada antes de assumir cobertura total.

## Turmas

- Função: cadastro e edição de turmas, com vínculo de alunos.
- Controller: `TurmasController`.
- Service: `TurmaService`.
- Views: `Index`, `Create`, `Edit`.
- Entidades: `Turma`, `AlunoTurma`, `Aluno`.
- Regras importantes: turma possui `Nome`, `Modalidade`, `Horario`, `Ativa`; aluno pode estar em mais de uma turma via tabela `AlunosTurmas`.

## Financeiro

- Função: controle de mensalidades, pagamentos, atrasos e histórico financeiro.
- Controller: `FinanceiroController`.
- Service: `FinanceiroService`.
- Views: `Index`, `Atrasados`, `RegistrarPagamento`, `HistoricoAluno`.
- Entidades: `Mensalidade`, `Pagamento`, `Aluno`, `Desconto`, `AcordoFinanceiro`, `ConfiguracaoSistema`.
- Regras importantes: geração mensal por competência, índice único por aluno/competência, status por `StatusMensalidadeEnum`, registro manual de pagamento, alteração de valor final e status.

## Admissões

- Função: acompanhar aula experimental, matrícula e checklist inicial.
- Controller: `AdmissoesController`.
- Service: `AdmissaoService`.
- Views: `Index`, `Create`, `Details`.
- Entidades: `Admissao`, `Aluno`, `Turma`.
- Regras importantes: status por `StatusAdmissaoEnum`, checklist de contrato, pagamento inicial e integração, criação de matrícula a partir da admissão.

## Desligamentos

- Função: registrar e acompanhar saída de alunos.
- Controller: `DesligamentosController`.
- Service: `DesligamentoService`.
- Views: `Index`, `Create`, `Details`.
- Entidades: `Desligamento`, `Aluno`, `Mensalidade`.
- Regras importantes: cálculo de pendências, requerimento recebido, confirmação, remoção de acessos e encerramento de cobranças futuras.

## Graduações

- Função: exames, resultados e histórico de graduação.
- Controller: `GraduacoesController`.
- Service: `GraduacaoService`.
- Views: `Index`, `Create`, `Details`.
- Entidades: `Graduacao`, `ExameGraduacao`, `Aluno`.
- Regras importantes: `NivelGraduacaoEnum`, resultado aprovado/reprovado, atualização de nível, certificado e omamori.

## Usuários

- Função: gestão de contas internas e futuras contas de aluno.
- Controller: `PainelAdminController`.
- Service: `AdminPainelService`.
- Views: `Usuarios`, `NovoUsuario`, `EditarUsuario`, `Acessos`.
- Entidades: `UsuarioSistema`, `UsuarioRole`, `UsuarioPermissao`, `AuditoriaLog`.
- Regras importantes: soft delete via `Excluido`, filtro global em `UsuarioSistema`, vínculo opcional `AlunoId`.

## Cargos

- Função: perfis/roles configuráveis.
- Controller: `PainelAdminController`.
- Service: `AdminPainelService`.
- Views: `Cargos`, `NovoCargo`, `EditarCargo`.
- Entidades: `RoleSistema`, `RolePermissao`, `UsuarioRole`.
- Regras importantes: roles de sistema (`IsSistema`) e tipo de acesso por `TipoAcessoEnum`.

## Permissões

- Função: controle granular de acesso.
- Código: `Security/AppPermissions.cs`, `Security/AuthorizationPolicies.cs`.
- Entidades: `PermissaoSistema`, `RolePermissao`, `UsuarioPermissao`.
- UI: `PainelAdmin/Acessos.cshtml` e telas de cargos.
- Regras importantes: admin recebe todas as permissões; funcionários dependem de roles/permissões; alunos têm acesso restrito.

## Inventário

- Função: controle de itens como taikos, bachis e equipamentos.
- Controller: `InventarioController`.
- Service: `InventarioService`.
- Views: `Index`, `Create`, `Edit`, `Details`, `_InventarioForm`.
- Entidades: `InventarioItem`, `InventarioMovimentacao`, `UsuarioSistema`.
- Regras importantes: filtros por categoria/tipo/status/estado/localização, soft delete por inativação/baixa, índices para filtros.
- Rota: `/admin/inventario`.

## Agenda / Google Agenda

- Função: visualizar e gerenciar eventos do Google Agenda.
- Controller: `GoogleAgendaController`.
- Service: `GoogleAgendaService`.
- Views: `Index`, `Create`, `Edit`, `Details`, `_GoogleAgendaForm`.
- Entidade persistida: `GoogleAgendaConexao`.
- ViewModels: `GoogleAgendaViewModels`.
- Regras importantes: OAuth, refresh token protegido com Data Protection, listagem por período, visão lista e calendário anual, filtros por tipo.
- Rota: `/admin/agenda`.

## Site institucional

- Função: apresentação pública da escola e do grupo artístico.
- Controller: `InstitucionalController`.
- Views: `Index`, `Escola`, `Eventos`.
- Layout: `_PublicLayout.cshtml`.
- Partials: header, footer, CTA, cursos, FAQ, vídeos e galeria.
- Recursos: imagens em `wwwroot/Images`, mapa incorporado, links de WhatsApp/redes sociais e vídeos YouTube.
- Rotas: `/`, `/escola`, `/eventos`.

## Eventos

- No site público, eventos são uma frente institucional/artística em `Views/Institucional/Eventos.cshtml`.
- Na agenda interna, eventos são registros integrados ao Google Agenda em `/admin/agenda`.
- São conceitos diferentes: eventos públicos de apresentação/contratação versus eventos operacionais da agenda.

## Futuro portal do aluno

- Função: permitir que o aluno consulte informações próprias.
- Controllers: `AlunoAuthController`, `AlunoAreaController`.
- Service: `AreaAlunoService`.
- Views: `AlunoAuth/Login`, `AlunoArea/Index`, `Perfil`, `Financeiro`, `Turmas`, `AcessoIndisponivel`.
- Layout: `_AlunoLayout.cshtml`.
- Regra central: busca dados pelo usuário logado e vínculo `UsuarioSistema.AlunoId`; não deve expor dados de outros alunos.

