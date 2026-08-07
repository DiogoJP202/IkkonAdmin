# Índice da documentação

Use este índice para identificar a fonte correta antes de alterar o sistema.

## Fontes de verdade atuais

- [Arquitetura e convenções](./ARQUITETURA.md): camadas, rotas, autorização e infraestrutura transversal.
- [Padrões de serviços e operações](./PADROES_DE_SERVICOS_E_OPERACOES.md): consultas, comandos, `OperationResult` e integração MVC.
- [Módulos administrativos e permissões](./MODULOS_ADMINISTRATIVOS.md): responsabilidades e acessos do painel.
- [Área do Aluno](./AREA_DO_ALUNO.md): portal, manutenção administrativa, segurança e padrões visuais.
- [Blog e idiomas](./BLOG_E_IDIOMAS.md): workflow editorial e seleção PT/EN/JA.
- [Uploads e storage](./UPLOADS_E_STORAGE.md): mídia pública, documentos privados e validações.
- [Frontend público](./frontend-public/README.md): estrutura de Razor, CSS, JavaScript e regressão visual.
- [Operação de SEO](./seo/SEO_OPERATIONS.md): manutenção contínua de SEO, idiomas e conteúdo.

## Operação e produção

- [Deploy](../DEPLOYMENT.md): opções de hospedagem e publicação.
- [Runbook de produção](./PRODUCTION_RUNBOOK.md): secrets, migrations, health checks, backup, rollback e restore.
- [Google Agenda](../GOOGLE_AGENDA_SETUP.md): configuração da integração.
- [Usuários de acesso](../USUARIOS_ACESSO.md): credenciais exclusivas de desenvolvimento.

## Contexto auxiliar

Os arquivos em [`contexto-codex`](./contexto-codex/) resumem o projeto para
manutenção assistida. Eles não substituem as fontes de verdade acima nem o
código e as migrations atuais.

`contexto-codex/11-blog-fase-1-desenho-tecnico.md` é um registro histórico de
planejamento. Decisões antigas que divergirem da implementação atual não devem
ser usadas como especificação.

## Regra de atualização

Ao mudar uma regra, contrato, rota, variável de ambiente ou procedimento
operacional, atualize no mesmo commit o documento principal correspondente e,
quando necessário, o resumo em `contexto-codex`.
