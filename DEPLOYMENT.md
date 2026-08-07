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

## Opção B - Render com Docker

Use:

- App: Render Web Service `starter` com Docker
- Banco: Azure SQL Database Free Offer
- Documentos privados: bucket S3 compatível
- Health check: `/health/ready`

Vantagens:

- Deploy simples via GitHub.
- Render consegue rodar o app via Docker.
- Azure SQL mantém compatibilidade com Entity Framework Core + SQL Server.

Limitações:

- O blueprint atual usa plano `starter`, migration bundle no pre-deploy e disco
  persistente para o key ring de Data Protection.
- Documentos privados não usam o disco do container; o startup de produção exige S3.
- Imagens públicas do blog e perfis ainda usam `wwwroot/uploads`. Em um host com
  filesystem efêmero, persistir essa pasta ou não permitir uploads públicos até
  existir um provider externo para mídia pública.

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

PrivateFileStorage__Provider=S3
PrivateFileStorage__BucketName=<bucket-privado>
PrivateFileStorage__Region=<região>
PrivateFileStorage__ServiceUrl=<endpoint-opcional-S3-compatível>
PrivateFileStorage__AccessKeyId=<secret-ou-vazio-com-IAM-role>
PrivateFileStorage__SecretAccessKey=<secret-ou-vazio-com-IAM-role>

DataProtection__KeysPath=/var/ikkon/dataprotection
```

`AccessKeyId` e `SecretAccessKey` devem ser fornecidos juntos. O bucket precisa
bloquear acesso público, usar criptografia, versionamento e credencial limitada
ao prefixo de documentos.

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

O sistema trata arquivos em duas áreas diferentes:

- imagens públicas do blog e perfis em `wwwroot/uploads`;
- documentos privados pelo `IPrivateFileStorageService`.

Em `Development`, documentos privados ficam em `App_Data/uploads/documentos`.
Em `Production`, o startup recusa provider local e exige S3 compatível.

Amazon S3, Cloudflare R2 e outros serviços compatíveis com a API S3 são
suportados para documentos. Azure Blob não possui implementação no projeto atual.

Documentos de aluno não devem ser movidos para uma pasta pública sem controle de autorização. Eles precisam continuar passando por download autenticado/autorizado.

O storage público de blog/perfil ainda é local. Em Render ou outro filesystem
efêmero, essa pasta precisa de volume persistente e backup; caso contrário, os
arquivos podem desaparecer em um novo deploy.

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

## Monitoramento externo

Configure o monitor de processo em:

```text
https://seu-app.onrender.com/health/live
```

Configure também um monitor de dependências em:

```text
https://seu-app.onrender.com/health/ready
```

`live` indica que o processo está ativo; `ready` falha quando SQL Server ou
storage privado não estão disponíveis. Monitoramento não deve ser usado para
contornar hibernação de plano gratuito.

## Checklist de publicação

1. Subir o código para GitHub.
2. Criar Azure SQL Database.
3. Configurar firewall do Azure SQL.
4. Criar Web Service no Render ou App Service na Azure.
5. Configurar `ConnectionStrings__DefaultConnection`.
6. Configurar `ASPNETCORE_ENVIRONMENT=Production`.
7. Configurar S3 privado e Data Protection persistente.
8. Configurar o bootstrap secreto somente se ainda não houver administrador.
9. Executar o deploy; a migration deve ocorrer antes da publicação.
10. Validar `/health/live` e `/health/ready`.
11. Testar `/auth/login`, blog PT/EN/JA e um upload/download privado.
12. Remover as variáveis de bootstrap imediatamente após o primeiro acesso.

## Observações importantes

- Não suba arquivos da pasta `secrets/` ou `.secrets/` para o GitHub.
- Não use LocalDB em produção ou hospedagem cloud.
- Não use provider local para documentos em produção; o startup bloqueia essa configuração.
- Não conte com filesystem efêmero para mídia pública em `wwwroot/uploads`.
- Para cliente testar o sistema inteiro sem lentidão, a melhor opção é plano pago pequeno ou Azure App Service com Always On.
- O `render.yaml` versionado usa plano `starter`, S3 privado e migration bundle no pre-deploy.

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
PrivateFileStorage__Provider=S3
PrivateFileStorage__BucketName=<bucket-privado>
PrivateFileStorage__Region=<região>
PrivateFileStorage__ServiceUrl=<endpoint-opcional>
PrivateFileStorage__AccessKeyId=<secret-ou-vazio-com-role>
PrivateFileStorage__SecretAccessKey=<secret-ou-vazio-com-role>
DataProtection__KeysPath=/home/data-protection
```

O exemplo de `DataProtection__KeysPath` é para App Service Linux. Em Windows,
use um diretório persistente sob `D:\home`. Se a aplicação usar PFX para proteger
o key ring, configure também caminho e senha conforme o runbook.

Em `Connection strings`, configure:

```text
Name: DefaultConnection
Type: SQLAzure
Value: Server=tcp:<servidor>.database.windows.net,1433;Initial Catalog=<banco>;Persist Security Info=False;User ID=<usuario>;Password=<senha>;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
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
5. testes automatizados;
6. aplicação explícita das migrations usando `PRODUCTION_DB_CONNECTION_STRING`;
7. publish;
8. deploy para o App Service `ikkon-admin-demo`.

### 6. Rodar o deploy

Faça commit e push para `main`, ou rode manualmente em:

```text
GitHub > Actions > Deploy IkkonAdmin to Azure App Service > Run workflow
```

### 7. Validar publicação

Depois do deploy, acesse:

```text
https://ikkon-admin-demo.azurewebsites.net/health/live
https://ikkon-admin-demo.azurewebsites.net/health/ready
https://ikkon-admin-demo.azurewebsites.net/
https://ikkon-admin-demo.azurewebsites.net/auth/login
```

O deploy só está saudável quando `live` e `ready` respondem com sucesso e o
smoke test autenticado funciona.

Se o login falhar por banco, revise:

- connection string no App Service;
- firewall do Azure SQL;
- `Allow Azure services and resources to access this server` no SQL Server;
- senha do usuário SQL;
- logs do App Service.
