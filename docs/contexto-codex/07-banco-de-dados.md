# Banco de dados

## Estrutura geral

O banco usa SQL Server via Entity Framework Core.

`ApplicationDbContext` define os principais `DbSet`:

- `Alunos`, `AlunosTurmas`, `Turmas`.
- `Mensalidades`, `Pagamentos`, `Descontos`, `AcordosFinanceiros`.
- `Admissoes`, `Desligamentos`, `Graduacoes`, `ExamesGraduacao`, `HistoricosAlunos`.
- `ConfiguracoesSistema`.
- `UsuariosSistema`, `RolesSistema`, `PermissoesSistema`.
- `UsuariosRoles`, `RolesPermissoes`, `UsuariosPermissoes`.
- `AuditoriaLogs`.
- `InventarioItens`, `InventarioMovimentacoes`.
- `GoogleAgendaConexoes`.
- `BlogPosts`, `BlogCategories`, `BlogTags`, `BlogPostTags`.
- `TurmaHorarios`, `TurmaInstrutores`, `Aulas`, `FrequenciasAlunos`.
- `EventosAlunoPortal`, `EventosAlunoPortalAlvos`.
- `Comunicados`, `ComunicadosAlvos`, `ComunicadosLeituras`.
- `DocumentoTipos`, `DocumentoSolicitacoes`, `DocumentoEnvios`.
- `Insignias`, `AlunoInsignias`.

## Configurações EF Core

As configurações ficam em `IkkonAdmin.Web/Data/Configurations`.

Padrão:

- `ToTable` com nome plural em português ou nome específico.
- `HasMaxLength` em strings.
- `HasPrecision` ou `decimal(x,y)` em valores financeiros.
- `HasColumnType("date")` para `DateOnly`.
- `HasColumnType("datetime2")` para timestamps técnicos.
- Índices em campos de busca e relacionamentos.

## Relacionamentos importantes

- `Aluno` 1:N `Mensalidade`.
- `Aluno` 1:N `Pagamento`.
- `Aluno` 1:N `Desconto`.
- `Aluno` 1:N `AcordoFinanceiro`.
- `Aluno` 1:N `Admissao`.
- `Aluno` 1:N `Desligamento`.
- `Aluno` 1:N `Graduacao`.
- `Aluno` N:N `Turma` via `AlunoTurma`.
- `Mensalidade` 1:N `Pagamento`.
- `RoleSistema` N:N `PermissaoSistema` via `RolePermissao`.
- `UsuarioSistema` N:N `RoleSistema` via `UsuarioRole`.
- `UsuarioSistema` N:N `PermissaoSistema` via `UsuarioPermissao`.
- `UsuarioSistema` 0:1 `Aluno` via `AlunoId`.
- `InventarioItem` 1:N `InventarioMovimentacao`.
- `GoogleAgendaConexao` referencia usuário que conectou.
- `Turma` 1:N `TurmaHorario`, `TurmaInstrutor` e `Aula`.
- `Aula` 1:N `FrequenciaAluno`; cada aluno possui no máximo uma frequência por aula.
- `DocumentoSolicitacao` 1:N `DocumentoEnvio`.
- `Comunicado` e `EventoAlunoPortal` possuem alvos por aluno, turma ou público geral.
- `Aluno` N:N `Insignia` via `AlunoInsignia`.
- `BlogPost` N:N `BlogTag` via `BlogPostTag` e pertence opcionalmente a uma categoria.

## Índices e restrições relevantes

- `Aluno.CPF` único.
- `Mensalidade` possui índice único por `{AlunoId, Competencia}`.
- `UsuarioSistema.LoginNormalizado` único.
- `UsuarioSistema.EmailNormalizado` único.
- `UsuarioSistema.AlunoId` único quando não nulo.
- `RoleSistema.Codigo` único.
- `PermissaoSistema.Codigo` único.
- `InventarioItem.CodigoInterno` único filtrado quando preenchido/ativo, conforme configuration.
- `InventarioItem` possui índices para categoria, tipo, status e ativo.
- `FrequenciaAluno` possui índice único por `{AulaId, AlunoId}`.
- `AlunoInsignia` possui índice único por `{AlunoId, InsigniaId}`.
- `Aula` possui índice único filtrado por `{TurmaHorarioId, DataOcorrenciaRecorrencia}` para impedir duplicidade na geração recorrente.
- `BlogPost.Slug`, `BlogCategory.Name`, `BlogCategory.Slug`, `BlogTag.Name` e `BlogTag.Slug` são únicos.
- `DocumentoTipo.Nome` e `Insignia.Nome` são únicos.

## Campos recorrentes

Padrões encontrados:

- `Ativo`: usado em turmas, descontos, acordos, cargos, permissões, usuários e inventário.
- `Status`: usado em aluno, mensalidade, admissão, inventário.
- `CriadoEmUtc`, `AtualizadoEmUtc`: usado em inventário e conexões Google.
- `DataCriacaoUtc`: usado em usuários, roles e permissões.
- `UltimoLoginUtc`: usado em usuários.
- `Excluido`, `DataExclusaoUtc`: soft delete de usuários.
- `DataEventoUtc`: auditoria.

Não há um padrão universal de `CreatedAt/UpdatedAt` para todas as entidades legadas.

## Migrations

As migrations ficam em `IkkonAdmin.Web/Data/Migrations`.

Principais migrations identificadas:

- `InitialCreate`.
- `AddAlunoTurmasManyToMany`.
- `AddConfiguracoesSistema`.
- `AddAuthUsuariosSistema`.
- `AddUserSettingsProfileFields`.
- `AddAdminAccessControlAndAudit`.
- `AddRoleTipoAcessoAndPermissionExpansion`.
- `AddGoogleAgendaAndInventario`.
- `AddGoogleAgendaOAuthConnection`.
- `AddBlogModuleBase`.
- `AddAreaAlunoPortal`.
- `AddBlogPostLanguageVersions`.
- `AddAuditCorrelation`.
- `AddStudentAutomations`.

## Aplicação de migrations

Em `Development`, `DatabaseBootstrap.EnsureDatabaseReady` executa
`dbContext.Database.Migrate()` e preserva as compatibilidades locais de baseline
e de `AlunosTurmas`.

Em `Production`, migrations são uma etapa explícita e bloqueante da pipeline. O
processo web não executa reparos nem migrations: ele verifica a conectividade e
encerra o startup quando há migrations pendentes. Consulte
`docs/PRODUCTION_RUNBOOK.md` para deploy, rollback e restauração.

## Seed inicial

`SeedData.InitializeStructural` cria somente:

- Configurações padrão.
- Roles e permissões base.

Em `Development`, `SeedData.Initialize` complementa o seed estrutural com:

- Turmas e alunos demo.
- Mensalidades, pagamentos, descontos e acordos.
- Admissão, desligamento e graduação demo.
- Inventário demo.
- Usuários demo.

Produção nunca recebe dados ou credenciais demonstrativas. O primeiro
administrador pode ser criado uma única vez por variáveis secretas de bootstrap.

## Cuidados antes de alterar o banco

- Criar migration EF Core para novas tabelas/campos.
- Revisar `ApplicationDbContext`.
- Criar configuration em `Data/Configurations`.
- Atualizar `SeedData` somente quando necessário.
- Evitar alterar migrations antigas já aplicadas.
- Verificar impacto em banco Azure/produção.
- Não remover colunas/tabelas sem plano de migração de dados.
- Manter índices coerentes para telas com filtros.
