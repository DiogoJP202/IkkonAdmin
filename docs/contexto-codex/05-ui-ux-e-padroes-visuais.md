# UI, UX e padrões visuais

## Painel administrativo

O painel administrativo usa `Views/Shared/_Layout.cshtml`.

Estrutura:

- `body.admin-body`.
- `admin-shell`.
- Sidebar fixa em `_Sidebar.cshtml`.
- Topbar em `_Topbar.cshtml`.
- Conteúdo em `admin-main`.
- Alertas em `_Alerts.cshtml`.

O padrão visual atual é de sistema administrativo interno: cards, filtros, tabelas, botões objetivos, indicadores e ações rápidas.

## Site público

O site público usa `Views/Shared/_PublicLayout.cshtml`.

Características:

- Fonte externa: `Noto Serif JP` e `Zen Kaku Gothic New`.
- Identidade off-white, grafite/preto e vermelho profundo.
- Header público em `_PublicHeader.cshtml`.
- Footer público em `_PublicFooter.cshtml`.
- Hero visual com imagens grandes.
- Seções institucionais com cards, vídeos, mapa e CTA.

Views públicas principais:

- `Views/Institucional/Index.cshtml`.
- `Views/Institucional/Escola.cshtml`.
- `Views/Institucional/Eventos.cshtml`.

## Área do Aluno

Usa `Views/Shared/_AlunoLayout.cshtml`.

Estrutura:

- `body.aluno-portal-body`.
- Sidebar própria do aluno.
- Links: Início, Meu perfil, Financeiro, Turmas, Sair.
- Visual separado do painel administrativo para evitar mistura entre CRUD interno e portal do aluno.

## Layouts existentes

- `_Layout.cshtml`: admin.
- `_PublicLayout.cshtml`: institucional.
- `_AuthLayout.cshtml`: login.
- `_AlunoLayout.cshtml`: portal do aluno.

## Componentes reutilizados

- `_Alerts.cshtml`: mensagens de sucesso/erro.
- `_ValidationScriptsPartial.cshtml`: validação client-side.
- `_PublicContactCta.cshtml`: CTA público.
- `_PublicCourseCards.cshtml`: cards de cursos.
- `_PublicFaqAlunos.cshtml`: FAQ de alunos.
- `_PublicVideoGrid.cshtml`: grid de vídeos.
- `_PublicEventosGallery.cshtml`: galeria/carrossel de eventos.
- `_AdminNav.cshtml`: navegação interna do painel administrativo.

## Padrões de CSS

O CSS fica em `wwwroot/css/ikkon-*.css`. Os layouts carregam apenas as camadas necessárias:

- público: fundação, tokens, editorial, composições e responsividade;
- autenticação: fundação interna e `ikkon-auth.css`;
- painel: fundação interna, `ikkon-admin-core.css`, módulos do controller e temas;
- aluno: fundação interna, `ikkon-aluno.css` e temas;
- configurações: acrescenta `ikkon-account.css` no layout correspondente ao perfil.

A ordem detalhada está em `docs/frontend-public/INTERNAL_CSS_ARCHITECTURE.md`.

O mapeamento das rotas administrativas fica em `Helpers/AdminCssModuleResolver.cs`. Ao criar um controller do painel, registrar seus módulos nesse resolver e manter a sequência `core → domínio → temas`.

Há blocos por módulo, com prefixos específicos:

- `dashboard-v2-*`.
- `alunos-v2-*`.
- `turmas-v2-*`.
- `financeiro-v2-*`.
- `agenda-*`.
- `inventario-v2-*`.
- `configuracoes-v2-*`.
- `institucional-*`.
- `public-*`.
- `aluno-portal-*`.

Ao criar nova tela, preferir um prefixo próprio do módulo para evitar colisões.

## Cards, tabelas, botões e forms

Padrões recorrentes:

- Hero ou cabeçalho de página com título, descrição e ações.
- KPIs em cards no topo.
- Filtros em cards separados.
- Tabelas com hover, badges/status e ações por linha.
- Estados vazios explícitos.
- Botões primários, secundários/ghost e perigo com classes do módulo.
- Inputs com labels, placeholders e foco visual.
- Forms divididos em seções.

## Modais

O projeto usa Bootstrap, mas muitas ações são feitas por páginas/formulários dedicados e POSTs. Quando usar modal, manter acessibilidade, labels, foco e evitar lógica crítica apenas no frontend.

## Responsividade

Boas práticas já usadas:

- Grids CSS com media queries.
- Layouts que colapsam para uma coluna em mobile.
- Botões em largura total em telas pequenas.
- Tabelas dentro de containers responsivos.
- Cards com espaçamento consistente.

## Recomendações para novas telas

- Seguir o visual do módulo mais próximo. Para admin moderno, Agenda/Inventário/Financeiro são boas referências.
- Criar classes com prefixo do módulo.
- Evitar estilos globais genéricos.
- Manter ações principais no topo.
- Evitar páginas muito densas sem seções.
- Garantir acessibilidade mínima: labels, foco, contraste, botões claros e textos de estado vazio.
