# Relatório da refatoração do frontend público

Data: 30/07/2026

## 1. Problemas encontrados

- O CSS público dependia de um arquivo histórico de aproximadamente 424 KB e de duas camadas posteriores, sem documentação da ordem de cascata.
- Fontes, tokens, componentes, páginas e media queries estavam no mesmo arquivo editorial.
- Três famílias de nomes (`institucional-*`, `public-*` e `ikkon-*`) eram usadas sem regra registrada.
- A seleção PT/EN/JA era reimplementada em oito partials, apesar de o serviço injetado já aceitar os três idiomas.
- Escola, eventos e blog repetiam a mesma introdução com `h1` e dois CTAs.
- O card de blog era repetido em destaques, listagem e posts relacionados.
- Conversão de data para São Paulo e construção da URL do post estavam duplicadas nas views.
- O script público reunia entrada, header, reveal e dois carrosséis em um fluxo contínuo, com duas implementações de rotação.
- Não havia guia de contribuição nem inventário do sistema visual.
- A entrada e o conteúdo principal da home possuíam dois `h1`.
- O painel ainda transferia todos os estilos administrativos para qualquer rota, embora cada tela usasse apenas um domínio.
- As regras móveis do shell administrativo estavam acopladas ao antigo bloco de Configurações, causando overflow horizontal quando esse bloco deixou de ser global.

## 2. Arquivos alterados

### Contratos e helpers

- `IkkonAdmin.Web/Models/ViewModels/PublicFrontendViewModels.cs`
- `IkkonAdmin.Web/Helpers/PublicViewFormatter.cs`
- `IkkonAdmin.Web/Helpers/AdminCssModuleResolver.cs`

### Views

- `IkkonAdmin.Web/Views/_ViewImports.cshtml`
- `IkkonAdmin.Web/Views/Blog/Index.cshtml`
- `IkkonAdmin.Web/Views/Blog/Details.cshtml`
- `IkkonAdmin.Web/Views/Institucional/Escola.cshtml`
- `IkkonAdmin.Web/Views/Institucional/Eventos.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicBlogCard.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicPageIntro.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicContactCta.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicCourseCards.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicEventosGallery.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicFaqAlunos.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicFooter.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicGateway.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicHeader.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicHeroCarousel.cshtml`
- `IkkonAdmin.Web/Views/Shared/_PublicLayout.cshtml`
- `IkkonAdmin.Web/Views/Shared/_Layout.cshtml`
- `IkkonAdmin.Web/Views/Shared/_AuthLayout.cshtml`
- `IkkonAdmin.Web/Views/Shared/_AlunoLayout.cshtml`

### CSS e JavaScript

- `IkkonAdmin.Web/wwwroot/css/ikkon-tokens.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-public-foundation.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-editorial.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-editorial-responsive.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-compositions.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-responsive.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-internal-foundation.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-auth.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-core.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-dashboard.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-alunos.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-turmas.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-financeiro.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-admissoes.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-desligamentos.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-graduacoes.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-resources.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-agenda.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-inventario.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-panel.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-blog.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-admin-configuracoes.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-aluno.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-account.css`
- `IkkonAdmin.Web/wwwroot/css/ikkon-internal-themes.css`
- `IkkonAdmin.Web/wwwroot/js/landing.js`

Removido:

- `IkkonAdmin.Web/wwwroot/css/site.css`

### Segurança e validação

- `IkkonAdmin.Web/IkkonAdmin.Web.csproj`
- `IkkonAdmin.Tests/BlogContentSanitizerTests.cs`
- `IkkonAdmin.Tests/CssArchitectureTests.cs`
- `IkkonAdmin.VisualTests/IkkonAdmin.VisualTests.csproj`
- `IkkonAdmin.VisualTests/Program.cs`
- `IkkonAdmin.VisualTests/Baselines/*.png`
- `IkkonAdmin.slnx`
- `scripts/visual-regression.ps1`

### Documentação

- `docs/frontend-public/README.md`
- `docs/frontend-public/VISUAL_STANDARDS.md`
- `docs/frontend-public/INTERNAL_CSS_ARCHITECTURE.md`
- `docs/frontend-public/REFACTOR_REPORT.md`

Arquivos de controllers, serviços de negócio, entidades, banco e integrações não foram alterados.

## 3. Componentes criados, removidos ou consolidados

### Criados

- `_PublicPageIntro`: recebe `PublicPageIntroViewModel` e padroniza kicker, `h1`, descrição e ações.
- `_PublicBlogCard`: recebe `PublicBlogCardPartialViewModel` e preserva as variações de destaque, badges, tags e rótulo de leitura.
- `PublicViewFormatter`: concentra data do blog no fuso de São Paulo e URL pública de post.
- `ikkon-tokens.css`: concentra fontes e decisões visuais compartilhadas.
- `ikkon-public-foundation.css`: isola a fundação pública antes hospedada no CSS global.
- `ikkon-compositions.css`: separa entrada, carrossel e composições dos componentes editoriais-base.
- `ikkon-internal-foundation.css`: concentra os fundamentos mínimos dos layouts autenticados.
- `ikkon-auth.css` e `ikkon-aluno.css`: isolam autenticação e portal do aluno.
- `ikkon-admin-core.css`: concentra shell, menu móvel, motion e contratos administrativos compartilhados.
- `ikkon-admin-{dominio}.css`: isola cada domínio administrativo e permite carregamento por rota.
- `AdminCssModuleResolver`: mantém em um único ponto a relação entre controller e módulos CSS.
- `ikkon-account.css`: concentra configurações usadas por administradores e alunos.
- `ikkon-internal-themes.css`: mantém variações escuras em uma camada transversal.
- `IkkonAdmin.VisualTests`: captura e compara 14 estados de referência em Chromium.

### Consolidados

- Três introduções de página passaram a usar `_PublicPageIntro`.
- Três marcações completas de card passaram a usar `_PublicBlogCard`.
- Oito helpers locais de idioma foram eliminados em favor de `IViewTextService`.
- Dois temporizadores/carrosséis passaram a usar `createAutoRotator`.
- Navegação de bolinhas passou a usar `bindDotNavigation`.
- Entrada, header, reveal, hero e galeria passaram a ter inicializadores isolados.

### Removidos

- Funções `FormatDate` e `BuildBlogUrl` duplicadas nas views.
- Helpers locais `T(pt, en, ja)`.
- Segundo `h1` da home: a pergunta da entrada agora é `h2`, mantendo seletor/id e aparência.

Nenhuma funcionalidade ou componente visual foi removido.

## 4. Duplicações eliminadas

| Duplicação | Antes | Depois |
| --- | --- | --- |
| Introdução interna | 3 blocos completos | 1 partial tipado |
| Card do blog | 3 blocos completos | 1 partial com quatro opções booleanas de apresentação |
| Data do blog | 2 funções | 1 formatter |
| URL do post | 2 funções | 1 formatter |
| Localização PT/EN/JA | 8 helpers | Serviço existente usado diretamente |
| Rotação automática | 2 implementações | 1 controlador |
| Teclado das bolinhas | Implementação só no hero | 1 função compartilhada para hero e galeria |
| Constantes JS | Literais espalhados | Seletores e tempos centralizados |
| Fontes/tokens | Misturados ao CSS de página | Camada própria |
| Fundação pública | Transferência do `site.css` global (~424 KB) | Arquivo público dedicado (~37 KB) |
| CSS público duplicado | Fundação mantida também no arquivo interno | Uma única origem em `ikkon-public-foundation.css` |
| CSS interno monolítico | Os três layouts carregavam 387.846 bytes | Cada layout e rota administrativa carrega somente seus módulos |
| Media queries editoriais | Misturadas com regras-base | Camadas responsivas posicionadas na cascata |
| Verificação visual | Sem screenshots versionados | 14 baselines e comparação pixel a pixel |

## 5. Padrões técnicos adotados

- Razor partial tipado somente para repetição ou responsabilidade real.
- View responsável por conteúdo; partial responsável por marcação; helper responsável por formatação.
- Localização sempre com `I18n[ptBr, enUs, jaJp]`.
- Classes existentes preservadas; convenção documentada por prefixo.
- Estado visual com `.is-*`; novos hooks de comportamento com `data-*`.
- CSS em ordem explícita: Bootstrap, fundação pública, tokens, editorial, responsivo intermediário, composições e responsivo final.
- CSS interno em ordem explícita por layout; o painel usa núcleo + módulos resolvidos por controller + temas.
- `ikkon-account.css` é carregado somente em Configurações, tanto no painel quanto no portal do aluno.
- Testes impedem o retorno de `site.css`, inversão da cascata ou mistura entre módulos internos.
- Sem estilo inline e sem script inline nas páginas públicas.
- Temporizadores pausados em aba oculta e desativados em redução de movimento.
- Scroll do header limitado por `requestAnimationFrame`.
- Bolinhas acessíveis por clique, Tab, setas, Home e End.
- Imagens abaixo da dobra continuam lazy; primeiro slide continua eager.
- Nenhuma dependência de runtime foi adicionada ao frontend.
- `Microsoft.Playwright` foi isolado no projeto de validação visual.
- `HtmlSanitizer` foi atualizado para `9.1.973`, resolvendo `AngleSharp` para `1.6.0`.

## 6. Documentação do padrão visual

O inventário completo está em `VISUAL_STANDARDS.md` e cobre:

- famílias, pesos, tamanhos e alturas de linha;
- paleta e usos semânticos;
- botões e estados;
- inputs, selects e regras futuras para outros campos;
- logos, imagens, ilustrações e ícones;
- espaçamentos, containers e gaps;
- bordas, raios, sombras e elevação;
- headings, cópia, labels e metadados;
- cards, FAQ, navegação, entrada, carrosséis, chips, paginação e estados;
- breakpoints e comportamento responsivo;
- loading, vazio, erro, sucesso, bloqueado e disabled;
- nomenclatura e estrutura recomendada para novas páginas.

Cada grupo informa finalidade, uso, variações, regras, exemplo e o que evitar.

## 7. Pontos que ainda precisam de atenção

1. `ikkon-internal-themes.css` continua transversal e reúne variações escuras de muitos módulos; uma divisão futura exige uma matriz autenticada específica para troca de tema.
2. Media queries da fundação histórica permanecem na ordem original dentro de `ikkon-public-foundation.css`; regras novas devem usar as camadas responsivas atuais.
3. O baseline do blog depende do conteúdo presente no banco de desenvolvimento e deve ser reaprovado quando esse conteúdo mudar intencionalmente.
4. Compilar diretamente enquanto `dotnet run` está ativo na pasta do OneDrive pode bloquear `obj/Debug/net10.0/rpswa.dswa.cache.json`.
5. Novos controllers administrativos precisam ser registrados no `AdminCssModuleResolver` e no teste de arquitetura.

## 8. Evidências de preservação

- Build da solução: concluído, zero erros e zero warnings.
- Testes automatizados: 126 executados, 126 aprovados.
- Quatro testes específicos confirmam remoção de script/event handler/URL insegura, preservação de HTML permitido, normalização de YouTube e remoção de iframe externo.
- Sintaxe de `landing.js`: analisada com o parser do Node, válida.
- CSS público: carregado nas cinco rotas sem erro e com a ordem de cascata documentada.
- Payload CSS público não comprimido: de 505.113 para 121.593 bytes, redução de 75,9%.
- CSS global interno: de 424.528 para 387.846 bytes após eliminar 1.753 linhas duplicadas.
- CSS interno por layout: autenticação 3.386 bytes; aluno 68.203 bytes nas rotas comuns e 90.762 bytes em Configurações.
- CSS administrativo por rota: de 96.114 a 120.631 bytes, redução de 68,9% a 75,2% sobre os 387.846 bytes anteriores.
- A extração mecânica cobriu todas as regras não vazias do CSS legado; nenhum bloco ficou sem destino.
- Navegação autenticada real validada nas 12 combinações administrativas, em Configurações e no portal do aluno, incluindo viewport móvel de 390 × 844, sem overflow horizontal, recursos ausentes ou erros de console.
- Cada rota administrativa carregou somente núcleo, módulos declarados no resolver e temas; Configurações e Blog receberam suas dependências adicionais na ordem prevista.
- Assets: todas as referências locais literais das views públicas e dos CSS existem.
- Comparação controlada antes/depois da migração CSS: 10 de 10 screenshots idênticos byte a byte.
- Auditoria interna: nenhum dos 122 seletores públicos específicos é usado pelos layouts administrativos, de autenticação ou do aluno.
- Regressão visual reproduzível: 14 de 14 baselines com 0,0000% de pixels alterados, incluindo os dois logins.
- Auditoria NuGet: nenhum pacote vulnerável; `AngleSharp 0.17.1` foi substituído por `1.6.0`.
- Não houve mudança em controller, regra de negócio, consulta do blog, banco ou integração.
- Os componentes consolidados mantêm as classes CSS, ordem dos elementos, textos, URLs, `target`, `rel`, `alt`, lazy loading, badges, tags e rótulos existentes.
- Os valores originais de fonte, paleta, sombra e foco foram movidos para tokens sem alteração.
- Carrosséis continuam automáticos, com as mesmas durações: hero 4300 ms e eventos 5200 ms.
- Entrada continua sendo forçada por `?entrada=1`, lembrada por sessão e fechada por botão/Escape.
