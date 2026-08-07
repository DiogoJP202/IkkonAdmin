# Runbook de produção

Este documento é o procedimento operacional do IkkonAdmin em produção. O banco oficial é SQL Server e documentos privados usam storage S3 compatível.

## Arquitetura e responsabilidades

- Aplicação: ASP.NET Core MVC em container ou Azure App Service.
- Banco: SQL Server/Azure SQL, atualizado somente por migration explícita.
- Arquivos públicos: storage público atual de imagens do blog e perfis.
- Documentos de alunos: bucket S3 privado, acessado somente pelo backend.
- Data Protection: key ring em volume persistente; opcionalmente protegido por certificado PFX.
- Fuso das automações: `America/Sao_Paulo`.

O processo web não executa migration nem reparo SQL em Production. Ele verifica migrations pendentes e se recusa a iniciar quando o schema não está atualizado.

## Variáveis obrigatórias

```text
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=<secret SQL Server>

PrivateFileStorage__Provider=S3
PrivateFileStorage__BucketName=<bucket privado>
PrivateFileStorage__Region=<região>
PrivateFileStorage__ServiceUrl=<vazio na AWS ou endpoint S3 compatível>
PrivateFileStorage__ForcePathStyle=false
PrivateFileStorage__AccessKeyId=<secret, quando não houver IAM role>
PrivateFileStorage__SecretAccessKey=<secret, quando não houver IAM role>
PrivateFileStorage__KeyPrefix=documents

DataProtection__KeysPath=/var/ikkon/dataprotection
DataProtection__CertificatePath=<PFX montado como secret, opcional>
DataProtection__CertificatePassword=<secret, opcional>
```

Prefira identidade gerenciada/IAM role a chaves estáticas. Nunca salve connection strings, JSON do Google, chaves S3, PFX ou senhas em `appsettings.json`.

Google Agenda, quando habilitado:

```text
GoogleAgenda__ApplicationName=IkkonAdmin
GoogleAgenda__CalendarId=primary
GoogleAgenda__OAuthClientSecretsPath=<JSON montado como secret>
GoogleAgenda__RedirectUri=https://<dominio>/admin/agenda/google/callback
GoogleAgenda__TimeZone=America/Sao_Paulo
```

## Bootstrap do primeiro administrador

Use apenas se o banco ainda não possuir nenhum administrador:

```text
InitialAdminBootstrap__Login=<login inicial>
InitialAdminBootstrap__Email=<email>
InitialAdminBootstrap__DisplayName=<nome>
InitialAdminBootstrap__Password=<secret com no mínimo 12 caracteres>
```

Após o primeiro startup bem-sucedido, remova as quatro variáveis e rotacione a senha pelo painel. O bootstrap não substitui nem reativa administradores existentes e não cria credenciais demo.

## Bucket privado

Configuração mínima obrigatória:

- bloquear todo acesso público;
- habilitar criptografia do bucket; cada upload também solicita SSE-S3/AES-256;
- habilitar versionamento;
- permitir à identidade da aplicação apenas `GetObject`, `PutObject`, `DeleteObject` e consulta do bucket no prefixo configurado;
- negar transporte sem TLS;
- manter logs de acesso do provedor.

Objetos não possuem URL pública. O banco guarda somente uma chave lógica e o download passa pela autorização MVC.

## Pipeline de deploy

1. Criar backup/snapshot antes de mudanças de schema.
2. Restaurar e compilar a solução em Release.
3. Executar a suíte de testes.
4. Aplicar migrations com credencial de deploy, separada da credencial da aplicação.
5. Publicar a nova versão.
6. Verificar `/health/live` e `/health/ready`.
7. Executar smoke test de login, navegação, download privado e blog PT/EN/JA.

Comandos equivalentes:

```powershell
dotnet restore .\IkkonAdmin.slnx
dotnet build .\IkkonAdmin.slnx -c Release --no-restore
dotnet test .\IkkonAdmin.Tests\IkkonAdmin.Tests.csproj -c Release --no-build
dotnet tool restore
dotnet ef database update --project .\IkkonAdmin.Web --startup-project .\IkkonAdmin.Web --connection "$env:ConnectionStrings__DefaultConnection" -c Release --no-build
dotnet publish .\IkkonAdmin.Web -c Release --no-build -o .\publish
```

Falha em migration interrompe o deploy. Não publique a aplicação nova sobre schema antigo.

## Health checks

- `/health` e `/health/live`: processo web vivo, sem dependências.
- `/health/ready`: SQL Server e storage privado disponíveis.
- `/health/sql`: diagnóstico isolado do banco.
- `/health/storage`: diagnóstico isolado do bucket/diretório privado.

Balanceadores devem usar `live` para reinício e `ready` para retirar a instância do tráfego.

## Rollback

Rollback de aplicação é preferível a rollback destrutivo de schema:

1. interromper novas publicações;
2. retirar a versão defeituosa do tráfego;
3. republicar a imagem anterior;
4. manter migrations aditivas quando a versão anterior for compatível;
5. se a migration for incompatível, restaurar o backup em um banco separado, validar e só então trocar a connection string.

Não execute `database update <migration-antiga>` em produção sem revisar perda de dados no método `Down`.

## Política de backup e retenção

- backup completo criptografado diário: 35 dias;
- backup semanal: 12 semanas;
- backup mensal: 12 meses;
- transaction log: a cada 15 minutos quando o banco estiver em recovery model Full;
- bucket privado com versionamento: versões antigas por 35 dias;
- objeto atual não expira enquanto estiver referenciado no banco;
- teste de restauração trimestral, com evidência de duração, integridade e responsável.

O backup deve estar em conta/região separada quando o provedor permitir. Restrinja exclusão de backups e use MFA/dupla aprovação para alterar retenção.

## Teste trimestral de restauração

1. escolher aleatoriamente um backup diário e um conjunto de objetos versionados;
2. restaurar em ambiente isolado sem acesso público;
3. aplicar validação de integridade do SQL Server;
4. iniciar a versão correspondente da aplicação;
5. validar um aluno, financeiro, frequência e documento privado;
6. registrar RPO observado, RTO, falhas e ações corretivas;
7. destruir o ambiente isolado após aprovação.

## Rotação de secrets

- SQL Server: criar nova credencial, validar readiness, trocar a aplicação e revogar a antiga.
- S3: emitir chave/role nova, validar upload e download, então revogar a anterior.
- Google: atualizar o JSON montado, reiniciar e validar OAuth.
- Bootstrap: remover imediatamente após uso; nunca manter a senha inicial configurada.
- Data Protection: preservar o key ring. Ao trocar o PFX, disponibilizar temporariamente o certificado anterior em `DataProtection__UnprotectCertificatePaths__0` até todos os keys antigos expirarem.

Registre toda rotação sem copiar o valor do secret para tickets ou logs.

## Incidentes de storage

Se o banco referencia um objeto ausente, não altere o caminho físico no banco. Restaure a versão do objeto sob a mesma chave. Se houver suspeita de vazamento, revogue credenciais, bloqueie o bucket, preserve logs e revise auditorias de download antes de reabrir o acesso.
