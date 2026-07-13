# Módulos administrativos e permissões

Este documento resume os módulos internos do IkkonAdmin e as permissões associadas.

## Acesso administrativo

O painel administrativo fica sob `/admin` e exige usuário com role:

- `ROLE_ADMIN`, ou
- `ROLE_FUNCIONARIO` com permissão específica.

Admins passam por todas as policies administrativas. Funcionários dependem das claims de permissão atribuídas diretamente ou por cargo.

## Dashboard

Rota principal:

```text
/admin
```

Permissão:

- `DASHBOARD_VIEW`

Funcionalidades:

- indicadores operacionais;
- filtros por competência e turma;
- inadimplência;
- alunos ativos;
- receita recebida;
- atalhos para operações frequentes;
- histórico de atividades recentes.

## Alunos

Rota:

```text
/admin/alunos
```

Permissões:

- `ALUNOS_VIEW`
- `ALUNOS_CREATE`
- `ALUNOS_EDIT`
- `ALUNOS_DELETE`

Funcionalidades:

- cadastro e edição de alunos;
- status do aluno;
- turma principal;
- dados de contato;
- histórico financeiro resumido;
- detalhes administrativos.

## Turmas

Rota:

```text
/admin/turmas
```

Permissões:

- `TURMAS_VIEW`
- `TURMAS_CREATE`
- `TURMAS_EDIT`
- `TURMAS_DELETE`

Funcionalidades:

- cadastro de turmas;
- modalidade e horário legado em texto;
- vínculo de alunos;
- base para horários estruturados do portal do aluno.

## Financeiro

Rota:

```text
/admin/financeiro
```

Permissões:

- `FINANCEIRO_VIEW`
- `FINANCEIRO_CREATE`
- `FINANCEIRO_EDIT`
- `FINANCEIRO_DELETE`

Funcionalidades:

- geração de mensalidades;
- registro de pagamentos;
- controle de atrasos;
- histórico financeiro por aluno;
- descontos e acordos.

Dados financeiros são sensíveis. Funcionários sem permissão financeira não devem visualizar valores, pendências ou históricos.

## Admissões

Rota:

```text
/admin/admissoes
```

Permissões:

- `ADMISSOES_VIEW`
- `ADMISSOES_CREATE`
- `ADMISSOES_EDIT`
- `ADMISSOES_DELETE`

Funcionalidades:

- processo de admissão;
- aula experimental;
- checklist de integração;
- status de matrícula;
- vínculo com aluno quando aplicável.

## Desligamentos

Rota:

```text
/admin/desligamentos
```

Permissões:

- `DESLIGAMENTOS_VIEW`
- `DESLIGAMENTOS_CREATE`
- `DESLIGAMENTOS_EDIT`
- `DESLIGAMENTOS_DELETE`

Funcionalidades:

- registro de desligamento;
- motivo e observações;
- checagem de pendências;
- alteração do status do aluno;
- histórico administrativo.

## Graduações

Rota:

```text
/admin/graduacoes
```

Permissões:

- `GRADUACOES_VIEW`
- `GRADUACOES_CREATE`
- `GRADUACOES_EDIT`
- `GRADUACOES_DELETE`

Funcionalidades:

- exames de graduação;
- nível pretendido;
- resultado;
- histórico de nível do aluno;
- base futura para conquistas automáticas.

## Inventário

Rota:

```text
/admin/inventario
```

Permissões:

- `INVENTARIO_VIEW`
- `INVENTARIO_CREATE`
- `INVENTARIO_EDIT`
- `INVENTARIO_DELETE`
- `INVENTARIO_MANAGE`

Funcionalidades:

- cadastro de taikos, bachis e equipamentos;
- categoria, tipo, código, conservação e localização;
- disponibilidade para aula e evento;
- movimentações;
- baixa/inativação preservando histórico.

## Google Agenda

Rota:

```text
/admin/agenda
```

Permissões:

- `GOOGLE_AGENDA_VIEW`
- `GOOGLE_AGENDA_CREATE`
- `GOOGLE_AGENDA_EDIT`
- `GOOGLE_AGENDA_DELETE`
- `GOOGLE_AGENDA_MANAGE`

Funcionalidades:

- conexão OAuth com Google Agenda;
- listagem de eventos;
- criação, edição e exclusão;
- filtros por período e tipo;
- visualização anual.

Guia específico: [GOOGLE_AGENDA_SETUP.md](../GOOGLE_AGENDA_SETUP.md).

## Blog

Rota:

```text
/admin/blog
```

Permissões:

- `BLOG_VIEW`
- `BLOG_CREATE`
- `BLOG_EDIT`
- `BLOG_PUBLISH`
- `BLOG_ARCHIVE`
- `BLOG_DELETE`
- `BLOG_FEATURE`
- `BLOG_CATEGORY_MANAGE`
- `BLOG_TAG_MANAGE`

Funcionalidades:

- posts com workflow editorial;
- categorias e tags;
- editor rico;
- imagens de capa e imagens no conteúdo;
- agendamento;
- destaque e blog da semana;
- versões por idioma.

Detalhes: [Blog e idiomas](./BLOG_E_IDIOMAS.md).

## Área do Aluno - administração

Rota:

```text
/admin/area-aluno
```

Permissões:

- `AREA_ALUNO_VIEW`
- `AREA_ALUNO_MANAGE`
- `AULAS_VIEW`, `AULAS_CREATE`, `AULAS_EDIT`
- `FREQUENCIA_VIEW`, `FREQUENCIA_CREATE`, `FREQUENCIA_EDIT`
- `DOCUMENTOS_VIEW`, `DOCUMENTOS_CREATE`, `DOCUMENTOS_EDIT`, `DOCUMENTOS_APPROVE`
- `COMUNICADOS_VIEW`, `COMUNICADOS_CREATE`, `COMUNICADOS_EDIT`, `COMUNICADOS_DELETE`
- `EVENTOS_ALUNO_VIEW`, `EVENTOS_ALUNO_CREATE`, `EVENTOS_ALUNO_EDIT`, `EVENTOS_ALUNO_DELETE`
- `CONQUISTAS_VIEW`, `CONQUISTAS_CREATE`, `CONQUISTAS_EDIT`

Funcionalidades:

- horários estruturados;
- instrutores;
- aulas;
- registro de frequência;
- tipos e solicitações de documentos;
- aprovação/recusa de documentos enviados;
- comunicados;
- eventos internos;
- insígnias e atribuições.

Detalhes: [Área do Aluno](./AREA_DO_ALUNO.md).

## Configurações

Rota:

```text
/configuracoes
```

Permissões:

- `CONFIGURACOES_VIEW`
- `CONFIGURACOES_EDIT`

Funcionalidades:

- dados da conta;
- senha;
- foto de perfil;
- tema claro/escuro;
- preferências de notificação;
- histórico de acessos;
- informações de perfil e permissões.

## Administração do sistema

Rota:

```text
/admin/painel
```

Policies administrativas:

- `POLICY_ADMIN_GERENCIAR_USUARIOS`
- `POLICY_ADMIN_GERENCIAR_CARGOS`
- `POLICY_ADMIN_EDITAR_PERMISSOES`
- `POLICY_ADMIN_VISUALIZAR_DADOS`
- `POLICY_ADMIN_GERENCIAR_SISTEMA`

Permissões:

- `GERENCIAR_USUARIOS`
- `GERENCIAR_CARGOS`
- `EDITAR_PERMISSOES`
- `VISUALIZAR_DADOS`
- `GERENCIAR_SISTEMA`

Funcionalidades:

- usuários;
- cargos;
- permissões;
- parâmetros globais;
- logs e auditoria.
