# Padrões para novas features

## Planejamento

Antes de implementar:

- Identificar se a feature é pública, administrativa ou do aluno.
- Definir entidades e relacionamentos.
- Definir ViewModels.
- Definir permissões.
- Definir rotas.
- Definir telas e estados vazios.
- Definir impactos no banco.
- Implementar em fases pequenas.

## Entidades

Padrão atual:

- Criar entidade em `Models/Entities`.
- Usar propriedades claras e tipos adequados.
- Usar enums em `Enums` quando houver status/tipo fechado.
- Usar navegações EF Core explícitas.
- Usar `DateOnly` para datas de negócio e `IClock.UtcNow` para timestamps técnicos em services novos.

## Configuração EF

Para cada entidade nova:

- Criar configuration em `Data/Configurations`.
- Definir `ToTable`.
- Definir `HasKey`.
- Definir tamanhos de string.
- Definir precisão decimal.
- Definir índices para filtros.
- Definir relacionamentos.

## Migrations

Fluxo esperado:

1. Alterar entities/configurations.
2. Atualizar `ApplicationDbContext` com `DbSet`.
3. Criar migration EF Core.
4. Revisar migration gerada.
5. Rodar build.
6. Testar aplicação local com banco atualizado.

Não alterar migrations antigas já versionadas.

## Services

Padrão atual:

- Criar interface `INomeQueryService` para consultas quando houver listagem, filtros ou detalhes complexos.
- Criar implementação `NomeQueryService`.
- Criar interface `INomeService` para comandos.
- Criar implementação `NomeService`.
- Registrar no DI em `Program.cs`.
- Usar `ApplicationDbContext` diretamente no service.
- Evitar repositories se não houver necessidade clara.
- Retornar `OperationResult` ou `OperationResult<T>` em comandos.
- Usar `IClock` para datas técnicas, `ICurrentUserService` para usuário atual e `IFileStorageService` para uploads.

## Controllers

Padrão:

- Controller fino.
- Injeção do service no construtor primário.
- Actions GET/POST separadas.
- `CancellationToken`.
- `ModelState` para validação de formulário.
- `TempData` para feedback.
- `result.AddToModelState(ModelState)` e `result.AddToTempData(TempData)` quando o service retornar `OperationResult`.
- `[ValidateAntiForgeryToken]` em POSTs.
- `[Authorize(Policy = ...)]` em controller/action.

## Views

Padrão:

- Views em `Views/{Controller}`.
- ViewModels em `Models/ViewModels`.
- Partials para formulários repetidos.
- Layout correto conforme área.
- Classes CSS com prefixo do módulo.
- Estados vazios, loading quando necessário e mensagens claras.

## Permissões

Para módulo administrativo novo:

1. Adicionar permissões em `AppPermissions`.
2. Adicionar policies em `AuthorizationPolicies`.
3. Registrar policies em `Program.cs`.
4. Atualizar seed de acesso em `SeedData`.
5. Proteger backend com `[Authorize]`.
6. Proteger UI com `User.HasPermission`.
7. Testar admin, funcionário com permissão e funcionário sem permissão.

## Padrão visual

Usar como referência:

- Dashboard para KPIs e visão geral.
- Agenda para filtros, abas, forms e calendário.
- Inventário para listagem, KPIs e detalhes.
- Financeiro para tabelas e ações operacionais.

Criar prefixo CSS próprio, por exemplo:

- `blog-admin-*`.
- `blog-public-*`.

## Validação de dados

- Validar no ViewModel com DataAnnotations quando fizer sentido.
- Validar regras de negócio no Service.
- Validar duplicidade no banco quando necessário.
- Validar permissões no backend.
- Validar uploads no backend.

## Como evitar quebrar módulos existentes

- Não alterar rotas antigas sem necessidade.
- Não mexer em layout global se a mudança é específica.
- Não reutilizar entidade administrativa como modelo público diretamente.
- Não expor dados sensíveis em views públicas.
- Não adicionar lógica pesada em views.
- Não criar queries paralelas no mesmo `DbContext`.

## Teste manual mínimo

- Build da solução.
- Abrir tela com admin.
- Abrir tela com usuário sem permissão.
- Testar GET e POST principais.
- Testar validações.
- Testar estado vazio.
- Testar mobile/responsivo quando for UI.
- Testar navegação pela sidebar/header.
- Verificar se o banco recebeu dados esperados.

## Checklist antes de finalizar

- Build sem erros.
- Sem alterações acidentais em `bin/obj`.
- Controllers protegidos.
- POSTs com antiforgery.
- Views usando ViewModels.
- Mensagens de feedback.
- CSS escopado ao módulo.
- Migrations revisadas.
- Seed atualizado somente se necessário.
- Documentar rotas e permissões novas quando a feature for grande.
- Atualizar [Padrões de serviços e operações](../PADROES_DE_SERVICOS_E_OPERACOES.md) quando introduzir um padrão novo.

