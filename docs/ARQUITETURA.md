# Arquitetura e convenções

Este documento descreve a estrutura técnica do IkkonAdmin e as principais decisões de organização do código.

## Visão geral

O projeto é uma aplicação **ASP.NET Core MVC server-side**, com Razor Views e Entity Framework Core. O frontend e o backend rodam no mesmo processo, o que simplifica deploy, autenticação, autorização e renderização das telas administrativas.

```text
IkkonAdmin.Web
├── Controllers       # Entrada HTTP e orquestração de telas
├── Data              # DbContext, configurations e migrations
├── Enums             # Estados e classificações de domínio
├── Models
│   ├── Entities      # Modelo persistido no banco
│   └── ViewModels    # Modelos específicos para telas/formulários
├── Infrastructure    # Operações, tempo, storage, usuário atual e auditoria
├── Security          # Roles, permissões, claims e policies
├── Services          # Regras de negócio e consultas
├── Views             # Razor Views
└── wwwroot           # CSS, JS, imagens e uploads públicos
```

## Camadas

### Controllers

Controllers devem ser finos. Eles validam autorização, recebem parâmetros, chamam services e retornam views, redirects, arquivos ou JSON.

Exemplos:

- `HomeController`: dashboard administrativo.
- `AlunoAreaController`: portal do aluno.
- `AreaAlunoAdminController`: manutenção administrativa do portal do aluno.
- `BlogAdminController`: editor e workflow editorial do blog.
- `BlogController`: blog público.
- `PainelAdminController`: usuários, cargos, permissões, sistema e auditoria.

### Services

Services concentram regras de negócio e consultas. A preferência do projeto é usar services específicos por módulo, em vez de repositories genéricos.

Nos módulos maiores, o projeto separa leitura e escrita:

- `*QueryService`: monta listas, detalhes, filtros e dashboards, preferencialmente com `AsNoTracking()`.
- `*Service`: executa comandos, valida regras de negócio, altera o banco e retorna `OperationResult` ou `OperationResult<T>`.

Exemplos:

- `AlunoQueryService` e `AlunoService`
- `TurmaQueryService` e `TurmaService`
- `FinanceiroQueryService` e `FinanceiroService`
- `InventarioQueryService` e `InventarioService`
- `AreaAlunoService` e services especializados do portal
- `AreaAlunoAdminService` e services administrativos especializados
- `BlogService`
- `BlogAdminQueryService`
- `BlogPublicService`
- `BlogWorkflowService`
- `BlogVersionService`
- `BlogCategoriaService` e `BlogMediaService`
- `DashboardService`
- `GoogleAgendaService`
- `UserSettingsService`

Detalhes do padrão: [Padrões de serviços e operações](./PADROES_DE_SERVICOS_E_OPERACOES.md).

### ViewModels

As views devem receber ViewModels específicos, não entidades cruas quando houver combinação de dados, filtros, permissões ou agregações.

Isso reduz exposição indevida de dados e mantém cada tela com um contrato claro.

### Data

O `ApplicationDbContext` centraliza os `DbSet`s. As configurações de entidade ficam em `Data/Configurations`, aplicadas por:

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
```

O sistema também aplica query filter para usuários excluídos:

```csharp
modelBuilder.Entity<UsuarioSistema>().HasQueryFilter(x => !x.Excluido);
```

## Roteamento

O projeto combina rotas convencionais e rotas por atributo.

### Rotas públicas

| Rota | Controller | Finalidade |
|---|---|---|
| `/` | `InstitucionalController.Index` | Home institucional |
| `/escola` | `InstitucionalController.Escola` | Aulas e cursos |
| `/eventos` | `InstitucionalController.Eventos` | Apresentações e eventos |
| `/blog` | `BlogController.Index` | Listagem pública do blog |
| `/blog/{slug}` | `BlogController.Details` | Detalhe de post |
| `/idioma/alterar` | `IdiomaController.Alterar` | Troca de idioma por cookie |

### Rotas administrativas

| Rota | Finalidade |
|---|---|
| `/auth/login` | Login administrativo |
| `/admin` | Dashboard operacional |
| `/admin/painel` | Administração de usuários, cargos, sistema e auditoria |
| `/admin/blog` | Blog administrativo |
| `/admin/blog/categorias` | Categorias do blog |
| `/admin/area-aluno` | Operação administrativa do portal do aluno |
| `/configuracoes` | Conta, senha e preferências do usuário autenticado |

### Rotas do aluno

| Rota | Finalidade |
|---|---|
| `/aluno/login` | Login do aluno |
| `/area-do-aluno` | Dashboard do aluno |
| `/area-do-aluno/perfil` | Perfil |
| `/area-do-aluno/financeiro` | Mensalidades |
| `/area-do-aluno/turmas` | Turmas |
| `/area-do-aluno/aulas` | Aulas e horários |
| `/area-do-aluno/frequencia` | Frequência |
| `/area-do-aluno/eventos` | Eventos |
| `/area-do-aluno/documentos` | Documentos |
| `/area-do-aluno/comunicados` | Comunicados |
| `/area-do-aluno/conquistas` | Conquistas |

## Autenticação e autorização

O sistema usa autenticação por cookie:

```text
Cookie: ikkonadmin.auth
Login administrativo: /auth/login
Login do aluno: /aluno/login
Logout: /auth/logout
Access denied: /auth/acesso-negado
```

Roles principais:

- `ROLE_ADMIN`
- `ROLE_FUNCIONARIO`
- `ROLE_ALUNO`

Permissões são claims do tipo definido em `AppClaimTypes.Permissao`.

O registro das policies fica centralizado em:

```text
AuthorizationPolicyRegistration.AddIkkonPolicies()
```

Tipos de apoio:

- `PermissionPolicyDefinition`;
- `PermissionPolicyScope`;
- `AppPermissionEvaluator`;
- `IBlogPostActionAuthorizer`.

Regras gerais:

- `Admin` passa por todas as policies administrativas.
- Funcionário precisa da permissão específica do módulo.
- `Aluno` acessa somente o portal do aluno.
- O portal do aluno nunca deve confiar em `AlunoId` vindo por rota para dados sensíveis.

## Infraestrutura transversal

Pasta:

```text
IkkonAdmin.Web/Infrastructure
```

Componentes principais:

- `IClock`: fonte de tempo injetável para regras e testes.
- `ICurrentUserService`: usuário autenticado atual sem espalhar leitura de claims pelos controllers/services.
- `IFileStorageService`: gravação de arquivos em storage local controlado.
- `IAuditLogger`: registro de ações sensíveis em `AuditoriaLog`.
- `OperationResult`: contrato padronizado de sucesso, validação e item não encontrado.

Essas abstrações devem ser preferidas em novas features para reduzir acoplamento com HTTP, filesystem e relógio do sistema.

## Internacionalização

O pipeline suporta:

- `pt-BR`
- `en-US`
- `ja-JP`

A troca acontece por `/idioma/alterar?culture=<cultura>&returnUrl=<url>`, usando cookie de request culture.

As views usam `IViewTextService`:

```csharp
I18n["Texto PT", "Text EN"]
I18n["Texto PT", "Text EN", "日本語"]
```

O japonês deve ser habilitado por contexto de página para evitar expor versões incompletas em áreas que ainda não foram traduzidas.

## Banco de dados

Principais grupos de entidades:

- Operação escolar: `Aluno`, `Turma`, `AlunoTurma`, `HistoricoAluno`.
- Financeiro: `Mensalidade`, `Pagamento`, `Desconto`, `AcordoFinanceiro`.
- Ciclo do aluno: `Admissao`, `Desligamento`, `Graduacao`, `ExameGraduacao`.
- Inventário: `InventarioItem`, `InventarioMovimentacao`.
- Agenda: `GoogleAgendaConexao`.
- Usuários e permissões: `UsuarioSistema`, `RoleSistema`, `PermissaoSistema`, `UsuarioRole`, `RolePermissao`, `UsuarioPermissao`, `AuditoriaLog`.
- Blog: `BlogPost`, `BlogCategory`, `BlogTag`, `BlogPostTag`.
- Área do Aluno: `TurmaHorario`, `TurmaInstrutor`, `Aula`, `FrequenciaAluno`, `EventoAlunoPortal`, `Comunicado`, `DocumentoSolicitacao`, `DocumentoEnvio`, `Insignia`, `AlunoInsignia`.

## Convenções de manutenção

- Adicionar novas regras em services, não diretamente nas views.
- Separar `QueryService` de comandos quando a tela tiver consultas complexas ou o service começar a acumular responsabilidades.
- Retornar `OperationResult` em operações esperadas de criação, edição, exclusão, aprovação, envio e confirmação.
- Criar ViewModel dedicado para telas com combinação de dados.
- Adicionar permissão em `AppPermissions`, policy em `AuthorizationPolicies`/`AuthorizationPolicyRegistration` e seed quando houver novo módulo protegido.
- Preferir migrations pequenas e nomeadas pelo comportamento.
- Evitar expor arquivos privados em `wwwroot`.
- Usar `TempData["Success"]` e `TempData["Error"]` para feedback simples de formulários.
- Para formulários sensíveis, manter `[ValidateAntiForgeryToken]`.
