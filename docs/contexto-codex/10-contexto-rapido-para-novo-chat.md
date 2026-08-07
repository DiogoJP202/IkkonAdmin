# Contexto rápido para novo chat Codex

O IkkonAdmin é um sistema ASP.NET Core MVC para a IKKON SPTD / Escola de Taiko. Ele tem site público institucional, painel administrativo interno, Área do Aluno e blog público multilíngue.

Stack principal:

- .NET `net10.0`.
- ASP.NET Core MVC + Razor Views.
- Entity Framework Core + SQL Server.
- Cookie Authentication.
- Authorization Policies + Claims.
- Bootstrap 5 + CSS modular em `wwwroot/css/ikkon-*.css`.

Estrutura:

- `IkkonAdmin.Web/Controllers`.
- `IkkonAdmin.Web/Models/Entities`.
- `IkkonAdmin.Web/Models/ViewModels`.
- `IkkonAdmin.Web/Services`.
- `IkkonAdmin.Web/Data/ApplicationDbContext.cs`.
- `IkkonAdmin.Web/Data/Configurations`.
- `IkkonAdmin.Web/Data/Migrations`.
- `IkkonAdmin.Web/Infrastructure`.
- `IkkonAdmin.Web/Security`.
- `IkkonAdmin.Web/Views`.
- `IkkonAdmin.Tests`.

Padrões:

- Controllers finos chamam Services.
- Services usam `ApplicationDbContext` direto; não há repositories.
- Módulos maiores separam `*QueryService` para leitura e `*Service` para comandos.
- Comandos devem retornar `OperationResult` ou `OperationResult<T>` quando forem fluxos esperados de domínio.
- Infraestrutura transversal: `IClock`, `ICurrentUserService`, `IFileStorageService`, `IAuditLogger`.
- Views usam ViewModels.
- Layout admin: `_Layout.cshtml`.
- Layout público: `_PublicLayout.cshtml`.
- Layout aluno: `_AlunoLayout.cshtml`.
- CSS usa prefixos por módulo.
- O painel carrega `ikkon-admin-core.css` e módulos por controller via `AdminCssModuleResolver`; novas rotas precisam entrar nesse mapeamento.

Permissões:

- Roles: `ROLE_ADMIN`, `ROLE_FUNCIONARIO`, `ROLE_ALUNO`.
- Permissões em `AppPermissions.cs`.
- Policies em `AuthorizationPolicies.cs` e `AuthorizationPolicyRegistration.cs`; `Program.cs` chama `AddIkkonPolicies()`.
- Banco: `UsuariosSistema`, `RolesSistema`, `PermissoesSistema`, `UsuariosRoles`, `RolesPermissoes`, `UsuariosPermissoes`.
- Admin passa em tudo; funcionário depende de claims; aluno usa policy própria.

Uploads:

- Mídia pública passa por `IFileStorageService` / `LocalFileStorageService`.
- Foto de perfil em `UserSettingsService`: `wwwroot/uploads/perfis`, até 2 MB.
- Blog em `BlogMediaService`: `wwwroot/uploads/blog/capas` e `wwwroot/uploads/blog/conteudo`.
- Documentos do aluno: `IPrivateFileStorageService`, local fora de `wwwroot` em
  Development e S3 compatível em Production, até 10 MB.
- Extensões comuns: JPG/JPEG/PNG/WEBP; documentos também aceitam PDF.
- Documentos validam extensão, tamanho e assinatura binária; antivírus externo
  ainda não está integrado.
- Não há media library genérica.

Rotas:

- Público: `/`, `/escola`, `/eventos`, `/blog`, `/blog/{slug}`.
- Login admin: `/auth/login`.
- Admin: `/admin`.
- Painel admin: `/admin/painel`.
- Agenda: `/admin/agenda`.
- Inventário: `/admin/inventario`.
- Configurações: `/configuracoes`.
- Login aluno: `/aluno/login`.
- Portal aluno: `/area-do-aluno`.
- Portal aluno também possui `/area-do-aluno/aulas`, `/frequencia`, `/documentos`, `/comunicados`, `/eventos` e `/conquistas`.

Cuidados:

- Não misturar CRUD administrativo de Alunos com Área do Aluno.
- Não expor dados internos em páginas públicas.
- Proteger backend com policies, não só esconder botões.
- Não alterar migrations antigas.
- Não commitar credenciais reais.
- Evitar queries paralelas no mesmo `DbContext`.
- Implementar features novas em fases pequenas e seguindo padrões existentes.
- Consultar `docs/PADROES_DE_SERVICOS_E_OPERACOES.md` antes de criar serviços novos.

