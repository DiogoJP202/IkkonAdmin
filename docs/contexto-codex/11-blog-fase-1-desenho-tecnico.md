# Modulo de Blog - Fase 1 - desenho tecnico

## Objetivo

Definir o desenho tecnico da primeira versao do modulo de Blog do IkkonAdmin, respeitando a arquitetura atual do projeto, os padroes visuais ja usados no admin e no site publico, o modelo de permissoes existente e a estrategia atual de upload local.

Esta fase nao implementa schema, controllers ou views finais. Ela fecha as decisoes necessarias para a Fase 2 com baixo risco de retrabalho.

## Base real do projeto

O Blog deve seguir os padroes ja usados no codigo atual:

- MVC tradicional com controllers finos, services com `ApplicationDbContext` direto e ViewModels especificos.
- Sem camada de repository separada.
- Controllers administrativos protegidos por policies declaradas em `Program.cs`.
- Navegacao administrativa condicionada por `User.HasPermission(...)` e `User.HasAnyPermission(...)`.
- Upload local em `wwwroot/uploads/...` com validacao no backend.
- Site publico baseado em Razor Views e `_PublicLayout.cshtml`.
- Sem dependencia atual para editor rico ou sanitizacao HTML.

Arquivos de referencia usados nesta definicao:

- `IkkonAdmin.Web/Program.cs`
- `IkkonAdmin.Web/Data/ApplicationDbContext.cs`
- `IkkonAdmin.Web/Security/AppPermissions.cs`
- `IkkonAdmin.Web/Security/AuthorizationPolicies.cs`
- `IkkonAdmin.Web/Data/SeedData.cs`
- `IkkonAdmin.Web/Services/UserSettingsService.cs`
- `IkkonAdmin.Web/Controllers/InventarioController.cs`
- `IkkonAdmin.Web/Services/InventarioService.cs`
- `IkkonAdmin.Web/Views/Shared/_Sidebar.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicLayout.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicHeader.cshtml`

## Escopo da v1

O modulo deve cobrir:

- painel administrativo para posts, categorias e tags;
- rascunho, agendamento, publicacao, arquivamento e exclusao logica;
- imagem de capa;
- editor low-code simples, sem page builder complexo;
- upload de imagens para capa e conteudo;
- embeds de YouTube;
- listagem publica `/blog`;
- pagina individual `/blog/{slug}`;
- filtros, busca e destaques;
- SEO basico por post;
- sanitizacao de conteudo e protecoes contra XSS.

Itens explicitamente fora da v1:

- page builder avancado por blocos customizados;
- upload local de video;
- analytics avancado;
- ordenacao manual sofisticada de destaques;
- full-text search nativo do SQL Server;
- workflow editorial multiaprovacao.

## Arquitetura proposta

### Separacao de controllers

Manter separacao clara entre publico e administrativo:

- `BlogController`: rotas publicas `/blog` e `/blog/{slug}`.
- `BlogAdminController`: rotas administrativas `/admin/blog`.
- `BlogCategoriasController`: rotas administrativas `/admin/blog/categorias`.
- Tags entram na v1 como selecao/criacao dentro do formulario do post. Uma tela propria de tags pode ser adicionada depois se houver necessidade operacional.

Motivo:

- evita misturar experiencia publica com CRUD interno;
- segue o padrao ja usado em modulos com route attribute;
- reduz risco de conflito entre `/blog` e `/admin/blog`.

### Services

Criar services especificos do modulo:

- `IBlogService` / `BlogService`: posts, filtros, status, publicacao e consultas publicas.
- `IBlogCategoriaService` / `BlogCategoriaService`: CRUD de categorias.
- `IBlogMediaService` / `BlogMediaService`: upload de capa e imagens de conteudo.

O projeto nao usa repository separado hoje. O Blog deve manter o mesmo padrao.

### ViewModels

Criar ViewModels dedicados para:

- index administrativo;
- formulario de post;
- preview administrativo;
- index publico;
- detalhes publicos;
- categorias;
- filtros administrativos;
- filtros publicos.

## Rotas propostas

### Publicas

- `/blog`
- `/blog/{slug}`

Filtros publicos via query string:

- `/blog?q=taiko`
- `/blog?categoria=nome-da-categoria`
- `/blog?tag=nome-da-tag`

Decisao:

- usar query string para busca e filtros;
- preservar `/blog/{slug}` exclusivamente para detalhe;
- evitar combinacoes de rotas como `/blog/tag/x` na v1.

### Administrativas

- `/admin/blog`
- `/admin/blog/criar`
- `/admin/blog/editar/{id}`
- `/admin/blog/preview/{id}`
- `/admin/blog/categorias`
- `/admin/blog/categorias/criar`
- `/admin/blog/categorias/editar/{id}`

## Modelagem final sugerida

### Entidade `BlogPost`

Campos propostos:

- `Id`
- `Title`
- `Slug`
- `Summary`
- `ContentHtml`
- `ContentJson`
- `ContentText`
- `CoverImageUrl`
- `AuthorUserId`
- `AuthorDisplayName`
- `CategoryId`
- `Status`
- `IsFeatured`
- `IsWeeklyHighlight`
- `PublishedAtUtc`
- `ScheduledAtUtc`
- `ArchivedAtUtc`
- `CreatedAtUtc`
- `UpdatedAtUtc`
- `SeoTitle`
- `SeoDescription`
- `ReadingTimeMinutes`
- `DeletedAtUtc`

Observacoes:

- `ContentHtml` e o HTML sanitizado, pronto para render.
- `ContentJson` guarda o payload canonico do editor para futura evolucao.
- `ContentText` guarda texto limpo para busca e leitura.
- `AuthorDisplayName` e um snapshot para preservar autoria publica.
- `DeletedAtUtc` implementa exclusao logica sem misturar com status editorial.
- `CategoryId` pode ser nulo em rascunho, mas nao em publicacao.

Campos adiados da v1:

- `ViewCount`
- `FeaturedOrder`

### Entidade `BlogCategory`

Campos propostos:

- `Id`
- `Name`
- `Slug`
- `Description`
- `IsActive`
- `CreatedAtUtc`
- `UpdatedAtUtc`

### Entidade `BlogTag`

Campos propostos:

- `Id`
- `Name`
- `Slug`
- `IsActive`
- `CreatedAtUtc`

### Entidade `BlogPostTag`

Campos propostos:

- `BlogPostId`
- `BlogTagId`

Regra:

- chave composta unica;
- nao permitir duplicidade do mesmo tag no mesmo post.

### Enum `BlogPostStatusEnum`

Valores:

- `Draft`
- `Scheduled`
- `Published`
- `Archived`

## Regras editoriais e de publicacao

### Fluxo de status

- `Draft`: post ainda nao publico.
- `Scheduled`: post com data futura definida.
- `Published`: post publico e elegivel para aparecer no site.
- `Archived`: post retirado da listagem publica.

### Regras de agendamento

Decisao da v1:

- nao criar background job;
- usar promocao lazy no service;
- quando um post `Scheduled` atingir `ScheduledAtUtc <= agora`, o service o promove para `Published` antes de consultas administrativas e publicas.

Motivo:

- atende a regra de negocio sem infraestrutura extra;
- evita post ficar invisivel publicamente apos o horario;
- mantem o schema simples.

### Regras de validacao por estado

Para salvar como rascunho:

- `Title` obrigatorio;
- `Slug` obrigatorio e unico;
- demais campos podem ficar incompletos.

Para publicar ou agendar:

- `Title` obrigatorio;
- `Slug` obrigatorio e unico;
- `Summary` obrigatorio;
- `ContentHtml` nao vazio;
- `CategoryId` obrigatorio;
- `AuthorUserId` obrigatorio;
- `SeoDescription` obrigatorio ou derivado do resumo;
- `CoverImageUrl` recomendado e validado na regra de negocio.

## Estrategia do editor low-code

### Escolha recomendada

Usar um editor rico simples, self-hosted, integrado em Razor.

Escolha recomendada para a v1:

- `Quill 2`

Motivos:

- encaixa melhor no projeto atual que uma stack SPA;
- suporta toolbar simples e experiencia low-code;
- permite salvar estrutura em JSON e HTML;
- tem curva de integracao menor que uma solucao por blocos avancada.

### Toolbar da v1

Suportar:

- titulos;
- subtitulos;
- paragrafos;
- negrito;
- italico;
- listas;
- links;
- blocos de citacao;
- separadores;
- imagens;
- videos incorporados do YouTube.

### Estrategia de preview

Preview administrativo deve:

- usar o mesmo HTML sanitizado da experiencia publica;
- abrir em rota propria;
- permitir visualizacao de layout, capa, meta e conteudo.

Na v1, o preview parte de um post salvo. Preview de post ainda nao salvo pode entrar depois.

## Estrategia de upload e midia

### Pastas

- `wwwroot/uploads/blog/capas`
- `wwwroot/uploads/blog/conteudo`

### Regras

- aceitar apenas `.jpg`, `.jpeg`, `.png` e `.webp`;
- bloquear SVG;
- gerar nome seguro com `Guid`;
- salvar apenas URL relativa publica;
- validar extensao e tamanho no backend;
- impedir sobrescrita;
- nao expor caminho fisico.

### Limites sugeridos

- capa: ate `3 MB`;
- imagem de conteudo: ate `2 MB`.

### Videos

Na v1:

- nao fazer upload local de video;
- aceitar apenas URL do YouTube;
- converter para embed seguro no backend;
- bloquear `iframe` arbitrario vindo do editor.

## Estrategia de permissoes

Permissoes propostas:

- `BLOG_VIEW`
- `BLOG_CREATE`
- `BLOG_EDIT`
- `BLOG_PUBLISH`
- `BLOG_ARCHIVE`
- `BLOG_DELETE`
- `BLOG_FEATURE`
- `BLOG_CATEGORY_MANAGE`
- `BLOG_TAG_MANAGE`

Desdobramentos obrigatorios:

- constantes em `AppPermissions`;
- constants em `AuthorizationPolicies`;
- registro em `Program.cs`;
- seed em `SeedData`;
- uso na sidebar;
- protecao em controller e actions;
- protecao de botoes na view.

Observacao:

- `Admin` continua com acesso total pelo comportamento atual do sistema.

## Estrategia da experiencia administrativa

### Telas da v1

- listagem de posts;
- criar post;
- editar post;
- preview de post;
- categorias;

Tags na v1:

- criadas e selecionadas dentro do formulario do post;
- sem tela separada obrigatoria no primeiro corte.

### Listagem administrativa

Exibir:

- titulo;
- status;
- categoria;
- autor;
- data de publicacao;
- data de criacao;
- destaque;
- blog da semana;
- acoes.

Filtros:

- busca textual;
- status;
- categoria;
- autor;
- destaque;
- blog da semana;
- periodo de publicacao.

### UX do formulario

O formulario deve ser pensado para usuario nao tecnico:

- secao de identificacao;
- secao editorial;
- secao de capa e SEO;
- secao de conteudo;
- secao de publicacao;
- botoes claros para rascunho, agendar, publicar e arquivar.

## Estrategia da experiencia publica

### Pagina `/blog`

Blocos previstos:

- hero do blog;
- faixa de destaques;
- faixa de blogs da semana;
- busca e filtros;
- grid/lista de cards;
- estado vazio;
- paginacao simples ou "carregar mais" posterior.

### Pagina `/blog/{slug}`

Blocos previstos:

- titulo;
- resumo;
- capa;
- data;
- autor;
- categoria;
- tags;
- conteudo renderizado;
- compartilhamento;
- posts relacionados.

### Navegacao publica

Atualizar:

- `_PublicHeader.cshtml`
- `_PublicFooter.cshtml`

Adicionar o item "Blog" na navegacao principal e no rodape.

## SEO e compartilhamento

A v1 deve incluir:

- slug amigavel e unico;
- `SeoTitle`;
- `SeoDescription`;
- `meta description`;
- Open Graph basico;
- imagem de compartilhamento usando capa;
- URL publica limpa;
- `canonical`.

Impacto tecnico:

- `_PublicLayout.cshtml` precisa suportar mais metadados no `head`.
- Pode ser via `ViewData` ou uma `RenderSection("Head")`.

## Busca e leitura

### Busca publica

A busca da v1 nao deve depender de full-text search do SQL Server.

Decisao:

- gerar `ContentText` no salvamento;
- pesquisar em `Title`, `Summary`, `ContentText`, categoria e tags.

### Tempo de leitura

`ReadingTimeMinutes` sera calculado no salvamento a partir de `ContentText`.

## Seguranca

Pontos obrigatorios:

- sanitizacao HTML no servidor;
- nao confiar no HTML nem no JSON do editor;
- validar URLs de links externos;
- aceitar somente embed seguro de YouTube;
- validar uploads no backend;
- exibir publicamente apenas posts efetivamente publicados;
- proteger todos os POSTs administrativos com antiforgery;
- proteger actions por policy, nao apenas por UI.

Dependencia recomendada para Fase 4:

- `Ganss.Xss`

## Checklist de saida da Fase 1

A Fase 1 estara concluida quando:

- naming final do modulo estiver fechado;
- modelagem estiver definida sem pendencias estruturais;
- estrategia de editor estiver definida;
- estrategia de upload estiver definida;
- estrategia de agendamento estiver definida;
- estrategia de permissao estiver definida;
- estrategia publica estiver definida;
- requisitos de SEO estiverem definidos;
- regras de seguranca e sanitizacao estiverem definidas;
- criterios de aceite da Fase 2 estiverem claros.

## Criterios de aceite para iniciar a Fase 2

Podemos entrar na Fase 2 quando:

- o modulo usar o padrao de service atual do projeto;
- a migration puder ser criada sem debate de naming;
- as entidades ja tiverem campos finais aprovados;
- status editorial e exclusao logica estiverem fechados;
- o comportamento de agendamento estiver definido;
- editor, preview e sanitizacao ja tiverem contrato aprovado;
- uploads de capa e conteudo ja tiverem contrato aprovado.
