# Padrões visuais do frontend público

Este documento registra o sistema visual encontrado na aplicação. Ele descreve o estado atual; não é uma proposta de redesign.

## Fundamentos

### Tipografia

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Corpo — Lato | Leitura contínua e interface | Parágrafos, navegação, botões, filtros e metadados | 300, 400, 700 e 900 | Usar `var(--ikkon-font-body)`; corpo normalmente 1rem e `line-height` entre 1.65 e 1.75 | `font: 400 1rem/1.65 var(--ikkon-font-body);` | Outra sans-serif local sem necessidade |
| Marca — Cinzel | Hierarquia editorial e institucional | `h1`, `h2`, títulos de cards, logo textual | 400, 700 e 900 | Usar `var(--ikkon-font-brand)`; reservar 900 para títulos principais | `.institucional-section-title` | Aplicar em textos longos |
| Display — Korosu | Ênfase gráfica curta | Títulos específicos e composições históricas | Peso fornecido pelo arquivo | Usar `var(--ikkon-font-display)` apenas em texto curto e caixa alta quando o componente já prevê | `.institucional-card h3` | Usar como fonte de formulário ou corpo |
| Japonês | Fallback cultural | Todo conteúdo em JA | Noto Serif JP para títulos; Zen Kaku Gothic New para corpo | Carregado pelo Google Fonts e posicionado após as fontes locais | `var(--ikkon-font-brand)` | Remover os fallbacks japoneses |
| Kicker | Identificar contexto da seção | Antes de `h1` e `h2` | Mesma estrutura em claro/escuro por herança | 0.7rem, 700, `0.28em`, caixa alta, vermelho e traço de 24 px | `<p class="institucional-kicker mb-2">Cursos</p>` | Usá-lo como título sem heading associado |
| Título principal | Nomear a página | Um por rota | Hero sobre foto e introdução clara | `clamp(2.6rem, 6.5vw, 5.8rem)`, 900, linha 0.98; na introdução `clamp(2.8rem, 6vw, 5.6rem)` | `<h1 class="institucional-hero-title">…</h1>` | Mais de um `h1` ou texto longo sem quebra semântica |
| Título de seção | Iniciar blocos | Seções institucionais e blog | Centralizado ou alinhado à esquerda | `clamp(1.85rem, 4vw, 2.9rem)`, 700, linha 1.15 e sublinhado vermelho | `<h2 class="institucional-section-title">…</h2>` | Pular de `h1` para `h3` |
| Cópia editorial | Texto de apoio | Introduções, descrições e colunas | Normal e `mb-0` | 1.04rem, linha 1.75, `--ikkon-copy` | `<p class="institucional-copy">…</p>` | Centralizar parágrafos extensos no mobile |
| Metadado | Data, autor e taxonomia | Cards e detalhe do blog | Claro/escuro conforme superfície | 0.68–0.7rem, 700/900, tracking 0.1em e caixa alta | `<div class="public-blog-meta">…</div>` | Usar como texto principal |

Fontes locais ficam em `wwwroot/design-ikkon/fonts`. Todos os `@font-face` usam `font-display: swap`.

### Paleta

| Token | Valor | Finalidade | Onde é usado | Variações/regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| `--ikkon-navy` | `#00203e` | Cor institucional principal | Títulos, texto forte, botões escuros | Sobre creme/branco | `color: var(--ikkon-navy);` | Texto longo sobre fundo muito escuro sem trocar para creme |
| `--ikkon-navy-dark` | `#00162a` | Profundidade | Fundos escuros e hover do botão dark | Usar para áreas de alta ênfase | `background: var(--ikkon-navy-dark);` | Substituir navy indiscriminadamente |
| `--ikkon-red` | `#e73439` | Ação e acento | CTA primário, linha, kicker, estado ativo | Não usar em grandes blocos de texto | `background: var(--ikkon-red);` | Criar outro vermelho semelhante |
| `--ikkon-red-dark` | `#ae272b` | Estado interativo | Hover/focus de CTA vermelho | Nunca como novo tom decorativo isolado | `background: var(--ikkon-red-dark);` | Usá-lo sem estado associado |
| `--ikkon-cream` | `#f7f4e7` | Fundo editorial | Body, introduções, cards claros | Cor-base pública | `background: var(--ikkon-cream);` | Branco puro em toda página |
| `--ikkon-white` / `--ikkon-surface` | `#ffffff` | Superfície elevada | Cards, filtros, mapa | Preferir alias semântico em código novo | `background: var(--ikkon-surface);` | Hardcode repetido |
| `--ikkon-copy` | `#33506a` | Texto secundário | Parágrafos e metadados | Deve manter contraste sobre creme/branco | `color: var(--ikkon-copy);` | Usar opacidade arbitrária para todo texto |
| `--ikkon-line` | `rgba(0,32,62,.18)` | Divisores claros | Cards, inputs e seções | 1 px por padrão | `border: 1px solid var(--ikkon-line);` | Bordas cinza sem token |
| `--ikkon-line-dark` | `rgba(247,244,231,.25)` | Divisores escuros | Áreas navy | Usar somente sobre fundo escuro | `border-color: var(--ikkon-line-dark);` | Usar em branco |

### Espaçamento, forma e elevação

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Escala `--ikkon-space-*` | Ritmo consistente | Novos componentes | 0.25, 0.5, 0.75, 1, 1.5, 2, 3 e 4rem | Escolher o passo mais próximo antes de criar valor novo | `gap: var(--ikkon-space-4);` | Valores como 17px sem razão visual |
| Seção | Separar capítulos | `.institucional-section` | Fluido por viewport | `clamp(4.25rem, 8vw, 7rem)` | `<section class="institucional-section">` | Padding individual em cada página |
| Container | Limitar leitura | Todas as seções | Bootstrap responsivo | `container-xxl px-3 px-lg-4`, máximo público de 1240 px | `<div class="container-xxl px-3 px-lg-4">` | Container diferente na mesma página |
| Pill | Chips e CTA | Botões, filtros e tags | `--ikkon-radius-pill` | Usar somente quando o padrão atual for arredondado | `border-radius: var(--ikkon-radius-pill);` | Arredondar cards editoriais |
| Card editorial | Superfície rígida | Cards, filtros e mapa | Sem raio | Borda 1 px; `border-radius: 0` | `.public-blog-card` | Adicionar radius por conveniência |
| Sombra base | Separação discreta | Mapa e cards selecionados | `--ikkon-shadow` | Aplicar somente quando a superfície precisa se destacar | `box-shadow: var(--ikkon-shadow);` | Sombras em cada bloco |
| Sombra hover | Resposta interativa | Cards clicáveis | `--ikkon-shadow-hover` + translate de 3–4 px | Somente para elementos interativos | `.public-blog-card:hover` | Movimento em conteúdo estático |
| Alvo de toque | Usabilidade móvel | Menu, dots e controles | 44 px mínimo | Usar `--ikkon-touch-target` | `min-height: var(--ikkon-touch-target);` | Área clicável menor no mobile |

## Componentes

### Botões e links de ação

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Primário | Ação principal | Intro, contato e conversão | `.btn-danger`, normal/`btn-lg` | Um primário por grupo; vermelho, texto creme | `<a class="btn btn-danger btn-lg">Agendar</a>` | Dois primários concorrentes |
| Escuro | Ação forte neutra | Estado vazio e ações editoriais | `.btn-dark` | Navy; hover navy-dark | `<a class="btn btn-dark">Ver todos</a>` | Usar quando já há CTA vermelho |
| Contornado escuro | Ação secundária | Intro e contato | `.btn-outline-dark` | Fundo transparente; preenche navy no hover/focus | `<a class="btn btn-outline-dark btn-lg">Ver cursos</a>` | Misturar com outro estilo secundário no mesmo grupo |
| Contornado claro | Ação sobre fundo escuro | Áreas navy/foto | `.btn-outline-light` | Creme com transparência; inverte no hover | `<a class="btn btn-outline-light">Saiba mais</a>` | Usar sobre creme |
| CTA da navegação | Contato persistente | Header desktop/mobile | `.institucional-nav-cta` | Compacto, vermelho, pill | Partial `_PublicHeader` | Copiar a regra em outra classe |
| Link editorial | Navegação contextual | Taxonomia, footer e voltar | Taxonomia vermelha, clear link, footer | Deve ter texto descritivo e foco navegável | `<a class="public-blog-taxonomy">Cultura</a>` | “Clique aqui” |

Todos os botões usam Lato 700, caixa alta, tracking 0.08em, padding `0.68rem 1.35rem`, pill e transição de 160 ms. Disabled não aparece atualmente no frontend público; quando necessário, manter o elemento nativo `disabled`/`aria-disabled`, remover movimento e usar opacidade sem esconder o rótulo.

### Formulários

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Busca | Pesquisa textual | Filtro do blog | Input search + botão | Label real pode ser `visually-hidden`; altura mínima 48 px; submit explícito | `<input type="search" id="blog-q" name="q">` | Placeholder como único nome acessível |
| Select | Taxonomia | Categoria e tag do blog | Uma opção neutra + opções com contagem | Associar `label for`; manter seleção no GET | `<select id="blog-tag" name="tag">…</select>` | Select sem opção “todas” |
| Checkbox/radio | Não existe no público atual | Futuras preferências | Nativo estilizado pelo Bootstrap se necessário | Preservar input nativo e label clicável | `<input class="form-check-input" type="checkbox">` | Recriar com `div` |
| Textarea | Não existe no público atual | Futuro formulário de contato | Mesma borda/foco dos inputs | Label visível, limite informado e resize vertical | `<textarea class="form-control"></textarea>` | Altura fixa sem resize |
| Validação | Comunicar erro/sucesso | Não existe no filtro GET atual | Erro inline; resumo somente em formulários longos | `aria-invalid`, `aria-describedby`, mensagem textual | `<p id="campo-error" class="invalid-feedback">…</p>` | Indicar erro só por cor |

Estados:

- foco: borda vermelha e `--ikkon-focus-ring` quando o componente possui caixa;
- hover do botão de busca: vermelho escuro;
- disabled: controle nativo desabilitado, cursor coerente e opacidade; não remover do fluxo;
- loading: desabilitar submit, conservar largura e adicionar texto “Carregando” traduzido;
- erro: mensagem junto ao campo nos três idiomas;
- sucesso: mensagem com texto, não apenas verde.

### Cards

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Blog | Resumir publicação | Destaques, grid e relacionados | Destaque, badges, tags e leitura compacta | Renderizar com `_PublicBlogCard`; capa 16:10 e fallback do emblema | `<partial name="_PublicBlogCard" model="…">` | Duplicar a marcação na página |
| Semanal | Chamada compacta | Blog da semana | Título + resumo curto | Link deve envolver conteúdo completo | `.public-blog-weekly-card` | Usar como card de grid |
| Institucional | Explicar formato/benefício | Eventos e seções informativas | Conteúdo variável | Borda reta, linha vermelha superior e hover somente se houver intenção interativa | `.institucional-card` | Raio ou sombra diferente |
| Pilar | Benefício curto | Diferenciais | Quatro colunas no desktop | Sem superfície; linha superior | `.institucional-pillar` | Texto extenso |
| Nível | Progressão pedagógica | Escola | 01, 02 e 03 | Manter ordem, bordas compartilhadas e altura equivalente | `.ikkon-level-card` | Transformar em cards desconectados |
| Vídeo | Incorporar YouTube | Escola e eventos | Mesmo card com conteúdo distinto | Usar `_PublicVideoGrid`; título e descrição obrigatórios | `PublicVideoCardViewModel` | Iframe sem título |
| Mapa | Localização | Contato | Única | Iframe lazy e link externo de rota | `.institucional-map-card` | Endereço somente dentro do mapa |

### Navegação, menus e idiomas

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Header público | Navegação principal | Todas as rotas públicas | Desktop expandido e mobile colapsado | Reutilizar `_PublicHeader`; informar seção ativa por `ViewData["PublicSection"]` | `<partial name="_PublicHeader" />` | Header próprio por página |
| Menu mobile | Economizar espaço | Abaixo de 992 px | Bootstrap Collapse | Fecha ao selecionar link; alvo de toque ≥44 px | `#navLanding` | Controle custom sem estado ARIA |
| Idiomas | Alternar PT/EN/JA | Header e entrada | Estado `.is-active` | Preservar URL de retorno e mostrar JA apenas quando habilitado | `Idioma/Alterar` | Traduzir apenas parte da tela |
| Footer | Navegação de fechamento | Todas as páginas | Links institucionais, internos e sociais | Reutilizar `_PublicFooter` | `<partial name="_PublicFooter" />` | Repetir links na view |

### Entrada, heróis e carrosséis

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Entrada de escolha | Separar escola e apresentações | Home na primeira visita ou `?entrada=1` | Hover/foco por metade | Reutilizar `_PublicGateway`; lembrar somente na sessão; Escape e botão de acesso completo fecham | `data-ikkon-gateway` | Overflow de página ou imagem ocupando a metade errada |
| Carrossel hero | Contexto fotográfico | Home, escola, eventos e blog | Modo define seis imagens | Automático em 4300 ms; somente bolinhas; swipe touch; pausa com aba oculta; sem animação automática em reduced motion | `_PublicHeroCarousel` | Setas/controles extras sem necessidade |
| Galeria de eventos | Alternar registros | Seção de eventos | Três imagens | Automático em 5200 ms; pausa em hover/foco; bolinhas com teclado | `_PublicEventosGallery` | Timer duplicado |
| Introdução de página | Apresentar rota e CTAs | Escola, eventos e blog | Texto e ações tipados | Usar `_PublicPageIntro`; um `h1`; dois CTAs no máximo | `PublicPageIntroViewModel` | Copiar estrutura para cada view |

### FAQ, chips, paginação e feedback

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| FAQ | Responder dúvidas | Escola | Bootstrap Accordion | Botão real, `aria-expanded` e relação controls/id | `_PublicFaqAlunos` | Accordion em `div` clicável |
| Chip | Aplicar filtro | Blog | Categoria, `#tag` e ativo | Pill; ativo vermelho; conservar demais parâmetros na URL | `.public-blog-chips a` | Estado só por hover |
| Paginação | Navegar resultados | Blog com mais de uma página | Numérica; atual `.is-active` | `nav` com label; manter filtros na URL | `.public-blog-pagination` | Remover indicação de página atual |
| Vazio | Explicar ausência | Blog sem resultados | Com ou sem filtro ativo | Título, explicação e “Ver todos” quando filtrado | `.public-blog-empty` | Tela em branco |
| Loading | Aguardar operação | Não existe nas páginas públicas atuais | Futuro formulário/conteúdo assíncrono | Manter dimensões, texto traduzido e `aria-live` apropriado | `aria-busy="true"` | Spinner sem rótulo |
| Erro | Recuperação | Falha futura de formulário/conteúdo | Inline ou bloco | Explicar ação possível; não expor exceção | `role="alert"` | Só cor/ícone |
| Sucesso | Confirmar ação | Futuro formulário | Mensagem contextual | Texto explícito em PT/EN/JA | `role="status"` | Sumir antes da leitura |
| Bloqueado/desabilitado | Impedir ação indisponível | Não usado atualmente | Nativo ou `aria-disabled` | Explicar o motivo próximo ao controle | `disabled` | Elemento visualmente ativo sem funcionar |

Tabelas, modais e alertas não aparecem nas páginas públicas atuais. Se forem introduzidos, devem partir do Bootstrap já instalado, receber uma classe pública de composição apenas quando houver necessidade visual e seguir os tokens desta documentação.

## Imagens, logos, ícones e ilustrações

| Padrão | Finalidade | Onde é usado | Variações | Regras | Exemplo | Evitar |
| --- | --- | --- | --- | --- | --- | --- |
| Emblema mitsudomoe | Identidade/fallback | Header, entrada e capa ausente do blog | PNG | Decorativo usa `alt=""`; fallback de post recebe alt contextual pelo card | `emblema-mitsudomoe.png` | Texto alternativo redundante ao lado da marca |
| Selo | Acento de marca | Header, hero e entrada | Pequeno e completo | Decorativo, `aria-hidden`/alt vazio | `selo-pequeno.png` | Tratar como botão |
| Enso e textura | Composição editorial | Entrada e fundos | Navy, azul/preto e pontos | Usar via CSS ou imagem decorativa | `aro-enso-azul.png` | Inserir como conteúdo sem alt vazio |
| Fotografia WebP | Conteúdo visual | Heroes, galeria, escola e eventos | Hero, gateway e galeria | `object-fit: cover`; lazy fora do primeiro frame | `galeria-1.webp` | JPEG novo sem necessidade |
| Imagem histórica | Conteúdo legado | Escola e outras áreas existentes | JPG, PNG e JFIF em `wwwroot/Images` | Preservar enquanto não houver migração otimizada | `AulaTaiko.png` | Apagar sem verificar referências |
| Ícones | Controles mínimos | Toggler do Bootstrap e setas textuais | Ícone nativo do Bootstrap/Unicode | Não há biblioteca externa de ícones no frontend público; manter esse padrão | `.navbar-toggler-icon` | Adicionar biblioteca para um único ícone |

Diretórios:

- identidade, textura e ilustrações: `wwwroot/design-ikkon/assets`;
- fontes: `wwwroot/design-ikkon/fonts`;
- fotos editoriais otimizadas: `wwwroot/design-ikkon/photos`;
- acervo legado: `wwwroot/Images`.

Toda imagem de conteúdo precisa de `alt` no idioma atual. Imagem puramente decorativa deve ter `alt=""`. Não repetir no alt a legenda adjacente sem acrescentar contexto.

## Responsividade

| Faixa | Comportamento |
| --- | --- |
| Acima de 1200 px | Container até 1240 px, grids completos e tipografia fluida |
| Até 1199.98 px | Ajustes intermediários históricos em composições editoriais |
| Até 991.98 px | Header colapsado, grids de duas colunas/empilhados e layouts complexos simplificados |
| Até 767.98 px | Conteúdo em uma coluna, filtros empilhados, detalhe do blog com aside acima, CTAs adaptados e entrada vertical |
| Até 575.98 px | Escala compacta, gutters menores e controles ocupando melhor a largura |
| Até 379.98 px | Proteções para telas estreitas, quebra de títulos/ações e redução de espaços |
| Mobile landscape | Entrada e áreas altas recebem ajustes específicos |
| Desktop com altura ≤700 px | Entrada reduz elementos para evitar rolagem |
| `hover: none` | Efeitos dependentes de hover são neutralizados/adaptados |
| `prefers-reduced-motion: reduce` | Reveals aparecem sem espera e carrosséis deixam de girar automaticamente |

Regras:

- projetar primeiro sem largura fixa;
- manter `min-width: 0` em filhos de grid/flex;
- usar `overflow-wrap: anywhere` em conteúdo externo quando necessário;
- não esconder conteúdo para “caber”;
- preservar a ordem semântica quando colunas empilham;
- o aside do post fica acima do conteúdo no mobile;
- filtros e grupos de CTA devem permanecer operáveis a partir de 320 px.

## Acessibilidade

- Um `h1` por página; hierarquia sequencial.
- Links externos com `target="_blank"` usam `rel="noopener"` ou `noopener noreferrer`.
- Inputs possuem `label`, ainda que visualmente oculto.
- Carrosséis expõem `aria-hidden`, `aria-selected`, tabulação única e navegação por setas/Home/End nas bolinhas.
- Menu mobile usa Bootstrap Collapse com `aria-controls` e `aria-expanded`.
- Accordion usa botões nativos.
- Foco nunca depende apenas do mouse.
- Movimento automático respeita redução de movimento e visibilidade da aba.
- Cor não deve ser a única forma de comunicar erro, sucesso, seleção ou bloqueio.

## Exemplo completo de composição

```cshtml
@{
    var pageIntro = new PublicPageIntroViewModel(
        I18n["Kicker", "Kicker", "キッカー"],
        I18n["Título", "Title", "タイトル"],
        I18n["Descrição", "Description", "説明"],
        new(I18n["Ação principal", "Primary action", "主なアクション"], "#conteudo", "btn btn-danger btn-lg"),
        new(I18n["Ação secundária", "Secondary action", "補助アクション"], "/", "btn btn-outline-dark btn-lg"));
}

<partial name="_PublicPageIntro" model="pageIntro" />

<section id="conteudo" class="institucional-section reveal">
    <div class="container-xxl px-3 px-lg-4">
        <p class="institucional-kicker mb-2">@I18n["Contexto", "Context", "コンテキスト"]</p>
        <h2 class="institucional-section-title">@I18n["Título da seção", "Section title", "セクションタイトル"]</h2>
        <p class="institucional-copy">@I18n["Texto.", "Copy.", "本文。"]</p>
    </div>
</section>
```

Evitar no exemplo:

- estilo inline;
- cores literais fora do arquivo de tokens;
- texto sem as três traduções;
- seletor JS baseado em classe utilitária;
- novo card quando um partial existente aceita a variação;
- tamanho fixo que dependa de um único viewport.
