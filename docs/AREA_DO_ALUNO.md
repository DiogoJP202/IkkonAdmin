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

## Arquitetura do módulo

O portal do aluno usa um serviço de fachada para manter compatibilidade com os controllers e services menores para cada responsabilidade.

Contexto seguro:

- `AreaAlunoContextService`: resolve o aluno pelo usuário autenticado e pelo vínculo `UsuarioSistema.AlunoId`.

Services do aluno:

- `AreaAlunoPerfilService`;
- `AreaAlunoFinanceiroService`;
- `AreaAlunoTurmasService`;
- `AreaAlunoFrequenciaService`;
- `AreaAlunoEventosService`;
- `AreaAlunoDocumentosService`;
- `AreaAlunoComunicadosService`;
- `AreaAlunoConquistasService`.

Services administrativos:

- `AreaAlunoAulasAdminService`;
- `AreaAlunoDocumentoAdminService`;
- `AreaAlunoComunicadoAdminService`;
- `AreaAlunoEventoAdminService`;
- `AreaAlunoConquistaAdminService`.

Operações administrativas e envio de documentos usam `OperationResult`. Consultas retornam ViewModels específicos para evitar exposição de entidades completas.

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

## Padrão visual e de implementação

O portal autenticado compartilha a identidade navy, creme e vermelho do frontend
público, mas mantém uma densidade adequada para consulta frequente. Dados, rotas,
permissões e formulários continuam independentes da camada de apresentação.

### Shell e navegação

- `_AlunoLayout` é o único shell do portal e não renderiza `h1`.
- A sidebar permanece fixa a partir de 992 px.
- Abaixo de 992 px ela vira uma gaveta com backdrop, Escape, armadilha de foco,
  retorno de foco e bloqueio de rolagem.
- O seletor de idioma oferece PT, EN e JA preservando a rota de retorno.
- A topbar mostra somente contexto compacto; o título principal pertence à página.
- Alvos de toque possuem no mínimo 44 px e o foco visível usa vermelho institucional.

Exemplo:

```cshtml
@{
    var pageHeader = new AlunoPageHeaderViewModel(
        I18n["Contexto", "Context", "コンテキスト"],
        I18n["Título", "Title", "タイトル"],
        I18n["Descrição.", "Description.", "説明。"]);
}

<partial name="_AlunoPageHeader" model="pageHeader" />
```

Evitar criar cabeçalho, seletor de idioma ou menu próprios dentro de uma view.

### Componentes compartilhados

| Componente | Finalidade | Variações | Regra de uso | Evitar |
| --- | --- | --- | --- | --- |
| `_AlunoPageHeader` | Introdução semântica e único `h1` | Metadado opcional | Usar no início de toda página de conteúdo | Repetir a marcação ou adicionar outro `h1` |
| `_AlunoMetricCard` | Indicador numérico ou textual | `success`, `warning`, `danger`, `info`, `neutral` | Sempre fornecer rótulo; dica é opcional | Comunicar estado somente pela cor |
| `_AlunoStatusBadge` | Status compacto | Mesmos cinco tons semânticos | Texto vem de `I18n.Term`; tom vem do helper | Classes montadas a partir de texto traduzido |
| `.aluno-portal-panel` | Agrupar uma responsabilidade | Cabeçalho, corpo, vazio | Um painel por assunto | Card aninhado sem necessidade |
| `.aluno-portal-responsive-table` | Dados tabulares densos | Tabela desktop, cartões no mobile | Cada `td` deve ter `data-label` | Ocultar coluna ou depender de scroll horizontal |
| `.aluno-portal-button` | Ações do portal | Primário, secundário e link | Preservar elemento nativo e estado disabled/loading | Novo botão com medidas próprias |

`AlunoPortalPresentation.StatusTone` converte enums e status conhecidos para tons
semânticos. Valores desconhecidos usam `neutral`, preservando texto e legibilidade.

### Tipografia, cores e forma

| Padrão | Uso | Regra |
| --- | --- | --- |
| Lato | Corpo, formulários, navegação e metadados | `var(--ikkon-font-body)` |
| Cinzel | Títulos, marca e números editoriais | `var(--ikkon-font-brand)` |
| Noto Serif JP / Zen Kaku Gothic New | Título/corpo em japonês | Carregadas somente quando `lang="ja"` |
| Navy `#00203e` | Títulos, sidebar e ações fortes | Token `--ikkon-navy` |
| Creme `#f7f4e7` | Fundo claro editorial | Token `--ikkon-cream` |
| Vermelho `#e73439` | Ação, foco e estado ativo | Token `--ikkon-red`; não usar como texto longo |
| Bordas | Separação entre superfícies | 1 px; cards internos não recebem raios decorativos |
| Espaçamento | Ritmo entre blocos | Preferir `--ikkon-space-*` e gaps fluidos existentes |
| Elevação | Separação excepcional | Sem sombra por padrão; não criar níveis arbitrários |

O tema escuro substitui superfícies por navy profundo, mantém texto creme e conserva
vermelho como ação. As regras são escopadas por `body.aluno-theme-dark`, sem modificar
`body.admin-theme-dark`.

### Formulários, feedback e estados

- Todo campo possui `label` associado e conserva nome, método e antiforgery token.
- `focus`, `hover`, `active`, `disabled` e `loading` devem permanecer textuais e
  navegáveis por teclado.
- Configurações recebe mensagens de loading, sucesso, erro e rede por atributos
  `data-*` traduzidos; JavaScript não contém mensagem fixa em português.
- Estados vazio, erro, bloqueado e sucesso usam título e explicação, nunca só ícone
  ou cor.
- Uploads ocupam a largura disponível no celular e mantêm extensão/limite visíveis.
- Textos externos e dados longos usam quebra segura em vez de corte.

### Imagens e ícones

- Marca, selo, enso, textura e fotos existentes ficam em `wwwroot/design-ikkon`.
- Imagens de conteúdo exigem `alt`; elementos decorativos usam `alt=""` ou CSS.
- Conquistas sem ícone usam o fallback textual `魂`.
- O portal não adiciona biblioteca de ícones: controles usam símbolos existentes e
  texto acessível.

### Responsividade

| Faixa | Comportamento |
| --- | --- |
| ≥1200 px | Sidebar fixa, grids completos e conteúdo com largura confortável |
| 992–1199.98 px | Grids intermediários e redução controlada de densidade |
| 768–991.98 px | Gaveta móvel e componentes em até duas colunas |
| 480–767.98 px | Uma coluna; tabelas viram cartões; ações e filtros empilham |
| 320–479.98 px | Labels tabulares acima do valor e ações em largura total |

Nenhuma faixa pode esconder informação ou ação. `prefers-reduced-motion: reduce`
elimina transições perceptíveis, e a página não deve apresentar overflow horizontal.

### Estrutura recomendada para nova página

1. Criar ViewModel de negócio no módulo responsável.
2. Configurar `Layout = "_AlunoLayout"` e `ViewData["Title"]` com três idiomas.
3. Compor `AlunoPageHeaderViewModel` e renderizar `_AlunoPageHeader`.
4. Reutilizar painel, métrica e badge antes de criar nova marcação.
5. Em tabelas, adicionar `aluno-portal-responsive-table` e `data-label` em cada célula.
6. Preservar ações, antiforgery e autorização existentes.
7. Validar claro/escuro, PT/EN/JA, teclado e larguras de 320 a 1440 px.

Convenções:

- CSS e hooks visuais: `aluno-portal-*`;
- comportamento JavaScript: atributos `data-aluno-*`;
- variação de estado: `is-*`;
- propriedades visuais compartilhadas: records `Aluno*ViewModel`;
- regras de negócio não devem ser movidas para partials de apresentação.

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
- `AlunoId` recebido por rota ou formulário não deve ser usado em consultas sensíveis do portal do aluno.
- Download de documento do aluno deve validar solicitação e vínculo.
- Download administrativo deve validar permissão.
- Documentos ficam fora de `wwwroot`.
- Financeiro do aluno não deve ser exposto em rotas administrativas genéricas sem permissão financeira.
- Comunicados e eventos devem filtrar por alvo: todos, turma ou aluno.
- Operações sensíveis devem usar antiforgery token.
- Services devem receber o usuário atual via `ICurrentUserService` ou parâmetro controlado pelo controller, nunca por campo editável pelo usuário.

## Pontos de evolução

- Auditoria mais detalhada para aprovação/recusa de documentos.
- Storage externo para documentos em produção.
- Regras automáticas de conquistas mais robustas.
- Geração automática de aulas futuras a partir de recorrências.
- Permissão limitada para instrutores registrarem frequência apenas nas turmas em que atuam.
