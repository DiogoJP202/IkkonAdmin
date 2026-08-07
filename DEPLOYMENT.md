# Deploy do IkkonAdmin

Este guia descreve uma forma simples de colocar o IkkonAdmin no ar para validação do cliente.

> Para produção com dados reais, use o [runbook de produção](./docs/PRODUCTION_RUNBOOK.md). Ele substitui as recomendações simplificadas de demonstração sobre migrations, storage, secrets, backup e rollback.

## Decisão recomendada para esta fase

Como o projeto é **ASP.NET Core MVC com Razor Views**, o frontend e o backend estão no mesmo processo da aplicação. Por isso, a opção mais simples agora é publicar o projeto inteiro como **um único serviço web**.

Separar frontend no Vercel neste momento não é recomendado, porque exigiria transformar a interface em SPA/static frontend ou criar uma API separada. Isso aumentaria o trabalho sem ganho real para a apresentação inicial ao cliente.

## Arquitetura sugerida para teste

```text
Cliente
  ↓
Render Web Service ou Azure App Service
  ↓
IkkonAdmin ASP.NET Core MVC
  ↓
Azure SQL Database
```

## Opção A - Mais simples e estável

Use tudo na Azure:

- App: Azure App Service
- Banco: Azure SQL Database Free Offer
- Domínio: domínio temporário do Azure ou domínio customizado depois

Vantagens:

- Melhor compatibilidade com ASP.NET Core e SQL Server.
- Menos peças para configurar.
- Azure SQL é SQL Server gerenciado.
- Evita adaptação para PostgreSQL.

Observação:

- Para evitar hibernação no App Service, use plano dedicado que suporte Always On. Planos Free/Shared são para desenvolvimento/teste e têm limitações.

## Opção B - Mais barata para teste rápido

Use:

- App: Render Web Service com Docker
- Banco: Azure SQL Database Free Offer
- Keep-alive: UptimeRobot, Better Stack ou cron-job.org chamando `/health`

Vantagens:

- Deploy simples via GitHub.
- Render consegue rodar o app via Docker.
- Azure SQL mantém compatibilidade com Entity Framework Core + SQL Server.

Limitações:

- Render Free hiberna após período sem tráfego.
- O filesystem do Render Free é efêmero, então uploads locais não devem ser usados como armazenamento permanente.
- Para uma demonstração, o keep-alive pode reduzir cold starts, mas isso não substitui um plano pago/sempre ativo.

## Por que não Vercel agora?

Vercel é excelente para Next.js, React, Vite, Astro e outros frontends, mas este projeto não é um frontend estático. Ele usa Razor Views renderizadas pelo ASP.NET Core.

Para usar Vercel corretamente, seria necessário separar a solução em:

```text
Frontend separado em React/Next.js
Backend separado em API ASP.NET Core
```

Isso pode ser feito no futuro, mas não é necessário para validar o sistema com o cliente agora.

## Endpoints de healthcheck

Os endpoints disponíveis são:

```text
/health/live
/health/ready
/health/sql
/health/storage
```

Use `/health/live` para liveness e `/health/ready` para receber tráfego somente quando SQL Server e storage privado estiverem disponíveis.

Exemplo:

```text
https://seu-app.onrender.com/health/ready
```

## Deploy no Render com Docker

Arquivos adicionados:

- `Dockerfile`
- `.dockerignore`
- `render.yaml`

### 1. Criar Web Service

1. Acesse Render.
2. Clique em `New` > `Web Service`.
3. Conecte o repositório GitHub.
4. Escolha deploy via Docker.
5. Use o `Dockerfile` da raiz.
6. Configure a porta como `8080`, se solicitado.

### 2. Variáveis de ambiente obrigatórias

Configure no Render:

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<connection-string-do-azure-sql>
```

Exemplo de connection string para Azure SQL:

```text
Server=tcp:<servidor>.database.windows.net,1433;Initial Catalog=IkkonAdminDb;Persist Security Info=False;User ID=<usuario>;Password=<senha>;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 3. Variáveis opcionais do Google Agenda

Se for testar Google Agenda no ambiente publicado:

```text
GoogleAgenda__ApplicationName=IkkonAdmin
GoogleAgenda__CalendarId=primary
GoogleAgenda__CredentialsPath=
GoogleAgenda__OAuthClientSecretsPath=<caminho-do-arquivo-json-no-ambiente>
GoogleAgenda__RedirectUri=https://seu-app.onrender.com/admin/agenda/google/callback
GoogleAgenda__TimeZone=America/Sao_Paulo
```

Se não for testar Google Agenda agora, pode deixar essas variáveis sem configurar. A tela da agenda pode avisar que a integração não está configurada.

## Uploads e storage

O sistema salva arquivos em duas áreas diferentes:

- imagens públicas do blog em `wwwroot/uploads/blog`;
- documentos privados de alunos em `App_Data/uploads/documentos`.

Em desenvolvimento local isso é suficiente. Em hospedagens com filesystem efêmero, como Render Free, esses arquivos podem ser perdidos quando o container for recriado ou movido.

Para demonstração simples, uploads locais podem ser aceitos com essa limitação conhecida. Para ambiente real, use storage persistente externo:

- Azure Blob Storage;
- Amazon S3;
- Cloudflare R2;
- outro storage de objetos.

Documentos de aluno não devem ser movidos para uma pasta pública sem controle de autorização. Eles precisam continuar passando por download autenticado/autorizado.

Guia detalhado: [Uploads e storage](./docs/UPLOADS_E_STORAGE.md).

## Banco de dados recomendado

Use **Azure SQL Database Free Offer** para manter compatibilidade com SQL Server.

### Passos gerais

1. Criar uma conta Azure.
2. Criar um SQL Server lógico.
3. Criar um Azure SQL Database usando o free offer.
4. Liberar firewall para acesso do serviço de hospedagem.
5. Copiar a connection string ADO.NET.
6. Configurar `ConnectionStrings__DefaultConnection` no provedor de hospedagem.

Migrations não são executadas pelo processo web em Production. A pipeline deve executar `dotnet ef database update` antes da publicação; qualquer falha interrompe o deploy. O startup recusa schema com migrations pendentes.

## Keep-alive para Render Free

Se usar Render Free, configure um monitor HTTP chamando:

```text
https://seu-app.onrender.com/health
```

Sugestões:

- UptimeRobot: monitor HTTP a cada 5 minutos no plano gratuito.
- Better Stack: monitor HTTP gratuito com intervalo mínimo de 3 minutos.
- cron-job.org: cron HTTP gratuito, podendo executar até uma vez por minuto.

Para evitar abuso, use intervalo de 5 minutos. É suficiente para manter tráfego periódico em ambiente de teste.

## Checklist de publicação

1. Subir o código para GitHub.
2. Criar Azure SQL Database.
3. Configurar firewall do Azure SQL.
4. Criar Web Service no Render ou App Service na Azure.
5. Configurar `ConnectionStrings__DefaultConnection`.
6. Configurar `ASPNETCORE_ENVIRONMENT=Production`.
7. Fazer deploy.
8. Acessar `/health`.
9. Acessar `/auth/login`.
10. Criar o primeiro administrador pelo bootstrap secreto, caso o banco ainda não tenha admin.
11. Remover as variáveis de bootstrap imediatamente após o primeiro acesso.

## Observações importantes

- Não suba arquivos da pasta `secrets/` ou `.secrets/` para o GitHub.
- Não use LocalDB em produção ou hospedagem cloud.
- Não conte com filesystem local para uploads em Render Free.
- Para cliente testar o sistema inteiro sem lentidão, a melhor opção é plano pago pequeno ou Azure App Service com Always On.
- Para demonstração inicial, Render Free + Azure SQL Free + UptimeRobot resolve com baixo custo.

## Deploy no Azure App Service com GitHub Actions

Este é o fluxo recomendado para o ambiente atual criado no Azure:

- App Service: `ikkon-admin-demo`
- Runtime: `.NET 10 LTS`
- Publicação: `Code`
- Banco: Azure SQL Database

### 1. Conferir variáveis no App Service

No Azure Portal, acesse:

```text
App Service > ikkon-admin-demo > Settings > Environment variables
```

Em `App settings`, configure:

```text
ASPNETCORE_ENVIRONMENT=Production
GoogleAgenda__RedirectUri=https://ikkon-admin-demo.azurewebsites.net/admin/agenda/google/callback
GoogleAgenda__OAuthClientSecretsPath=
```

Em `Connection strings`, configure:

```text
Name: DefaultConnection
Type: SQLAzure
Value: Server=tcp:tonnyserver.database.windows.net,1433;Initial Catalog=Ikkon_DataBase;Persist Security Info=False;User ID=tonny;Password=<senha>;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### 2. Habilitar publish profile para o deploy simples

O workflow criado usa `azure/webapps-deploy` com publish profile.

Para esse modo funcionar, o App Service precisa permitir credenciais de publicação.

No Azure Portal, procure no App Service por:

```text
Configuration > General settings
```

ou:

```text
Deployment Center / FTPS credentials / Basic authentication publishing credentials
```

Ative a opção de basic publishing credentials caso o download do publish profile esteja bloqueado.

Para um ambiente de teste sem dados reais, isso é aceitável. Em produção, o ideal é trocar para OIDC/federated credentials.

### 3. Baixar publish profile

No App Service, clique em:

```text
Overview > Download publish profile
```

Abra o arquivo baixado em um editor de texto e copie todo o conteúdo XML.

### 4. Criar secret no GitHub

No repositório do GitHub, vá em:

```text
Settings > Secrets and variables > Actions > New repository secret
```

Crie o secret:

```text
Name: AZURE_WEBAPP_PUBLISH_PROFILE
Value: <cole aqui o XML inteiro do publish profile>
```

### 5. Workflow criado no projeto

O arquivo de workflow está em:

```text
.github/workflows/deploy-azure.yml
```

Ele faz:

1. checkout do repositório;
2. setup do .NET 10;
3. restore;
4. build em Release;
5. publish;
6. deploy para o App Service `ikkon-admin-demo`.

### 6. Rodar o deploy

Faça commit e push para `main`, ou rode manualmente em:

```text
GitHub > Actions > Deploy IkkonAdmin to Azure App Service > Run workflow
```

### 7. Validar publicação

Depois do deploy, acesse:

```text
https://ikkon-admin-demo.azurewebsites.net/health
https://ikkon-admin-demo.azurewebsites.net/
https://ikkon-admin-demo.azurewebsites.net/auth/login
```

Se `/health` funcionar e `/auth/login` abrir, o app subiu corretamente.

Se o login falhar por banco, revise:

- connection string no App Service;
- firewall do Azure SQL;
- `Allow Azure services and resources to access this server` no SQL Server;
- senha do usuário SQL;
- logs do App Service.
