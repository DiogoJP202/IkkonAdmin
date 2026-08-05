# Auditoria e estratégia de SEO, SEO internacional e GEO

Data da auditoria: 30 de julho de 2026.

## Resumo executivo

O frontend público já tinha conteúdo trilíngue, títulos hierárquicos, imagens em WebP, carregamento tardio e páginas específicas para escola, apresentações e blog. O principal impedimento de SEO internacional era estrutural: português, inglês e japonês eram servidos na mesma URL, com o idioma escolhido por cookie. Isso dificultava rastreamento, indexação, compartilhamento e atribuição de uma URL a cada idioma.

A arquitetura adotada usa subdiretórios no mesmo domínio:

```text
/pt/
/en/
/ja/
```

Subdiretórios mantêm a autoridade em um único domínio, são simples de operar no ASP.NET Core e permitem que cada idioma tenha canonical, `lang`, `hreflang`, links internos e sitemap próprios. As URLs antigas sem prefixo continuam funcionando para preservar compatibilidade, mas apontam por canonical para a versão localizada. O site não força idioma por geolocalização; a escolha explícita continua salva em cookie.

## Diagnóstico inicial e prioridade

| Prioridade | Estado anterior | Impacto | Correção |
|---|---|---|---|
| P0 | Mesmo endereço exibia idiomas diferentes por cookie | Versões difíceis de rastrear e propensas a indexação no idioma errado | URLs `/pt`, `/en` e `/ja`, provedor de cultura por caminho e links localizados |
| P0 | Ausência de `hreflang`, `x-default` e sitemap internacional | Baixa compreensão da relação entre traduções | Alternates no HTML e no sitemap |
| P0 | Canonical ausente ou não localizado em parte das páginas | Duplicidade entre idioma, filtros e URLs legadas | Canonical absoluto por página e idioma |
| P0 | Sem `robots.txt` e sem sitemap XML | Descoberta e controle de rastreamento incompletos | Endpoints dinâmicos `/robots.txt` e `/sitemap.xml` |
| P1 | Descrições institucionais definidas somente em português no controller | Snippets incorretos em inglês e japonês | Títulos e descrições únicos nos três idiomas |
| P1 | Sem dados estruturados públicos | Entidades e relações institucionais pouco explícitas | JSON-LD factual para organização, negócio local, páginas, cursos, grupo, FAQ, breadcrumbs e artigos |
| P1 | Área privada sem regra explícita de indexação | Possível exposição de telas de login e portais nos resultados | `noindex,nofollow,noarchive` nos layouts internos |
| P1 | Blog sem idioma, atualização e traduções no modelo público | Article schema e alternates incompletos | Idioma real, data de atualização e slugs alternativos no view model |
| P1 | Ausência de páginas centrais “Sobre”, “O que é taiko” e “Contato” | Identidade, definição e dados oficiais dispersos | Novas páginas úteis em PT, EN e JA |
| P2 | Open Graph parcial e sem imagem social dedicada | Compartilhamentos inconsistentes | OG/Twitter completos e card social 1200 × 630 |
| P2 | Sem `llms.txt` | Menor clareza para ferramentas que consultam esse recurso | Arquivo factual com páginas e canais oficiais |
| P2 | Sem página 404 pública orientativa | Experiência ruim e poucos caminhos de recuperação | 404 real, útil e com `noindex` |
| P2 | Fontes japonesas externas carregadas em todos os idiomas | Requisições desnecessárias em PT e EN | Google Fonts somente em japonês; fontes latinas continuam locais |
| P2 | Cache e compactação sem política explícita | Transferência maior e recursos repetidos | Brotli/Gzip e cache longo para recursos versionados |
| P3 | Imagem de textura PNG com aproximadamente 1,69 MB | Custo de transferência em seções que a utilizam | Converter e comparar visualmente antes de substituir |
| P3 | Galeria com nomes e textos alternativos genéricos | Contexto visual limitado | Revisar foto a foto com local, evento, data e direitos |

## Arquitetura internacional

### Regras

- Português brasileiro: `/pt`, cultura `pt-BR`, `hreflang="pt-BR"`.
- Inglês internacional: `/en`, cultura `en-US`, `hreflang="en"`.
- Japonês: `/ja`, cultura `ja-JP`, `hreflang="ja"`.
- `x-default`: versão em português, salvo quando um artigo não tem versão em português.
- Cada tradução usa canonical para si mesma.
- Alternates de artigos incluem somente traduções realmente publicadas.
- Se um slug de artigo não existir no idioma solicitado, a URL localizada incorreta é redirecionada permanentemente para a versão publicada real.
- Filtros e pesquisas do blog usam `noindex,follow`.
- Paginação sem filtro tem canonical próprio.
- Não há redirecionamento automático por IP ou país.
- O seletor de idioma altera a URL e persiste a escolha.

Essa abordagem segue a recomendação do Google de usar URLs diferentes para cada idioma e declarar as relações com `hreflang`. Referências: [sites multilíngues e multirregionais](https://developers.google.com/search/docs/specialty/international/managing-multi-regional-sites) e [versões localizadas](https://developers.google.com/search/docs/specialty/international/localized-versions).

## Mapa de páginas, público e intenção

| Página | Intenção principal | Público | Conversão |
|---|---|---|---|
| `/{idioma}` | Encontrar uma escola e grupo de taiko em São Paulo | Local, internacional e japonês | Escola ou apresentações |
| `/{idioma}/sobre` | Entender quem é o IKKON e sua atuação | Alunos, instituições, imprensa, parceiros | Conhecer escola ou grupo |
| `/{idioma}/taiko` | Aprender o que é taiko/wadaiko e sua prática | Iniciantes, estudantes, pesquisadores culturais | Aula ou apresentação |
| `/{idioma}/escola` | Encontrar aulas de taiko em São Paulo | Potenciais alunos e familiares | Aula experimental |
| `/{idioma}/eventos` | Contratar apresentação de taiko | Produtores, empresas, escolas e festivais | Pedido de proposta |
| `/{idioma}/blog` | Acompanhar conhecimento e atividade concreta | Comunidade, imprensa, alunos e público internacional | Leitura e contato |
| `/{idioma}/contato` | Confirmar canais e localização oficiais | Todos os públicos | WhatsApp, e-mail ou mapa |

## Pesquisa e mapeamento de palavras-chave

“Relevância” é uma estimativa qualitativa baseada em aderência ao serviço, intenção e linguagem encontrada em fontes oficiais; não representa volume de uma ferramenta paga. Depois da publicação, validar volume, variações e posição no Planejador de Palavras‑chave e no Search Console.

| Palavra-chave | Idioma | Intenção | Relevância | Página recomendada | Prioridade | Conteúdo necessário |
|---|---|---|---|---|---|---|
| escola de taiko | PT-BR | Comercial/local | Alta | `/pt/escola` | P0 | Página de aulas |
| aula de taiko | PT-BR | Comercial | Alta | `/pt/escola` | P0 | Método, níveis, FAQ e CTA |
| taiko em São Paulo | PT-BR | Local | Alta | `/pt` e `/pt/escola` | P0 | Identidade, localização e aulas |
| curso de taiko São Paulo | PT-BR | Comercial/local | Alta | `/pt/escola` | P0 | Cursos, progressão e contato |
| apresentação de taiko | PT-BR | Comercial | Alta | `/pt/eventos` | P0 | Formatos e contratação |
| grupo de taiko São Paulo | PT-BR | Local/institucional | Alta | `/pt/eventos` | P1 | Ensemble, galeria e atuação |
| taiko no Brasil | PT-BR | Informacional | Média/alta | `/pt/taiko` | P1 | Conteúdo educativo e fontes |
| tambor japonês | PT-BR | Informacional | Média/alta | `/pt/taiko` | P1 | Definições de taiko e wadaiko |
| cultura japonesa em São Paulo | PT-BR | Informacional/local | Média | `/pt/sobre` e blog | P1 | Atuação cultural concreta |
| workshop de taiko | PT-BR | Comercial | Média | Conteúdo futuro | P2 | Criar somente quando houver oferta real |
| taiko para evento corporativo | PT-BR | Comercial | Média | `/pt/eventos` | P1 | Formatos e briefing |
| aula experimental de taiko | PT-BR | Transacional | Média | `/pt/escola` | P0 | CTA e processo de participação |
| サンパウロ 和太鼓 | JA | Local/informacional | Alta | `/ja` | P0 | Quem, onde e o que oferece |
| サンパウロ 太鼓教室 | JA | Comercial/local | Alta | `/ja/escola` | P0 | 教室、初心者、体験 |
| ブラジル 和太鼓 | JA | Informacional | Alta | `/ja/taiko` | P0 | ブラジルでの活動と文化交流 |
| ブラジル 太鼓教室 | JA | Comercial | Média/alta | `/ja/escola` | P1 | レッスン、場所、連絡方法 |
| ブラジル 和太鼓 チーム | JA | Institucional | Média/alta | `/ja/eventos` | P1 | 演奏チームと公演 |
| 和太鼓 公演 ブラジル | JA | Comercial/institucional | Média | `/ja/eventos` | P1 | 公演形式と依頼方法 |
| 和太鼓 体験 サンパウロ | JA | Transacional | Média | `/ja/escola` | P1 | 体験レッスン |
| 海外 和太鼓 グループ | JA | Pesquisa/internacional | Média | `/ja/sobre` | P2 | História e intercâmbios comprovados |
| 日系社会 和太鼓 | JA | Cultural/comunitária | Média | `/ja/taiko` e blog | P1 | Atuação documentada |
| ブラジル 日本文化 | JA | Informacional | Média | `/ja/sobre` e blog | P2 | Projetos e eventos reais |
| 日伯文化交流 | JA | Institucional | Nicho estratégico | `/ja/sobre` | P1 | Parcerias e intercâmbios comprovados |
| 一魂サンパウロ太鼓道場 | JA | Marca/navegacional | Alta | `/ja` | P0 | Nome oficial consistente |
| taiko school in Brazil | EN | Comercial/internacional | Alta | `/en/escola` | P0 | School, method, location |
| taiko classes in Sao Paulo | EN | Comercial/local | Alta | `/en/escola` | P0 | Classes, beginners, contact |
| Brazilian taiko group | EN | Institucional | Alta | `/en/eventos` | P1 | Ensemble and performances |
| Japanese drumming in Brazil | EN | Informacional | Média/alta | `/en/taiko` | P1 | Definition and Brazilian context |
| taiko performance Sao Paulo | EN | Comercial | Média/alta | `/en/eventos` | P1 | Formats and booking |
| Japanese culture in Brazil | EN | Informacional | Média | `/en/sobre` e blog | P2 | Concrete cultural work |
| international taiko community Brazil | EN | Comunidade/pesquisa | Nicho | `/en/sobre` e blog | P2 | Collaborations and documented participation |
| Brazil Japan cultural exchange | EN | Institucional | Nicho estratégico | `/en/sobre` | P1 | Projects, partners, sources |
| taiko workshop Brazil | EN | Comercial | Média | Conteúdo futuro | P2 | Publish only for an actual workshop |
| wadaiko Brazil | EN/JA romanizado | Informacional | Média | `/en/taiko` | P1 | Wadaiko terminology and context |

O uso japonês de `和太鼓`, `ブラジル太鼓協会` e `日系社会` foi confirmado em conteúdo oficial da [JICA sobre o taiko no Brasil](https://www.jica.go.jp/volunteer/outline/publication/pamphlet/crossroad/202203/pickup_03_16/index.html). Esse material sustenta a escolha de vocabulário, mas não deve ser usado para atribuir ao IKKON fatos que a fonte não declara.

## GEO e descoberta por IA

### Entidades e fatos oficiais

O código centraliza e expõe somente fatos já públicos:

- Nome: IKKON São Paulo Taiko Dojo.
- Formas alternativas: IKKON SPTD e 一魂サンパウロ太鼓道場.
- Natureza: escola de taiko e grupo artístico.
- Fundação: 2015.
- Endereço: Rua Domingos de Morais, 2975, São Paulo, SP, Brasil.
- Telefone/WhatsApp: +55 11 93779-9916.
- E-mail: contato@ikkontaiko.com.
- Instagram: `@ikkontaiko`.

Esses dados aparecem em conteúdo visível, JSON-LD e `llms.txt`. O arquivo para IA é complementar: canonical, conteúdo HTML, links, sitemap e dados estruturados continuam sendo as fontes primárias.

### Conteúdo favorável a respostas e citações

- Resumos diretos no início das páginas.
- Definição de taiko, wadaiko e prática em conjunto.
- FAQ visível e gerada a partir da mesma fonte usada pelo `FAQPage`.
- Página institucional com atividade, cidade, data de início e canais.
- Datas, idioma, autoria e atualização dos artigos.
- Breadcrumbs visíveis e semânticos.
- Relações internas claras entre aprender, assistir, contratar e contatar.

### Conteúdos de autoridade recomendados

1. Linha do tempo do IKKON com datas, fontes e fotos autorizadas.
2. Biografias de professores e responsáveis, com formação e papel atual.
3. Relatos de apresentações com data, local, organização anfitriã e repertório.
4. Método de ensino, instrumentos usados e critérios de progressão.
5. Depoimentos autorizados de alunos, sem marcação de avaliação agregada artificial.
6. Projetos com comunidades brasileiras, japonesas e nikkeis.
7. Glossário PT–JA–EN de taiko, wadaiko, bachi, fue e termos de prática.
8. Transcrições e resumos dos vídeos principais.
9. Página de imprensa com apresentação institucional, fotos autorizadas e contato.
10. Agenda pública somente quando datas, local, status e ingresso forem confiáveis.

## Dados estruturados implementados

| Schema | Uso | Limite adotado |
|---|---|---|
| `EducationalOrganization` + `LocalBusiness` | Identidade, endereço e contato na home/sobre | Sem horário, preço, nota ou avaliação não confirmados |
| `WebSite` | Relação do site oficial com a organização | Na home/sobre |
| `WebPage`, `AboutPage`, `ContactPage`, `CollectionPage` | Tipo e idioma de cada página | Título e descrição visíveis |
| `BreadcrumbList` | Hierarquia das páginas públicas | Espelha breadcrumb visível |
| `FAQPage` | FAQ de alunos na página da escola | Mesmas perguntas e respostas do HTML |
| `Course` | Taiko, fue e teoria musical | Sem preço, duração ou certificado não confirmados |
| `MusicGroup` | IKKON Taiko Arts Ensemble | Sem integrantes individuais não documentados |
| `BlogPosting` | Artigos publicados | Título, resumo, imagem, autor e datas reais |

O Google recomenda informar dados úteis e verificáveis de presença real em `Organization`/`LocalBusiness` e validar o resultado antes da publicação. Referências: [Organization](https://developers.google.com/search/docs/appearance/structured-data/organization) e [LocalBusiness](https://developers.google.com/search/docs/appearance/structured-data/local-business).

Não foram adicionados `Person`, `Event`, `VideoObject`, `ImageObject`, avaliações ou preços porque faltam dados completos e verificáveis.

## Estratégia de links internos

- Header: home, escola, eventos, blog, FAQ, contato e áreas privadas.
- Footer: acrescenta sobre, o que é taiko e contato aos destinos já existentes.
- Home: direciona para escola e apresentações.
- “O que é taiko”: liga conteúdo informacional às duas conversões principais.
- Blog: categorias, tags, relacionados, escola e contato preservam caminhos úteis.
- Breadcrumbs: home → seção → artigo.
- Links externos: somente canais oficiais e mapas; novas referências devem ter propósito editorial.

Evitar links ocultos, rodapés com listas extensas de cidades, âncoras repetitivas e páginas locais sem conteúdo real.

## SEO local

Implementado:

- NAP público consistente.
- Endereço e mapa visíveis.
- Telefone com código do país.
- Página de contato rastreável nos três idiomas.
- `LocalBusiness` com cidade, estado e país.
- Conteúdo específico para “taiko em São Paulo”.

Próximas ações externas:

1. Confirmar e vincular o Perfil da Empresa no Google.
2. Usar exatamente o mesmo nome, endereço e telefone em Google, Instagram, Linktree, associações e diretórios.
3. Informar CEP, bairro, horários reais e orientações de transporte.
4. Resolver a divergência entre “2 unidades” exibida no site e o único endereço público atual.
5. Solicitar avaliações de forma ética no perfil empresarial, sem publicar marcação de avaliações que o site não coleta.

## Autoridade e divulgação externa

Priorizar relações editoriais e institucionais reais:

- Bunkyo e eventos da comunidade nipo-brasileira.
- Associação Brasileira de Taiko e grupos parceiros.
- Fundação Japão, Consulado-Geral do Japão e entidades culturais.
- Escolas de língua japonesa, universidades e projetos de estudos asiáticos.
- Festivais, centros culturais, escolas e organizações anfitriãs.
- Imprensa local, cultural e nipo-brasileira.
- Colaborações internacionais e intercâmbios documentados.

Cada menção deve apontar para a página mais útil: escola, evento, relato, biografia ou projeto. Não comprar links, trocar links em massa ou criar diretórios artificiais. Programas públicos do Bunkyo já registram o nome IKKON e sua forma japonesa; manter a identidade consistente favorece consolidação de entidade: [programa de 2023](https://www.bunkyo.org.br/wp-content/uploads/2023/06/Programacao56Gueinosai.pdf) e [programa de 2025](https://bunkyo.org.br/wp-content/uploads/2025/06/Programacao-58o-Gueinosai.pdf).

## Desempenho e experiência

Melhorias aplicadas:

- Brotli e Gzip para respostas compatíveis.
- Cache de um ano e `immutable` para assets com hash de versão.
- Cache de sete dias para outros arquivos estáticos.
- Fontes latinas locais; fontes japonesas externas apenas em `/ja`.
- Imagem inicial do carrossel com prioridade alta.
- Demais slides, vídeos, mapa e imagens abaixo da dobra com carregamento tardio.
- Card social de 1200 × 630 com aproximadamente 95 KB.
- Layouts responsivos e foco visível preservados.

Metas de campo a acompanhar no percentil 75:

| Métrica | Meta “boa” |
|---|---|
| LCP | até 2,5 s |
| INP | até 200 ms |
| CLS | até 0,1 |

Core Web Vitals devem ser avaliados com dados reais do Chrome UX Report/Search Console; Lighthouse é diagnóstico de laboratório e não mede INP real. Referência: [Web Vitals](https://web.dev/articles/vitals).

Pendências de desempenho:

- Converter `textura-pontos.png` para WebP/AVIF e comparar lado a lado antes de substituir.
- Produzir tamanhos responsivos para imagens hero e galeria.
- Avaliar auto-hospedagem de fontes japonesas licenciadas.
- Medir o impacto dos iframes do YouTube e considerar fachada leve após validação visual/funcional.

## Conteúdo japonês que exige revisão nativa

Antes de produção, um falante nativo deve revisar:

- títulos, descrições e H1 de `/ja`, `/ja/sobre`, `/ja/taiko`, `/ja/escola`, `/ja/eventos` e `/ja/contato`;
- explicações de 太鼓, 和太鼓 e 組太鼓;
- termos de ensino como 体験レッスン, 初級, 中級 e 上級;
- tom de pedidos de apresentação e parcerias;
- FAQ completo;
- textos japoneses de artigos e slugs;
- nome japonês oficial `一魂サンパウロ太鼓道場`.

A revisão deve confirmar naturalidade, nível de formalidade, termos usados pela comunidade japonesa de taiko e consistência entre japonês, fatos oficiais e chamadas de contato.

## Pendências que dependem da escola

- Nome institucional/legal exato e forma japonesa aprovada.
- CEP, bairro, horários e instruções de acesso.
- Endereço e funcionamento da segunda unidade mencionada no site.
- Professores, responsáveis, cargos, biografias, fotos e autorizações.
- Linha do tempo e documentos da história desde 2015.
- Quantidades atuais de alunos, apresentações e unidades.
- Agenda real de eventos, workshops e aulas abertas.
- Preços, duração, capacidade e pré-requisitos, caso devam ser públicos.
- Perfil oficial no Google e URLs de outros perfis verificados.
- Participações, parceiros, imprensa e intercâmbios que podem ser comprovados.
- Direitos, local, data e legenda de cada foto.
- Data, thumbnail, transcrição e direitos dos vídeos.
- Revisão nativa japonesa.
- IDs e governança de Google Search Console, GA4 e gerenciador de consentimento.

## Plano de acompanhamento

### Antes da publicação

1. Confirmar domínio canônico e HTTPS.
2. Revisar japonês com falante nativo.
3. Validar JSON-LD no Rich Results Test e Schema Markup Validator.
4. Rastrear o site com sitemap e conferir status, canonical, `lang` e `hreflang`.
5. Executar Lighthouse em mobile nas sete páginas principais.
6. Verificar manualmente seletor de idioma, formulários, WhatsApp, mapa e blog.

### Primeira semana

1. Adicionar as propriedades do domínio ao Search Console.
2. Enviar `/sitemap.xml`.
3. Inspecionar uma URL de cada idioma e um artigo com traduções.
4. Configurar GA4 com eventos de WhatsApp, e-mail, mapa, aula e apresentação.
5. Registrar uma linha de base de páginas indexadas, impressões e conversões.

### Mensal

- consultas e páginas por idioma;
- impressões, cliques, CTR e posição;
- contatos de aula e apresentação;
- erros de rastreamento e páginas excluídas;
- Core Web Vitals;
- crescimento de páginas e links institucionais;
- conteúdo desatualizado, especialmente datas, pessoas e agenda.

### Trimestral

- revisar o mapa de palavras-chave com dados reais;
- atualizar páginas com perguntas encontradas nas consultas;
- auditar consistência NAP e perfis;
- revisar alternates, traduções, links quebrados e conteúdo sem tráfego;
- selecionar estudos de caso, relatos e materiais educativos com evidência real.
