# Área do Aluno

Este documento descreve o portal do aluno e a manutenção administrativa relacionada.

## Objetivo

A Área do Aluno centraliza informações importantes para o aluno, sem expor dados administrativos internos. O portal usa o usuário autenticado para descobrir o aluno vinculado:

```text
UsuarioSistema.AlunoId -> Aluno.Id
```

O aluno não informa `AlunoId` por rota para acessar dados sensíveis. Essa regra é essencial para evitar acesso cruzado entre alunos.

## Acessos

### Aluno

| Rota | Finalidade |
|---|---|
| `/aluno/login` | Login do aluno |
| `/area-do-aluno` | Dashboard |
| `/area-do-aluno/perfil` | Dados do aluno |
| `/area-do-aluno/financeiro` | Mensalidades |
| `/area-do-aluno/turmas` | Turmas vinculadas |
| `/area-do-aluno/aulas` | Aulas e horários |
| `/area-do-aluno/frequencia` | Histórico de presenças e faltas |
| `/area-do-aluno/eventos` | Eventos vinculados |
| `/area-do-aluno/documentos` | Documentos solicitados e enviados |
| `/area-do-aluno/comunicados` | Comunicados e leitura |
| `/area-do-aluno/conquistas` | Insígnias e marcos |

Policy:

```text
POLICY_ALUNO -> ROLE_ALUNO
```

### Administrativo

| Rota | Finalidade |
|---|---|
| `/admin/area-aluno` | Visão geral operacional |
| `/admin/area-aluno/aulas` | Horários, instrutores e aulas |
| `/admin/area-aluno/frequencia` | Aulas com registro de frequência |
| `/admin/area-aluno/frequencia/{aulaId}` | Registro por aula |
| `/admin/area-aluno/documentos` | Tipos, solicitações e avaliações |
| `/admin/area-aluno/comunicados` | Comunicados |
| `/admin/area-aluno/eventos` | Eventos internos do portal |
| `/admin/area-aluno/conquistas` | Insígnias e atribuições |

## Telas do aluno

### Dashboard

Mostra uma visão rápida com:

- dados do aluno;
- turma atual;
- aulas próximas;
- eventos próximos;
- pendências financeiras;
- documentos pendentes;
- comunicados recentes;
- faltas recentes;
- conquistas recentes;
- alertas importantes.

### Perfil

Exibe dados cadastrais do aluno. Dados sensíveis devem ser tratados como leitura ou alteração controlada pelo administrativo.

Configurações de conta, senha, foto e preferências ficam em `/configuracoes`.

### Financeiro

Mostra mensalidades do próprio aluno:

- competência;
- vencimento;
- valor;
- status;
- data de pagamento quando houver;
- forma de pagamento quando houver.

Dados financeiros devem ser tratados como sensíveis. A Área do Aluno vê apenas seus próprios registros.

### Turmas e aulas

Turmas mostram os vínculos do aluno. Aulas e horários usam:

- `TurmaHorario`;
- `TurmaInstrutor`;
- `Aula`.

O campo legado `Turma.Horario` ainda pode existir para contexto, mas novas funcionalidades devem preferir horários estruturados.

### Frequência

Mostra histórico por período, com:

- presente;
- falta;
- falta justificada;
- observação/justificativa quando aplicável;
- indicadores de presença e faltas.

### Documentos

Fluxo:

1. Admin cria um tipo de documento.
2. Admin solicita documento para aluno.
3. Aluno envia arquivo.
4. Admin aprova ou recusa com observação.
5. Aluno acompanha o status.

Status:

- `Solicitado`
- `Enviado`
- `Aprovado`
- `Recusado`
- `Pendente`

Arquivos aceitos pelo aluno:

- `.pdf`
- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

Limite atual:

```text
10 MB por arquivo
```

Os documentos ficam em:

```text
App_Data/uploads/documentos
```

Esse diretório fica fora de `wwwroot`; o download passa por controller/service para validar autorização.

### Comunicados

Comunicados podem ser:

- globais;
- por turma;
- por aluno.

Recursos:

- importante;
- fixado;
- data de publicação;
- expiração;
- leitura por aluno.

### Eventos

Eventos do portal são internos e persistidos no banco. Eles podem ter alvo:

- todos;
- turma;
- aluno.

O campo `GoogleEventoId` permite referência opcional a evento do Google Agenda, mas a exibição para alunos não deve depender exclusivamente do Google Agenda.

### Conquistas

Conquistas usam:

- `Insignia`: catálogo;
- `AlunoInsignia`: atribuição ao aluno.

Origem:

- `Manual`
- `Automatica`

Regras automáticas devem evitar duplicidade.

## Manutenção administrativa

### Aulas e horários

Entidades:

- `TurmaHorario`: dia da semana, hora início/fim, local e ativo.
- `TurmaInstrutor`: usuário instrutor, turma, principal e período.
- `Aula`: turma, horário opcional, instrutor, início/fim, local, status e observações.

Instrutor é inicialmente um `UsuarioSistema` com perfil administrativo/funcionário, não um novo tipo de usuário.

### Frequência

Entidade:

```text
FrequenciaAluno
```

Campos principais:

- aula;
- aluno;
- status;
- justificada;
- justificativa;
- usuário que registrou;
- data de registro.

O registro deve ser único por aula e aluno.

### Documentos

Entidades:

- `DocumentoTipo`
- `DocumentoSolicitacao`
- `DocumentoEnvio`

Permissões:

- `DOCUMENTOS_VIEW`
- `DOCUMENTOS_CREATE`
- `DOCUMENTOS_EDIT`
- `DOCUMENTOS_APPROVE`

### Comunicados

Entidades:

- `Comunicado`
- `ComunicadoAlvo`
- `ComunicadoLeitura`

Permissões:

- `COMUNICADOS_VIEW`
- `COMUNICADOS_CREATE`
- `COMUNICADOS_EDIT`
- `COMUNICADOS_DELETE`

### Eventos

Entidades:

- `EventoAlunoPortal`
- `EventoAlunoPortalAlvo`

Permissões:

- `EVENTOS_ALUNO_VIEW`
- `EVENTOS_ALUNO_CREATE`
- `EVENTOS_ALUNO_EDIT`
- `EVENTOS_ALUNO_DELETE`

### Conquistas

Entidades:

- `Insignia`
- `AlunoInsignia`

Permissões:

- `CONQUISTAS_VIEW`
- `CONQUISTAS_CREATE`
- `CONQUISTAS_EDIT`

## Permissões administrativas

Grupo principal:

- `AREA_ALUNO_VIEW`
- `AREA_ALUNO_MANAGE`

Submódulos:

- `AULAS_VIEW`, `AULAS_CREATE`, `AULAS_EDIT`
- `FREQUENCIA_VIEW`, `FREQUENCIA_CREATE`, `FREQUENCIA_EDIT`
- `DOCUMENTOS_VIEW`, `DOCUMENTOS_CREATE`, `DOCUMENTOS_EDIT`, `DOCUMENTOS_APPROVE`
- `COMUNICADOS_VIEW`, `COMUNICADOS_CREATE`, `COMUNICADOS_EDIT`, `COMUNICADOS_DELETE`
- `EVENTOS_ALUNO_VIEW`, `EVENTOS_ALUNO_CREATE`, `EVENTOS_ALUNO_EDIT`, `EVENTOS_ALUNO_DELETE`
- `CONQUISTAS_VIEW`, `CONQUISTAS_CREATE`, `CONQUISTAS_EDIT`

## Regras de segurança

- O aluno acessa apenas dados resolvidos pelo usuário autenticado.
- Download de documento do aluno deve validar solicitação e vínculo.
- Download administrativo deve validar permissão.
- Documentos ficam fora de `wwwroot`.
- Financeiro do aluno não deve ser exposto em rotas administrativas genéricas sem permissão financeira.
- Comunicados e eventos devem filtrar por alvo: todos, turma ou aluno.
- Operações sensíveis devem usar antiforgery token.

## Pontos de evolução

- Auditoria mais detalhada para aprovação/recusa de documentos.
- Storage externo para documentos em produção.
- Regras automáticas de conquistas mais robustas.
- Geração automática de aulas futuras a partir de recorrências.
- Permissão limitada para instrutores registrarem frequência apenas nas turmas em que atuam.
