# Arquitetura CSS das áreas internas

Esta documentação registra a divisão do antigo `site.css` por responsabilidade. A migração preservou todas as declarações existentes e alterou apenas a forma de carregamento.

## Módulos

| Arquivo | Finalidade | Layouts que carregam | Evitar |
| --- | --- | --- | --- |
| `ikkon-internal-foundation.css` | Tokens internos, altura do documento e pequenos fundamentos compartilhados | Administrativo, autenticação e aluno | Estilos de página ou de um domínio específico |
| `ikkon-auth.css` | Login de funcionários, administradores e alunos | `_AuthLayout` | Regras do painel ou do portal autenticado |
| `ikkon-admin-core.css` | Shell, sidebar, topbar, responsividade e contratos de movimento compartilhados pelo painel | `_Layout` | Estilos exclusivos de uma rota administrativa |
| `ikkon-admin-{dominio}.css` | Componentes e páginas de Dashboard, Alunos, Turmas, Financeiro, Admissões, Desligamentos, Graduações, Agenda, Inventário, Painel e Blog | `_Layout`, somente nas rotas resolvidas para o domínio | Regras de outro domínio ou do shell |
| `ikkon-admin-resources.css` | Primitivos realmente compartilhados por Agenda e Inventário | Rotas de Agenda e Inventário | Componentes exclusivos de apenas uma dessas páginas |
| `ikkon-admin-configuracoes.css` | Apresentação administrativa específica de Configurações | Rota de Configurações no painel | Shell, estilos de Alunos ou regras do portal |
| `ikkon-aluno.css` | Shell, navegação e componentes do portal do aluno | `_AlunoLayout` | Regras administrativas ou de autenticação |
| `ikkon-account.css` | Conta, senha e preferências usadas pelos dois perfis autenticados | Somente a rota de Configurações em `_Layout` e `_AlunoLayout` | Regras exclusivas de uma das áreas |
| `ikkon-internal-themes.css` | Tokens e correções dos temas escuros administrativo e aluno | `_Layout` e `_AlunoLayout` | Estilos-base de componentes; o arquivo deve conter apenas variações de tema |

## Ordem de carregamento

### Painel administrativo

```cshtml
<link rel="stylesheet" href="~/css/ikkon-internal-foundation.css" />
<link rel="stylesheet" href="~/css/ikkon-admin-core.css" />
@foreach (var cssModule in AdminCssModuleResolver.Resolve(controllerName))
{
    <link rel="stylesheet" href="~/css/@cssModule" />
}
<link rel="stylesheet" href="~/css/ikkon-internal-themes.css" />
```

O `AdminCssModuleResolver` mantém a relação entre controller e arquivos. Configurações inclui `ikkon-account.css`; Blog inclui `ikkon-admin-panel.css` e `ikkon-admin-blog.css`; Agenda e Inventário incluem primeiro `ikkon-admin-resources.css`.

### Autenticação

```html
<link rel="stylesheet" href="~/css/ikkon-internal-foundation.css" />
<link rel="stylesheet" href="~/css/ikkon-auth.css" />
```

### Portal do aluno

```html
<link rel="stylesheet" href="~/css/ikkon-internal-foundation.css" />
<link rel="stylesheet" href="~/css/ikkon-aluno.css" />
<!-- ikkon-account.css é incluído somente em /configuracoes -->
<link rel="stylesheet" href="~/css/ikkon-internal-themes.css" />
```

Essa ordem faz parte da cascata e é protegida por testes automatizados.

## Regras para manutenção

1. Colocar a regra no arquivo da área que realmente utiliza o componente.
2. Usar `ikkon-account.css` somente quando a mesma tela ou componente existir no painel e no portal do aluno.
3. Manter variações escuras em `ikkon-internal-themes.css`, sempre escopadas por `body.admin-theme-dark` ou `body.aluno-theme-dark`.
4. Evitar seletores globais. Preferir os prefixos já existentes, como `dashboard-v2-*`, `alunos-v2-*`, `configuracoes-v2-*` e `aluno-portal-*`.
5. Não reintroduzir `site.css` como agregador.
6. Não importar um módulo interno dentro de outro com `@import`; a ordem deve permanecer explícita no layout.
7. Ao criar uma nova área compartilhada, confirmar repetição real antes de criar outro arquivo transversal.
8. Ao criar um controller administrativo, registrar seus módulos em `AdminCssModuleResolver` e atualizar o teste de mapeamento.
9. Manter no `ikkon-admin-core.css` apenas contratos necessários em todas as rotas, incluindo shell, menu móvel, motion e compatibilidade responsiva compartilhada.

## Métricas sem compressão

| Layout | Antes | Depois | Redução |
| --- | ---: | ---: | ---: |
| Autenticação | 387.846 bytes | 3.386 bytes | 99,1% |
| Portal do aluno, rotas comuns | 387.846 bytes | 68.203 bytes | 82,4% |
| Portal do aluno, Configurações | 387.846 bytes | 90.762 bytes | 76,6% |
| Administrativo, conforme a rota | 387.846 bytes | 96.114–120.631 bytes | 75,2–68,9% |

### Payload administrativo por rota

| Domínio | CSS transferido |
| --- | ---: |
| Painel de administração | 96.114 bytes |
| Dashboard | 97.152 bytes |
| Turmas | 97.304 bytes |
| Inventário | 98.730 bytes |
| Financeiro | 99.871 bytes |
| Admissões | 100.598 bytes |
| Configurações | 101.518 bytes |
| Graduações | 101.565 bytes |
| Desligamentos | 102.153 bytes |
| Alunos | 104.420 bytes |
| Blog | 106.460 bytes |
| Agenda | 120.631 bytes |

Os valores incluem fundação interna, núcleo administrativo, módulo da rota e temas. Eles são bytes não comprimidos e servem como referência de arquitetura, não como orçamento de rede comprimido.

## Validação obrigatória

```powershell
dotnet build IkkonAdmin.slnx
dotnet test IkkonAdmin.Tests/IkkonAdmin.Tests.csproj --no-build
pwsh -NoProfile -File scripts/visual-regression.ps1 -SkipBrowserInstall
```

Os testes de arquitetura verificam:

- ausência do arquivo legado;
- módulos esperados em cada layout;
- ordem dos links;
- isolamento entre autenticação, administração, aluno e configurações;
- mapeamento de cada controller para módulos existentes;
- presença do contrato responsivo móvel no núcleo administrativo;
- presença da camada transversal de temas.
