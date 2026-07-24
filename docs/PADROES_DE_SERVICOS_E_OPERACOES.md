# Padrões de serviços e operações

Este documento registra os padrões atuais usados para deixar o projeto mais organizado, testável e próximo de uma arquitetura SOLID sem transformar a aplicação MVC em uma arquitetura pesada.

## Objetivo

O IkkonAdmin continua sendo uma aplicação ASP.NET Core MVC server-side. A evolução arquitetural busca:

- manter controllers finos;
- separar consultas de comandos quando o módulo cresce;
- centralizar regras de negócio em services;
- padronizar retorno de operações;
- isolar infraestrutura transversal atrás de interfaces;
- facilitar testes unitários com EF Core InMemory e doubles pequenos.

## Organização por responsabilidade

Controllers devem cuidar de HTTP:

- receber parâmetros;
- validar `ModelState`;
- chamar services;
- traduzir resultado para view, redirect, arquivo ou JSON;
- aplicar `[Authorize]` e `[ValidateAntiForgeryToken]`.

Services de comando devem cuidar de alteração de estado:

- criar, editar, excluir, aprovar, registrar, confirmar e enviar;
- validar regras de negócio;
- persistir via `ApplicationDbContext`;
- retornar `OperationResult` ou `OperationResult<T>`.

Services de consulta devem montar telas e listas:

- nomes terminam em `QueryService`;
- usam `AsNoTracking()` quando possível;
- retornam ViewModels ou listas de ViewModels;
- não devem salvar alterações no banco.

## OperationResult

O padrão atual para operações de comando fica em:

```text
IkkonAdmin.Web/Infrastructure/Operations
```

Tipos principais:

- `OperationResult`: operações sem valor de retorno;
- `OperationResult<T>`: operações que retornam um valor, como o `Id` criado;
- `OperationError`: erro opcionalmente associado a um campo;
- `OperationResultStatus`: `Success`, `ValidationError` ou `NotFound`.

Use `OperationResult` quando o service precisa informar sucesso, falha de validação ou item inexistente sem jogar exceção para fluxos esperados.

Exemplo conceitual:

```csharp
if (string.IsNullOrWhiteSpace(model.Nome))
{
    return OperationResult.Fail("Informe o nome.", nameof(model.Nome));
}

return OperationResult.Ok("Registro salvo com sucesso.");
```

## Integração com controllers

Helpers disponíveis:

- `result.AddToModelState(ModelState)`;
- `result.AddToTempData(TempData)`.

Padrão recomendado:

```csharp
var result = await service.AtualizarAsync(id, model, cancellationToken);

if (result.Status == OperationResultStatus.NotFound)
{
    return NotFound();
}

if (!result.Succeeded)
{
    result.AddToModelState(ModelState);
    return View(model);
}

result.AddToTempData(TempData);
return RedirectToAction(nameof(Index));
```

Para actions JSON ou modais administrativos, o controller pode retornar `BadRequest`, `NotFound` ou `Ok` com base no mesmo status.

## Infraestrutura transversal

Interfaces transversais ficam em `IkkonAdmin.Web/Infrastructure` e `IkkonAdmin.Web/Infrastructure/Security`.

Padrões já disponíveis:

- `IClock` e `SystemClock`: fonte única de tempo para auditoria, vencimentos, filtros e testes.
- `ICurrentUserService` e `HttpCurrentUserService`: leitura centralizada do usuário autenticado.
- `IFileStorageService` e `LocalFileStorageService`: gravação de arquivos com raiz controlada.
- `IAuditLogger` e `EfAuditLogger`: registro de ações sensíveis.

Evite chamar `DateTime.UtcNow`, `User.FindFirstValue` ou lógica de path diretamente em services novos quando já houver uma abstração disponível.

## Autorização

Policies administrativas são registradas por:

```text
AuthorizationPolicyRegistration.AddIkkonPolicies()
```

Tipos relacionados:

- `AppPermissions`;
- `AuthorizationPolicies`;
- `PermissionPolicyDefinition`;
- `PermissionPolicyScope`;
- `AppPermissionEvaluator`.

Regra geral:

- `ROLE_ADMIN` passa nas policies administrativas;
- `ROLE_FUNCIONARIO` precisa da permissão específica;
- `ROLE_ALUNO` usa a policy do portal do aluno;
- permissões devem ser validadas no backend, não apenas escondidas na UI.

## Services já alinhados ao padrão

Módulos com consultas separadas e comandos retornando `OperationResult`:

- alunos;
- turmas;
- financeiro;
- admissões;
- desligamentos;
- graduações;
- inventário;
- área do aluno administrativa;
- configurações de conta;
- autenticação com `OperationResult<AuthSession>`.

Módulos com consulta especializada:

- dashboard;
- painel administrativo;
- Google Agenda;
- blog administrativo e público;
- configurações do sistema.

## Pendências conhecidas

Alguns contratos ainda usam tipos específicos e devem ser migrados em uma próxima etapa, se fizer sentido:

- `BlogOperationResult`, usado pelo fluxo editorial do blog e categorias;
- `AdminOperationResult`, usado por partes do painel administrativo avançado.

Ao migrar, preservar mensagens, status HTTP/redirects e comportamento visual existente.

## Testes

Os testes unitários devem cobrir:

- sucesso;
- validação de negócio;
- `NotFound`;
- filtros de consultas;
- regras de permissão quando aplicável;
- helpers de `OperationResult`.

Comandos usados antes de finalizar mudanças estruturais:

```bash
dotnet build IkkonAdmin.slnx
dotnet test IkkonAdmin.slnx --no-build
```

Depois de build/test, limpar alterações geradas em `bin/obj` antes de commit.
