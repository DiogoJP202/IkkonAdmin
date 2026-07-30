# Frontend público

Este diretório documenta a arquitetura e o sistema visual das páginas públicas:

- entrada de escolha;
- home;
- escola;
- eventos;
- listagem e detalhes do blog;
- cabeçalho, rodapé, contato, FAQ, galerias, vídeos e carrosséis compartilhados.

O objetivo da organização é preservar o visual existente e tornar novas alterações previsíveis. O guia visual completo está em [VISUAL_STANDARDS.md](VISUAL_STANDARDS.md), e a divisão das áreas autenticadas está em [INTERNAL_CSS_ARCHITECTURE.md](INTERNAL_CSS_ARCHITECTURE.md).

## Arquitetura adotada

### Razor

| Camada | Responsabilidade | Local |
| --- | --- | --- |
| Página | Conteúdo e composição específicos de uma rota | `Views/Institucional` e `Views/Blog` |
| Componente público | Marcação recorrente com responsabilidade única | `Views/Shared/_Public*.cshtml` |
| Modelo de componente | Contrato tipado entre página e partial | `Models/ViewModels/PublicFrontendViewModels.cs` |
| Formatação de apresentação | Data, URL e outras transformações sem regra de negócio | `Helpers/PublicViewFormatter.cs` |
| Localização | Seleção PT/EN/JA | `IViewTextService` injetado por `_ViewImports.cshtml` |
| Layout | Metadados, ordem de CSS/JS e estrutura do documento | `Views/Shared/_PublicLayout.cshtml` |

Componentes consolidados:

- `_PublicPageIntro`: introdução interna usada por escola, eventos e blog.
- `_PublicBlogCard`: card-base usado em destaques, listagem e relacionados.
- `_PublicHeroCarousel`: carrossel visual compartilhado, configurado por modo.
- Partials existentes de cabeçalho, entrada, rodapé, contato, cursos, FAQ, vídeos e galeria foram mantidos e aprimorados.

Não criar um partial apenas para diminuir algumas linhas. Um novo componente deve ter repetição real, contrato claro ou responsabilidade independente.

### CSS

A ordem abaixo é parte do contrato visual:

1. `bootstrap.min.css`: grid e componentes-base.
2. `ikkon-public-foundation.css`: fundação pública extraída do CSS histórico.
3. `ikkon-tokens.css`: fontes e tokens semânticos.
4. `ikkon-editorial.css`: componentes editoriais-base.
5. `ikkon-editorial-responsive.css`: ajustes históricos que precisam ocorrer antes das composições.
6. `ikkon-compositions.css`: entrada, carrossel e composições editoriais.
7. `ikkon-responsive.css`: ajustes finais agrupados por página e breakpoint.
8. `public-construction-tag.css`: somente quando a etiqueta de construção está habilitada.

Não inverter essa ordem. A camada responsiva intermediária existe porque sua posição participa da cascata. A fundação dedicada preserva internamente a ordem histórica, inclusive seus blocos de mídia, para manter equivalência.

As áreas internas não usam mais um CSS monolítico. Cada layout carrega somente sua fundação e seus módulos:

- autenticação: `ikkon-internal-foundation.css` e `ikkon-auth.css`;
- painel: fundação, `ikkon-admin-core.css`, módulos resolvidos pela rota e `ikkon-internal-themes.css`;
- aluno: fundação, `ikkon-aluno.css` e `ikkon-internal-themes.css`;
- configurações: acrescenta `ikkon-account.css` ao painel ou ao portal, conforme o perfil autenticado.

Não reintroduzir `site.css`. A ordem completa e as regras de contribuição estão no guia de arquitetura interna.

### JavaScript

`wwwroot/js/landing.js` usa inicializadores independentes:

- `initGateway`;
- `initHeroCarousels`;
- `initRevealAnimations`;
- `initPublicHeader`;
- `initEventGallery`.

Carrosséis compartilham `createAutoRotator` e `bindDotNavigation`. Seletores, tempos e limites ficam centralizados no início do arquivo.

Regras:

- usar `data-*` para novos hooks de comportamento;
- usar `.is-*` apenas para estado visual;
- não armazenar dados de negócio no DOM;
- respeitar `prefers-reduced-motion`;
- suspender temporizadores quando a aba estiver oculta;
- atualizar eventos de scroll por `requestAnimationFrame`;
- não adicionar uma biblioteca para comportamentos disponíveis com a plataforma e o Bootstrap já carregado.

## Convenções de nomenclatura

| Prefixo | Uso | Exemplo |
| --- | --- | --- |
| `institucional-*` | Primitivos públicos já estabelecidos | `.institucional-section-title` |
| `public-*` | Componentes ligados a um domínio ou conteúdo | `.public-blog-card` |
| `ikkon-*` | Composições do sistema visual | `.ikkon-page-intro` |
| `is-*` | Estado visual temporário | `.is-active`, `.is-visible` |
| `data-*` | Hook exclusivo de JavaScript | `data-ikkon-carousel` |
| `_Public*` | Partial público compartilhado | `_PublicBlogCard.cshtml` |
| `Public*ViewModel` | Contrato tipado de apresentação | `PublicPageIntroViewModel` |

Evitar:

- classe que descreve cor ou posição, como `.texto-vermelho-esquerda`;
- seletor JavaScript baseado em texto, posição no DOM ou classe utilitária do Bootstrap;
- novos nomes genéricos como `.card2`, `.box`, `.content-wrapper`;
- mistura de português e inglês dentro do mesmo padrão novo.

## Estrutura recomendada para uma nova página

```cshtml
@{
    Layout = "_PublicLayout";
    ViewData["Title"] = I18n["Título", "Title", "タイトル"];
    ViewData["PublicSection"] = "secao";
    ViewData["ContactMode"] = "geral";
    ViewData["JapanesePublicEnabled"] = true;

    var pageIntro = new PublicPageIntroViewModel(
        I18n["Contexto", "Context", "コンテキスト"],
        I18n["Título da página", "Page title", "ページタイトル"],
        I18n["Descrição.", "Description.", "説明。"],
        new(I18n["Ação", "Action", "アクション"], "#conteudo", "btn btn-danger btn-lg"),
        new(I18n["Ação secundária", "Secondary action", "補助アクション"], "/", "btn btn-outline-dark btn-lg"));
}

<div class="institucional-page public-secao-page">
    <partial name="_PublicHeader" />
    <partial name="_PublicPageIntro" model="pageIntro" />

    <main id="conteudo">
        <section class="institucional-section reveal">
            <div class="container-xxl px-3 px-lg-4">
                <!-- conteúdo específico -->
            </div>
        </section>
    </main>

    <partial name="_PublicContactCta" />
    <partial name="_PublicFooter" />
</div>
```

Checklist:

1. Definir título, seção ativa, modo de contato e idiomas.
2. Reutilizar header, introdução, contato e footer.
3. Manter um único `h1`; iniciar seções com `h2`.
4. Usar `container-xxl px-3 px-lg-4`.
5. Incluir PT/EN/JA na mesma chamada `I18n`.
6. Usar imagem WebP quando possível, `loading="lazy"` fora do conteúdo inicial e `alt` contextual.
7. Validar 320 px, 375 px, 576 px, 768 px, 992 px e desktop amplo.
8. Validar teclado, foco, redução de movimento e ausência de conteúdo.

## Decisões da refatoração

1. As classes visuais existentes foram preservadas para evitar regressões de cascata.
2. A introdução de página virou componente porque a mesma estrutura aparecia em três rotas.
3. Os cards do blog viraram um componente configurável pequeno; as variações alteram apenas badges, tags, layout destacado e rótulo de leitura.
4. Data e URL de post foram retiradas das views e centralizadas em `PublicViewFormatter`.
5. Os helpers locais de idioma foram removidos; as páginas públicas habilitam os três idiomas e usam diretamente o serviço existente.
6. Fontes, paleta, elevação, espaçamento, forma e alvo de toque foram centralizados em tokens.
7. O JavaScript permaneceu sem dependência nova e foi separado por inicializadores.
8. Regras de mídia editoriais foram movidas para arquivos responsivos nos mesmos pontos lógicos da cascata.
9. A fundação pública foi extraída de `site.css`; o layout público deixou de transferir o arquivo global de aproximadamente 424 KB.
10. A migração CSS foi comparada em cinco rotas, desktop e mobile, antes de o novo baseline ser registrado.
11. O CSS interno foi dividido por layout e por rota administrativa; autenticação, painel e aluno não transferem mais estilos exclusivos das outras áreas.
12. Configurações de conta são carregadas somente nessa rota; temas permanecem transversais porque atendem administradores e alunos.
13. O mapeamento de controller para CSS administrativo foi centralizado em `AdminCssModuleResolver` e protegido por teste automatizado.

## Como validar alterações

```powershell
dotnet build IkkonAdmin.slnx
dotnet test IkkonAdmin.Tests/IkkonAdmin.Tests.csproj --no-build
pwsh -NoProfile -File scripts/visual-regression.ps1 -SkipBrowserInstall
```

Se o projeto estiver aberto com `dotnet run` dentro de uma pasta sincronizada pelo OneDrive, o arquivo de cache de static web assets pode ficar bloqueado. Nesse caso, compile uma cópia limpa sem `bin` e `obj`, ou encerre o processo antes da validação.

### Regressão visual

O projeto `IkkonAdmin.VisualTests` usa Playwright em Chromium e compara pixels sem alterar o runtime da aplicação. O script:

- compila a solução;
- inicia a aplicação escondida em ambiente Development;
- valida entrada, home, escola, eventos, blog e os logins administrativo/do aluno;
- captura `1440x1000` e `390x844`;
- conclui animações antes da captura;
- compara com tolerância padrão de 0,1% dos pixels;
- falha quando a página pública emite erro JavaScript não tratado;
- salva capturas e diffs em `artifacts/visual-regression`.

Na primeira execução, o script instala o Chromium compatível. Para validar:

```powershell
pwsh -NoProfile -File scripts/visual-regression.ps1
```

Use `-SkipBrowserInstall` nas execuções seguintes. Se a aplicação já estiver aberta na mesma URL, use também `-UseExistingServer`.

Atualize o baseline somente quando a mudança visual for intencional e tiver sido revisada:

```powershell
pwsh -NoProfile -File scripts/visual-regression.ps1 -UpdateBaseline
```

Os arquivos aprovados ficam em `IkkonAdmin.VisualTests/Baselines`. O baseline do blog reflete os dados disponíveis no banco de desenvolvimento; alterações editoriais intencionais podem exigir nova aprovação.

Rotas de regressão:

- `/?entrada=1`
- `/`
- `/escola`
- `/eventos`
- `/blog`
- `/auth/login`
- `/aluno/login`

## Pontos de manutenção

- `ikkon-internal-themes.css` permanece transversal e relativamente grande porque reúne variações escuras de todos os módulos. Uma divisão futura exige validar alternância de tema em cada rota autenticada.
- Toda nova rota administrativa deve declarar seus módulos em `AdminCssModuleResolver`; não adicionar links condicionais dispersos nas views.
- `ikkon-public-foundation.css` conserva a ordem de seus media queries históricos. Código público novo deve ir para a camada editorial/composição e para os arquivos responsivos, não para a fundação.
- Mudanças em conteúdo persistido do blog podem alterar o baseline mesmo sem mudança de CSS.
- Compilar com um processo aberto na pasta do OneDrive ainda pode bloquear arquivos de `obj`; o script visual gerencia seu próprio processo e o encerra ao final.
