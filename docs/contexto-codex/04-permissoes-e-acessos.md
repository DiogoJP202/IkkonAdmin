# Permissões e acessos

## Login

O sistema usa Cookie Authentication configurado em `Program.cs`.

Configuração principal:

- Login administrativo: `/auth/login`.
- Acesso negado: `/auth/acesso-negado`.
- Logout administrativo: `/auth/logout`.
- Cookie: `ikkonadmin.auth`.
- Expiração: 8 horas com sliding expiration.

Fluxos:

- `AuthController`: login de admin/funcionário.
- `AlunoAuthController`: login separado da Área do Aluno em `/aluno/login`.
- `AuthService`: valida login/e-mail, senha, tipo de acesso, usuário ativo e registra auditoria de login.
- `AuthClaimsFactory`: monta claims, roles e permissões no cookie.

## Perfis/cargos

Roles fixas em código:

- `ROLE_ADMIN`.
- `ROLE_FUNCIONARIO`.
- `ROLE_ALUNO`.

Entidades:

- `RoleSistema`: cargo/perfil.
- `UsuarioRole`: vínculo usuário/cargo.
- `PermissaoSistema`: permissão cadastrada no banco.
- `RolePermissao`: permissões do cargo.
- `UsuarioPermissao`: permissões diretas do usuário.

`TipoAcessoEnum` define o tipo principal da conta:

- `Funcionario`.
- `Aluno`.
- `Admin`.

## Verificação de permissões

As policies são definidas em `Program.cs`, usando constantes de `AuthorizationPolicies`.

Padrão:

- Admin passa automaticamente em permissões operacionais.
- Funcionário precisa da role `ROLE_FUNCIONARIO` e da claim de permissão.
- Aluno usa `AuthorizationPolicies.Aluno`.
- Configurações exigem usuário autenticado e permissões de configuração.

Helpers:

- `ClaimsPrincipalExtensions.HasPermission`.
- `ClaimsPrincipalExtensions.HasAnyPermission`.

## Onde ficam permissões

Em código:

- `Security/AppPermissions.cs`: códigos e definições.
- `Security/AuthorizationPolicies.cs`: nomes das policies.
- `Program.cs`: registro das policies.
- `SeedData.SeedAccessControl`: sincronização inicial no banco.

No banco:

- `PermissoesSistema`.
- `RolesSistema`.
- `RolesPermissoes`.
- `UsuariosRoles`.
- `UsuariosPermissoes`.

## Como proteger novas telas administrativas

1. Criar constantes em `AppPermissions`.
2. Criar constantes em `AuthorizationPolicies`.
3. Registrar a policy em `Program.cs`.
4. Sincronizar permissões no seed em `SeedData`.
5. Proteger controller/action com `[Authorize(Policy = AuthorizationPolicies.NomeDaPolicy)]`.
6. Esconder botões na view usando `User.HasPermission(...)` ou `User.HasAnyPermission(...)`.
7. Validar também actions POST no backend.

## Exemplo para nova feature

Exemplo conceitual para um módulo `Blog`:

```csharp
// AppPermissions.cs
public const string BlogView = "BLOG_VIEW";
public const string BlogCreate = "BLOG_CREATE";
public const string BlogEdit = "BLOG_EDIT";
public const string BlogDelete = "BLOG_DELETE";

// AuthorizationPolicies.cs
public const string BlogView = "POLICY_BLOG_VIEW";

// Program.cs
AddFuncionarioPermissionPolicy(options, AuthorizationPolicies.BlogView, AppPermissions.BlogView);
```

Uso no controller:

```csharp
[Authorize(Policy = AuthorizationPolicies.Funcionario)]
[Authorize(Policy = AuthorizationPolicies.BlogView)]
public class BlogAdminController : Controller
{
}
```

Uso na view:

```csharp
@if (User.HasPermission(AppPermissions.BlogCreate))
{
    <a class="btn btn-primary" href="/admin/blog/criar">Novo post</a>
}
```

## Cuidados

- Não confiar apenas no frontend.
- Não proteger um módulo novo só por esconder links na sidebar.
- Admin deve continuar recebendo todas as permissões em `AuthService.ObterPermissoesAsync`.
- Ao criar permissão nova, garantir que ela seja incluída em `AppPermissions.Definicoes`.
- Revisar `SeedData.SeedAccessControl` para roles padrão.
- Usar `[ValidateAntiForgeryToken]` em POSTs.

