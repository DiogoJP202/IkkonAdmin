# Blog e idiomas

Este documento descreve o módulo de blog, o workflow editorial e o suporte a idiomas.

## Rotas

### Públicas

| Rota | Finalidade |
|---|---|
| `/blog` | Listagem pública |
| `/blog/{slug}` | Detalhe do post |

### Administrativas

| Rota | Finalidade |
|---|---|
| `/admin/blog` | Listagem administrativa |
| `/admin/blog/criar` | Criar post |
| `/admin/blog/editar/{id}` | Editar post |
| `/admin/blog/preview/{id}` | Preview |
| `/admin/blog/{id}/versoes` | Modal de versões por idioma |
| `/admin/blog/categorias` | CRUD de categorias |

## Entidades

### BlogPost

Campos principais:

- título;
- slug;
- resumo;
- conteúdo HTML;
- conteúdo JSON do editor;
- texto limpo para busca;
- imagem de capa;
- autor;
- categoria;
- status;
- idioma;
- grupo de tradução;
- destaque;
- blog da semana;
- publicação/agendamento;
- SEO;
- tempo de leitura.

Campos de idioma:

```text
LanguageCode
TranslationGroupId
```

### BlogCategory

Categorias editoriais do blog.

### BlogTag e BlogPostTag

Tags associadas aos posts.

## Arquitetura interna

O blog foi dividido em services menores para separar consulta, workflow editorial, idioma e mídia:

- `BlogService`: fachada principal usada pelo controller administrativo.
- `BlogAdminQueryService`: listagem, formulários, detalhes e modal de versões no admin.
- `BlogPublicService`: listagem e detalhes públicos, com seleção da melhor versão por idioma.
- `BlogLanguageService`: definição de idiomas suportados e cultura atual.
- `BlogWorkflowService`: regras de rascunho, agendamento, publicação, arquivamento e exclusão.
- `BlogWorkflowValidation`: validações obrigatórias para publicar/agendar.
- `BlogVersionService`: criação e exclusão de versões por idioma.
- `BlogSlugService`: normalização e unicidade de slug.
- `BlogTagService`: criação, normalização e vínculo de tags.
- `BlogLookupService`: categorias, autores e dados auxiliares.
- `BlogTextService`: resumo, texto limpo e tempo de leitura.
- `BlogDateTimeService`: datas de publicação/agendamento.
- `BlogMediaService`: upload de capa e imagens do conteúdo.

O retorno operacional do blog ainda usa `BlogOperationResult`. A migração para `OperationResult` deve preservar o comportamento editorial existente e pode ser feita em uma etapa própria.

## Idiomas de post

Idiomas suportados para versões do blog:

| Código | Rótulo | Nativo | Sufixo de slug |
|---|---|---|---|
| `pt-BR` | Português | Português | `pt` |
| `en-US` | Inglês | English | `en` |
| `ja-JP` | Japonês | 日本語 | `ja` |

O conteúdo não é traduzido automaticamente. Cada idioma é uma versão independente do mesmo post, criada como rascunho a partir do conteúdo atual.

## Fluxo editorial

1. Usuário cria um post em `/admin/blog/criar`.
2. O post pode ficar como rascunho.
3. Para publicar, os campos essenciais precisam estar completos:
   - resumo;
   - conteúdo;
   - categoria;
   - autor;
   - imagem de capa.
4. O post pode ser:
   - publicado;
   - agendado;
   - arquivado;
   - excluído logicamente.
5. O editor pode abrir o modal de versões e criar as versões faltantes em inglês ou japonês.
6. Cada versão deve ser revisada antes de publicar.

Status:

- `Draft`
- `Scheduled`
- `Published`
- `Archived`

## Editor

O editor do blog usa Quill e salva:

- HTML sanitizado;
- Delta JSON;
- texto limpo para busca.

Recursos:

- negrito, itálico, listas e citações;
- links;
- imagens no conteúdo;
- vídeos do YouTube;
- tags por chips;
- imagem de capa.

## Categorias

Categorias podem ser mantidas pelo modal dentro da tela de criação/edição do post, sem sair da página e sem perder o formulário preenchido.

Operações:

- criar;
- editar;
- ativar/desativar;
- excluir quando permitido.

Permissão:

```text
BLOG_CATEGORY_MANAGE
```

## Tags

Tags são adicionadas por chips no formulário do post.

Limite atual:

```text
12 tags por post
```

As tags podem ser digitadas, coladas ou removidas individualmente.

## Imagens

Formatos aceitos:

- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

Limites:

- capa: 3 MB;
- imagem de conteúdo: 2 MB.

Pastas:

```text
wwwroot/uploads/blog/capas
wwwroot/uploads/blog/conteudo
```

Esses arquivos são públicos, pois precisam ser exibidos no site.

Leia também: [Uploads e storage](./UPLOADS_E_STORAGE.md).

## Versões por idioma

O modal de versões mostra:

- Português;
- Inglês;
- Japonês.

Para cada idioma, ele indica se a versão existe, permite criar rascunho, editar ou excluir.

Ao criar uma versão:

- copia título, resumo, conteúdo, autor, categoria, capa, tags e SEO do post atual;
- define status como rascunho;
- define `LanguageCode` do idioma escolhido;
- mantém o mesmo grupo de tradução;
- gera slug com sufixo do idioma, por exemplo `meu-post-ja`.

## Seleção pública da melhor versão

Na área pública, o blog escolhe a melhor versão por grupo:

1. tenta o idioma atual da interface;
2. se não existir, usa `pt-BR`;
3. se ainda não existir, usa a versão raiz/disponível mais adequada.

Isso permite que um visitante em japonês veja posts em japonês quando publicados e continue vendo conteúdo em português quando a tradução ainda não existir.

## Interface pública traduzida

A interface do blog público tem textos fixos em:

- português;
- inglês;
- japonês.

Exemplos:

- filtros;
- botões;
- labels;
- textos vazios;
- informações do post;
- blocos de relacionados;
- CTA e rodapé.

O conteúdo editorial do post depende da versão cadastrada no admin.

## Internacionalização geral

Culturas suportadas no pipeline:

```text
pt-BR
en-US
ja-JP
```

Troca de idioma:

```text
/idioma/alterar?culture=ja-JP&returnUrl=/blog
```

O idioma fica em cookie usando `CookieRequestCultureProvider`.

## Landing em japonês

As páginas institucionais usam a flag:

```text
ViewData["JapaneseLandingEnabled"] = true
```

Essa flag habilita:

- textos japoneses nas views institucionais;
- seletor `日本語`;
- tag decorativa de site em construção/testes.

## Blog em japonês

O blog usa:

```text
ViewData["JapanesePublicEnabled"] = true
```

Isso habilita interface japonesa e seletor `日本語` no blog, sem acionar a tag decorativa da landing.

## Permissões

- `BLOG_VIEW`
- `BLOG_CREATE`
- `BLOG_EDIT`
- `BLOG_PUBLISH`
- `BLOG_ARCHIVE`
- `BLOG_DELETE`
- `BLOG_FEATURE`
- `BLOG_CATEGORY_MANAGE`
- `BLOG_TAG_MANAGE`

## Cuidados de manutenção

- Não publicar versões traduzidas sem revisão humana.
- Manter slugs únicos entre todos os idiomas.
- Lembrar que categorias e tags ainda são globais, não traduzidas por idioma.
- Ao mexer em textos fixos do blog público, usar overload trilíngue do `IViewTextService`.
- Ao adicionar texto fixo novo no blog público, validar português, inglês e japonês.
- A tag decorativa japonesa pertence apenas à landing; o blog em japonês não deve carregá-la.
- Imagens de blog são públicas; não usar esse módulo para arquivos sensíveis.
