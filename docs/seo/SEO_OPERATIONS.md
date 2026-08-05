# Operação e manutenção de SEO

Este guia descreve como evoluir o site sem quebrar a arquitetura internacional, criar duplicidade ou publicar dados estruturados que não correspondam ao conteúdo visível.

## Fontes de verdade no código

| Responsabilidade | Local |
|---|---|
| Idiomas, segmentos e `hreflang` | `IkkonAdmin.Web/Helpers/PublicSiteLocales.cs` |
| Nome, endereço, contato e JSON-LD | `IkkonAdmin.Web/Helpers/PublicSeoHelper.cs` |
| FAQ compartilhado entre HTML e schema | `IkkonAdmin.Web/Helpers/PublicContentCatalog.cs` |
| Cultura derivada da URL | `IkkonAdmin.Web/Infrastructure/Localization/PublicPathRequestCultureProvider.cs` |
| Metadados globais | `IkkonAdmin.Web/Views/Shared/_PublicLayout.cshtml` |
| Rotas localizadas | `IkkonAdmin.Web/Program.cs` |
| Sitemap, robots e `llms.txt` | `IkkonAdmin.Web/Controllers/SeoController.cs` |
| Posts publicados no sitemap | `IkkonAdmin.Web/Services/PublicSeoService.cs` |
| Metadados institucionais | `IkkonAdmin.Web/Controllers/InstitucionalController.cs` |
| Metadados e traduções do blog | `IkkonAdmin.Web/Controllers/BlogController.cs` e `BlogPublicService.cs` |

## Como adicionar uma página pública

1. Defina uma única intenção principal e a conversão desejada.
2. Adicione a rota localizada `/{culture}/caminho` e, se necessário para compatibilidade, a rota sem prefixo em `Program.cs`.
3. Crie a action no controller e chame o padrão de SEO usado por `InstitucionalController`.
4. Defina título e description exclusivos em PT, EN e JA.
5. Crie uma view com `_PublicLayout`, `_PublicHeader`, um único H1, `_PublicBreadcrumbs` e `_PublicFooter`.
6. Use `I18n.LocalizePath("/destino")` em todo link público interno.
7. Acrescente a página em `StaticPublicPaths` no `SeoController`.
8. Inclua links internos apenas onde ajudam o visitante.
9. Adicione dados estruturados somente se o tipo e todos os fatos estiverem visíveis.
10. Atualize o mapa de páginas e palavras-chave.
11. Execute build, testes, rastreamento e revisão mobile.

Exemplo:

```csharp
SetPageSeo(
    i18n[
        "Título em português | IKKON",
        "English title | IKKON",
        "日本語のタイトル | IKKON"],
    i18n[
        "Descrição útil em português.",
        "Useful English description.",
        "日本語の説明。"],
    "secao",
    "/caminho",
    i18n["Rótulo", "Label", "ラベル"]);
```

Evite páginas criadas apenas para uma palavra-chave, páginas de cidades onde não há atuação comprovada e conteúdo curto que repete a home.

## Como adicionar ou revisar traduções

- Toda página pública relevante deve existir em PT-BR, EN e JA.
- Adapte intenção, termos, exemplos e CTA; não traduza palavra por palavra.
- Preserve nomes próprios e termos de taiko com a grafia aprovada.
- Use `I18n[pt, en, ja]` para blocos curtos.
- Para conteúdo longo ou crescente, migre os três textos para uma fonte de conteúdo estruturada antes de multiplicar condicionais na view.
- Links internos devem permanecer no idioma atual com `I18n.LocalizePath`.
- O seletor deve apontar para a URL equivalente no idioma escolhido.
- Não publique uma alternate de artigo se a tradução não estiver publicada.
- Registre data de revisão e revisor nos processos editoriais.

Checklist japonês:

- texto natural e respeitoso;
- termos 太鼓, 和太鼓, 組太鼓 e 体験レッスン usados no contexto correto;
- forma oficial do nome da escola;
- formalidade adequada em contato e parceria;
- slugs legíveis em caracteres latinos e estáveis;
- revisão por falante nativo antes de produção.

## Títulos, descriptions e headings

- Título: único, descritivo e alinhado à intenção; evite listas de palavras-chave.
- Description: resumo útil e convite coerente; não prometer preço, agenda ou serviço inexistente.
- H1: exatamente um por página.
- H2: grandes respostas ou seções.
- H3: componentes dentro da seção.
- Não use heading apenas para aumentar visualmente um texto.
- Controller é a fonte de verdade dos metadados; a view é a fonte do conteúdo visível.

Modelo recomendado:

```text
Título: benefício/tema principal + local quando útil | IKKON SPTD
Description: o que é oferecido + para quem + local/contexto + próxima ação
H1: resposta humana e direta à intenção
```

## Como escolher palavras-chave

1. Comece pelo serviço ou pergunta real.
2. Classifique a intenção: navegação, informação, local, comercial ou transacional.
3. Consulte Search Console, Planejador de Palavras‑chave e perguntas recebidas.
4. Escolha uma página já adequada antes de criar outra.
5. Defina uma principal e poucas secundárias semanticamente relacionadas.
6. Escreva para responder à pessoa; depois revise título, introdução, headings e links.
7. Meça consulta, país, idioma, página, conversão e qualidade do contato.

Não estimar volume como número sem fonte e data. Não repetir termos artificialmente.

## Blog e conteúdo editorial

Cada artigo deve conter:

- idioma correto;
- grupo de tradução consistente;
- slug estável;
- título e resumo;
- conteúdo original;
- autor público verificável;
- capa com direitos;
- data de publicação e atualização;
- categoria e tags úteis;
- SEO title e description quando o padrão automático não bastar;
- links para fontes e páginas internas;
- CTA contextual.

Ao atualizar fatos materiais, atualize também `UpdatedAtUtc`. O serviço público usa esse campo em `BlogPosting` e no sitemap.

Filtros e pesquisa são `noindex,follow`. Não crie páginas indexáveis para combinações de tags sem conteúdo editorial próprio.

## Como cadastrar eventos

O sistema atual não publica uma agenda estruturada completa. Antes de implementar `Event`, o cadastro público deve possuir:

- nome;
- descrição;
- data e hora de início;
- data e hora de término, se conhecida;
- fuso horário;
- local físico ou URL online;
- endereço completo;
- organizador;
- status;
- imagem autorizada;
- URL oficial;
- ingresso/preço somente quando real;
- idioma e traduções.

Somente então:

1. crie uma URL permanente do evento;
2. mostre os mesmos dados na página;
3. gere `Event` JSON-LD;
4. inclua evento futuro e público no sitemap;
5. marque cancelamento, adiamento ou nova data sem apagar a URL;
6. remova eventos antigos de destaques, mas preserve relatos úteis.

Evite usar `Event` em uma página genérica de contratação.

## Imagens

Antes do upload:

- confirme autoria e permissão;
- use nome descritivo, estável e sem acentos, por exemplo `ikkon-apresentacao-gueinosai-2025.webp`;
- remova metadados sensíveis;
- gere dimensões adequadas ao uso;
- prefira WebP ou AVIF com fallback quando necessário;
- mantenha o original fora de `wwwroot` se houver política de arquivo-fonte.

Na página:

- `alt` descreve o que a imagem comunica naquele idioma;
- imagem decorativa usa `alt=""`;
- informe largura/altura quando possível;
- use `loading="lazy"` abaixo da dobra;
- não aplique lazy à imagem LCP;
- use `srcset`/`sizes` quando houver variantes;
- legenda deve acrescentar evento, local, data ou contexto.

Não renomeie arquivos em uso sem atualizar todas as referências e redirects quando a URL já tiver sido publicada.

## Vídeos

- título humano e específico;
- descrição e contexto visíveis;
- iframe com `loading="lazy"`;
- transcrição ou resumo editorial;
- thumbnail autorizada e otimizada;
- link para a fonte oficial;
- data de upload real;
- duração real.

Adicionar `VideoObject` apenas com thumbnail, data e conteúdo correspondentes. Para vídeos estratégicos, criar uma página ou artigo com transcrição, contexto da apresentação e links relacionados.

## Dados estruturados

Regras:

- o conteúdo precisa ser verdadeiro, público, visível e atual;
- reutilize `PublicSeoHelper`;
- não serializar HTML não confiável sem sanitização;
- não inventar campos para eliminar warnings;
- não marcar avaliações externas como avaliações coletadas pelo site;
- não criar `Person` sem nome, papel, biografia e consentimento;
- não criar `Event` sem data/local/status;
- validar JSON e Rich Results depois de qualquer mudança.

Ao alterar nome, endereço ou telefone:

1. confirme a mudança com a escola;
2. atualize conteúdo visível e `PublicSeoHelper` no mesmo commit;
3. atualize `llms.txt`;
4. conferir mapa, footer, contato e perfis externos;
5. validar Organization/LocalBusiness.

## Sitemap e rastreamento

- Páginas estáticas são listadas em `SeoController.StaticPublicPaths`.
- Artigos publicados são carregados por `PublicSeoService`.
- Alternates de artigos incluem somente versões publicadas.
- `lastmod` usa data real do blog; páginas estáticas não recebem data fictícia.
- Nunca incluir admin, login, portal do aluno, filtros ou resultados de pesquisa.
- Depois de alterar rotas, valide `/sitemap.xml` como XML e confira amostras PT/EN/JA.
- Envie o sitemap no Search Console após deploy.

`robots.txt` não substitui `noindex`. Layouts privados usam meta robots e os principais caminhos privados também são desautorizados para rastreamento.

## Como verificar indexação

1. Search Console → Inspeção de URL.
2. Testar URL publicada, canonical declarada e canonical escolhida.
3. Confirmar rastreamento permitido.
4. Conferir idioma e `hreflang`.
5. Solicitar indexação somente para páginas finais.
6. Search Console → Sitemaps → enviar `/sitemap.xml`.
7. Search Console → Indexação → revisar excluídas, duplicadas e 404.
8. Pesquisar amostras com `site:dominio /ja/` e `site:dominio /en/`, sem tratar isso como contagem exata.

## Como evitar conteúdo duplicado

- Uma URL canônica por idioma.
- `hreflang` recíproco entre equivalentes.
- Tradução localizada, não apenas substituição mecânica.
- Sem parâmetros para idioma.
- Filtros do blog com `noindex`.
- Sem páginas locais em massa.
- Slugs de tradução ligados pelo grupo de tradução.
- URLs antigas mantidas somente com canonical; avaliar redirects após confirmar tráfego e integrações.
- Não repetir o mesmo artigo com mudanças mínimas.

## Consistência entre idiomas

Para cada atualização material, usar uma matriz:

| Campo | PT | EN | JA | Revisado |
|---|---|---|---|---|
| Título/meta |  |  |  |  |
| H1/resumo |  |  |  |  |
| Corpo |  |  |  |  |
| CTA/link |  |  |  |  |
| Fatos/datas |  |  |  |  |
| Alt/legenda |  |  |  |  |
| Schema |  |  |  |  |

Uma atualização pode ser publicada sem todas as traduções apenas quando isso for uma decisão editorial consciente. Nesse caso, não declare `hreflang` para uma tradução inexistente e não apresente conteúdo em idioma diferente sob um prefixo incorreto.

## Analytics e conversões

Configurar GA4 e Search Console somente com contas e consentimento aprovados.

Eventos recomendados:

- `contact_whatsapp`;
- `contact_email`;
- `map_open`;
- `trial_class_click`;
- `performance_request_click`;
- `language_change`;
- `blog_read`;
- `video_start`.

Dimensões úteis:

- idioma;
- página;
- CTA;
- tipo de contato;
- origem/campanha;
- país, respeitando privacidade e consentimento.

Relatório mensal:

- impressões, cliques, CTR e posição por idioma;
- landing pages orgânicas;
- consultas de marca e não marca;
- conversões de aula e apresentação;
- erros e páginas excluídas;
- Core Web Vitals;
- artigos que geram links, contato ou leitura qualificada.

Não considerar tráfego isolado como sucesso. O objetivo é contato qualificado, inscrição, contratação, parceria ou descoberta institucional.

## Checklist de publicação

- [ ] Build e testes passam.
- [ ] Um H1 por página.
- [ ] Título e description únicos nos três idiomas.
- [ ] Canonical absoluta correta.
- [ ] `lang`, `hreflang` e `x-default` corretos.
- [ ] Links do seletor preservam a página equivalente.
- [ ] Breadcrumb visível e JSON-LD correspondente.
- [ ] Schema representa o HTML real.
- [ ] Imagens têm direitos, alt e dimensão adequados.
- [ ] Links e CTAs funcionam por teclado e toque.
- [ ] Mobile sem overflow horizontal.
- [ ] 404 devolve status 404 e `noindex`.
- [ ] Sitemap contém a nova página.
- [ ] Robots não bloqueia a página.
- [ ] Japonês revisado por falante nativo.
- [ ] Lighthouse e inspeção manual executados.
