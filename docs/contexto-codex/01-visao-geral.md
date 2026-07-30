# Visão geral do IkkonAdmin

## O que é

O IkkonAdmin é um sistema web em ASP.NET Core MVC para apoiar a operação administrativa da IKKON SPTD / Escola de Taiko. A proposta é substituir controles manuais feitos em planilhas, Notion, WhatsApp e registros financeiros soltos por uma central interna organizada.

## Frentes do sistema

- Site público institucional: apresenta a escola, cursos, eventos, vídeos, localização, FAQ e contato.
- Painel administrativo: área interna para funcionários e administradores operarem alunos, turmas, financeiro, processos e configurações.
- Área do Aluno: portal separado para o aluno consultar dashboard, perfil, financeiro, turmas, aulas, frequência, documentos, comunicados, eventos e conquistas.
- Integrações e módulos de apoio: Google Agenda, inventário, blog, configurações, auditoria e controle de acesso.

## Separação conceitual

- Site público não exige login e usa `InstitucionalController`, `_PublicLayout.cshtml` e views em `Views/Institucional`.
- Painel administrativo exige autenticação de funcionário/admin e usa o layout principal `_Layout.cshtml`, com sidebar e topbar.
- Área do Aluno exige role de aluno e usa `AlunoAreaController`, `AlunoAuthController` e `_AlunoLayout.cshtml`.
- Área do Aluno resolve dados pelo usuário autenticado e pelo vínculo `UsuarioSistema.AlunoId`.

## Principais módulos existentes

- Dashboard operacional.
- Alunos.
- Turmas.
- Financeiro.
- Admissões.
- Desligamentos.
- Graduações.
- Configurações da conta.
- Painel administrativo.
- Usuários, cargos e permissões.
- Logs/auditoria.
- Inventário.
- Agenda/Google Agenda.
- Site institucional público.
- Área do Aluno.
- Blog público e administrativo com versões `pt-BR`, `en-US` e `ja-JP`.

## Objetivo geral

Centralizar dados e processos da escola para reduzir erro humano, melhorar visibilidade financeira, organizar a gestão dos alunos e criar uma base evolutiva para novas áreas, como portal do aluno e conteúdos públicos.

## Tecnologias usadas

- .NET `net10.0`.
- ASP.NET Core MVC.
- Razor Views.
- Entity Framework Core.
- SQL Server.
- Cookie Authentication.
- Authorization Policies / Claims.
- `OperationResult` para comandos de domínio.
- Services de consulta separados em módulos maiores.
- Bootstrap 5.
- CSS customizado modular em `wwwroot/css/ikkon-*.css`, separado entre frontend público, autenticação, painel, aluno, conta e temas.
- JavaScript leve em `wwwroot/js/site.js`, `landing.js` e `configuracoes.js`.
- Google Calendar API via `HttpClient`, OAuth e Data Protection.

