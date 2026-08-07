# Arquitetura do projeto

## Estrutura geral

O projeto principal está em `IkkonAdmin.Web`. A solução usa MVC tradicional, com Controllers, Services, Models/ViewModels, Entities, EF Core e Razor Views.

Arquivos de entrada relevantes:

- `IkkonAdmin.slnx`.
- `IkkonAdmin.Web/IkkonAdmin.Web.csproj`.
- `IkkonAdmin.Web/Program.cs`.
- `IkkonAdmin.Web/Data/ApplicationDbContext.cs`.

## Organização de pastas

- `Controllers`: controllers MVC públicos, administrativos, autenticação e portal do aluno.
- `Models/Entities`: entidades persistidas pelo EF Core.
- `Models/ViewModels`: modelos específicos para telas e formulários.
- `Enums`: enums de status, tipos e preferências.
- `Data`: `ApplicationDbContext`, migrations, seed, bootstrap e configurations.
- `Data/Configurations`: configurações Fluent API por entidade.
- `Infrastructure`: operações, tempo, storage, usuário atual e auditoria.
- `Services`: regras de aplicação e consultas por módulo.
- `Security`: roles, permissões, policies, claims e helpers.
- `Views`: Razor Views organizadas por controller.
- `Views/Shared`: layouts, partials públicas, sidebar, topbar, alerts e componentes reutilizáveis.
- `wwwroot`: CSS, JS, imagens, Bootstrap e jQuery.

## Models/Entities

As entidades ficam em `IkkonAdmin.Web/Models/Entities`. O padrão atual é simples: classes POCO, propriedades públicas e navegações EF Core.

Exemplos:

- `Aluno`, `Turma`, `AlunoTurma`.
- `Mensalidade`, `Pagamento`, `Desconto`, `AcordoFinanceiro`.
- `Admissao`, `Desligamento`, `Graduacao`, `ExameGraduacao`, `HistoricoAluno`.
- `UsuarioSistema`, `RoleSistema`, `PermissaoSistema`.
- `InventarioItem`, `InventarioMovimentacao`.
- `GoogleAgendaConexao`.

## Controllers

Controllers chamam services e fazem validação básica de fluxo, autorização, mensagens via `TempData` e retorno de views.

Padrões observados:

- Actions assíncronas com `CancellationToken`.
- `[ValidateAntiForgeryToken]` em POSTs relevantes.
- `[Authorize(Policy = ...)]` em controllers/actions administrativos.
- Controllers com injeção primária, por exemplo `AlunosController(IAlunoService alunoService)`.

## Views

Views ficam em `Views/{Controller}`. O projeto usa Razor Views com Bootstrap e classes CSS específicas por módulo.

Layouts principais:

- `Views/Shared/_Layout.cshtml`: área administrativa.
- `Views/Shared/_PublicLayout.cshtml`: site público.
- `Views/Shared/_AuthLayout.cshtml`: telas de login.
- `Views/Shared/_AlunoLayout.cshtml`: portal do aluno.

Partials importantes:

- `_Sidebar.cshtml`.
- `_Topbar.cshtml`.
- `_Alerts.cshtml`.
- `_PublicHeader.cshtml`.
- `_PublicFooter.cshtml`.
- `_PublicContactCta.cshtml`.
- `_PublicCourseCards.cshtml`.
- `_PublicVideoGrid.cshtml`.

## DbContext e migrations

O EF Core usa `ApplicationDbContext` com `DbSet` para todas as entidades principais. Configurações são aplicadas via:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```

As migrations ficam em `IkkonAdmin.Web/Data/Migrations`.

Em `Development`, `DatabaseBootstrap.EnsureDatabaseReady` mantém a conveniência
local e executa:

- baseline quando o schema principal já existe;
- `dbContext.Database.Migrate()`;
- garantia do schema `AlunosTurmas`;
- seed estrutural e dados demonstrativos.

Em `Production`, o processo web não altera o schema. A pipeline aplica as
migrations explicitamente antes do deploy; no startup, a aplicação apenas testa
a conexão, recusa migrations pendentes, executa o seed estrutural idempotente e,
quando configurado, o bootstrap único do primeiro administrador.

## Services, queries e operações

O projeto usa Services diretamente com EF Core. Não há camada de repositories separada no estado atual.

Padrão atual:

- `*QueryService` para consultas, listas, filtros e detalhes.
- `*Service` para comandos e regras que alteram estado.
- `OperationResult` ou `OperationResult<T>` para sucesso, validação e `NotFound`.
- `IClock`, `ICurrentUserService`, `IFileStorageService` e `IAuditLogger` para infraestrutura transversal.

Services existentes por módulo:

- `AlunoService`, `TurmaService`, `FinanceiroService`.
- `AdmissaoService`, `DesligamentoService`, `GraduacaoService`.
- `DashboardService`, `ConfiguracaoService`.
- `AuthService`, `AreaAlunoService`, `AdminPainelService`.
- `InventarioService`, `GoogleAgendaService`, `UserSettingsService`.
- `BlogService` e services auxiliares de workflow, idioma, versão, mídia, slug e tags.

Contratos específicos ainda pendentes de migração para `OperationResult`:

- `BlogOperationResult`.
- `AdminOperationResult`.

## Separação entre público e administrativo

- Público: `InstitucionalController`, rotas `/`, `/escola`, `/eventos`, layout público.
- Administrativo: rota `/admin/{controller=Home}/{action=Index}/{id?}`, layout administrativo e policies de funcionário/admin.
- Administração avançada: rota `/admin/painel/{action=Index}/{id?}`, controller `PainelAdminController`.
- Portal do aluno: rotas `/area-do-aluno/{action}` e `/aluno/{action}`, policy `POLICY_ALUNO`.

## Convenções importantes

- Preferir ViewModels para telas e formulários.
- Manter regras de aplicação em Services.
- Usar `AsNoTracking()` em consultas somente leitura.
- Usar `DateOnly` para datas de negócio quando apropriado.
- Usar `IClock.UtcNow` para auditoria e timestamps técnicos em código novo.
- Proteger actions administrativas no backend, não apenas ocultar botões na UI.
- Usar classes CSS por módulo, como `financeiro-v2-*`, `agenda-*`, `inventario-v2-*`.

