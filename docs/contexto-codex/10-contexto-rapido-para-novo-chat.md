# Contexto rápido para novo chat Codex

O IkkonAdmin é um sistema ASP.NET Core MVC para a IKKON SPTD / Escola de Taiko. Ele tem site público institucional, painel administrativo interno e base inicial para Área do Aluno.

Stack principal:

- .NET `net10.0`.
- ASP.NET Core MVC + Razor Views.
- Entity Framework Core + SQL Server.
- Cookie Authentication.
- Authorization Policies + Claims.
- Bootstrap 5 + CSS customizado em `wwwroot/css/site.css`.

Estrutura:

- `IkkonAdmin.Web/Controllers`.
- `IkkonAdmin.Web/Models/Entities`.
- `IkkonAdmin.Web/Models/ViewModels`.
- `IkkonAdmin.Web/Services`.
- `IkkonAdmin.Web/Data/ApplicationDbContext.cs`.
- `IkkonAdmin.Web/Data/Configurations`.
- `IkkonAdmin.Web/Data/Migrations`.
- `IkkonAdmin.Web/Security`.
- `IkkonAdmin.Web/Views`.

Padrões:

- Controllers finos chamam Services.
- Services usam `ApplicationDbContext` direto; não há repositories.
- Views usam ViewModels.
- Layout admin: `_Layout.cshtml`.
- Layout público: `_PublicLayout.cshtml`.
- Layout aluno: `_AlunoLayout.cshtml`.
- CSS usa prefixos por módulo.

Permissões:

- Roles: `ROLE_ADMIN`, `ROLE_FUNCIONARIO`, `ROLE_ALUNO`.
- Permissões em `AppPermissions.cs`.
- Policies em `AuthorizationPolicies.cs` e `Program.cs`.
- Banco: `UsuariosSistema`, `RolesSistema`, `PermissoesSistema`, `UsuariosRoles`, `RolesPermissoes`, `UsuariosPermissoes`.
- Admin passa em tudo; funcionário depende de claims; aluno usa policy própria.

Uploads:

- Upload real identificado: foto de perfil em `UserSettingsService`.
- Salva em `wwwroot/uploads/perfis`.
- Extensões: JPG/JPEG/PNG/WEBP.
- Máximo: 2 MB.
- Salva URL relativa, ex: `/uploads/perfis/{arquivo}`.
- Não há media library genérica.

Rotas:

- Público: `/`, `/escola`, `/eventos`.
- Login admin: `/auth/login`.
- Admin: `/admin`.
- Painel admin: `/admin/painel`.
- Agenda: `/admin/agenda`.
- Inventário: `/admin/inventario`.
- Configurações: `/configuracoes`.
- Login aluno: `/aluno/login`.
- Portal aluno: `/area-do-aluno`.

Cuidados:

- Não misturar CRUD administrativo de Alunos com Área do Aluno.
- Não expor dados internos em páginas públicas.
- Proteger backend com policies, não só esconder botões.
- Não alterar migrations antigas.
- Não commitar credenciais reais.
- Evitar queries paralelas no mesmo `DbContext`.
- Implementar features novas em fases pequenas e seguindo padrões existentes.

