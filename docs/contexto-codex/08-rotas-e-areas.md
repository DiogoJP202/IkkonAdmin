# Rotas e áreas

## Rotas públicas

Definidas em `Program.cs`:

- `/`: `InstitucionalController.Index`.
- `/escola`: `InstitucionalController.Escola`.
- `/eventos`: `InstitucionalController.Eventos`.
- `/blog` e `/blog/{slug}`: `BlogController`.
- `/pt`, `/en`, `/ja` e respectivas rotas localizadas: site público internacional.
- `/institucional/{action=Index}/{id?}`: rota complementar institucional.
- `/health` e `/health/live`: liveness anônimo.
- `/health/ready`: readiness de SQL Server e storage privado.
- `/health/sql` e `/health/storage`: diagnósticos isolados.

Também são públicas:

- `/auth/login`: login administrativo.
- `/aluno/login`: login da Área do Aluno.

## Rotas administrativas

Rota base:

- `/admin/{controller=Home}/{action=Index}/{id?}`.

Exemplos:

- `/admin`: dashboard interno.
- `/admin/Alunos`.
- `/admin/Turmas`.
- `/admin/Financeiro`.
- `/admin/Admissoes`.
- `/admin/Desligamentos`.
- `/admin/Graduacoes`.

Rotas administrativas com route attribute:

- `/admin/agenda`: `GoogleAgendaController`.
- `/admin/agenda/criar`.
- `/admin/agenda/editar/{eventoId}`.
- `/admin/agenda/detalhes/{eventoId}`.
- `/admin/inventario`: `InventarioController`.
- `/admin/inventario/criar`.
- `/admin/inventario/editar/{id}`.
- `/admin/inventario/detalhes/{id}`.

Painel administrativo:

- `/admin/painel`.
- `/admin/painel/Usuarios`.
- `/admin/painel/NovoUsuario`.
- `/admin/painel/EditarUsuario/{id}`.
- `/admin/painel/Acessos/{id}`.
- `/admin/painel/Cargos`.
- `/admin/painel/NovoCargo`.
- `/admin/painel/EditarCargo/{id}`.
- `/admin/painel/Logs`.
- `/admin/painel/Sistema`.

## Configurações

- `/configuracoes/{action=Index}/{id?}`.

Exige usuário autenticado e policies de configurações nas actions.

## Área do Aluno

Rotas:

- `/aluno/login`: login do aluno.
- `/aluno/sair`: logout do aluno.
- `/area-do-aluno`: dashboard do aluno.
- `/area-do-aluno/perfil`.
- `/area-do-aluno/financeiro`.
- `/area-do-aluno/turmas`.
- `/area-do-aluno/aulas`.
- `/area-do-aluno/frequencia`.
- `/area-do-aluno/documentos`.
- `/area-do-aluno/comunicados`.
- `/area-do-aluno/eventos`.
- `/area-do-aluno/conquistas`.
- `/area-do-aluno/acessoindisponivel`.

Também existe rota mapeada:

- `/aluno/{action=Index}/{id?}` para `AlunoAreaController`, exigindo `AuthorizationPolicies.Aluno`.

## Separação de controllers

- Público: `InstitucionalController`.
- Auth administrativo: `AuthController`.
- Auth aluno: `AlunoAuthController`.
- Portal do aluno: `AlunoAreaController`.
- Admin operacional: `HomeController`, `AlunosController`, `TurmasController`, `FinanceiroController`, `AdmissoesController`, `DesligamentosController`, `GraduacoesController`.
- Admin especializado: `GoogleAgendaController`, `InventarioController`,
  `PainelAdminController`, `BlogAdminController`, `BlogCategoriasController` e
  `AreaAlunoAdminController`.

## Convenção para novas rotas públicas

Para novas páginas públicas, preferir:

- Controller público dedicado ou action em `InstitucionalController`, se for institucional simples.
- URL limpa em minúsculas, sem depender de `/admin`.
- Layout `_PublicLayout.cshtml`.
- Sem `[Authorize]`.

Exemplos atuais:

- `/blog`.
- `/blog/{slug}`.

## Convenção para novas rotas administrativas

Para novas features administrativas, preferir:

- `/admin/{modulo}` quando houver rota attribute dedicada.
- Ou controller convencional acessado por `/admin/{Controller}`.
- Sempre aplicar `[Authorize]` com policy adequada.
- Incluir link na sidebar somente se o usuário tiver permissão.

Exemplos atuais:

- `/admin/blog`.
- `/admin/blog/criar`.
- `/admin/blog/editar/{id}`.
