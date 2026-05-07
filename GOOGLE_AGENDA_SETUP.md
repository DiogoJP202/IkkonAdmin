# Configuração da Integração com Google Agenda

Este guia explica como conectar o Google Agenda ao IkkonAdmin usando OAuth Client Web.

## 1. Pré-requisitos

- Ter acesso ao Google Cloud Console.
- Ter um projeto Google Cloud criado para o sistema.
- Ter a API Google Calendar habilitada.
- Ter um usuário administrador no IkkonAdmin com permissão `GOOGLE_AGENDA_MANAGE`.

## 2. Habilitar Google Calendar API

1. Acesse `https://console.cloud.google.com/`.
2. Selecione o projeto do IkkonAdmin.
3. Vá em `APIs & Services` > `Library`.
4. Pesquise por `Google Calendar API`.
5. Clique em `Enable`.

## 3. Configurar OAuth Consent Screen

1. Vá em `APIs & Services` > `OAuth consent screen`.
2. Escolha o tipo `External`, se for usar contas Google comuns.
3. Preencha os dados básicos do app.
4. Em `Test users`, adicione os e-mails que poderão testar a integração.

Exemplo:

```text
seu-email@gmail.com
```

Enquanto o app estiver em modo `Testing`, somente usuários adicionados nessa lista conseguem conectar.

## 4. Criar OAuth Client

1. Vá em `APIs & Services` > `Credentials`.
2. Clique em `Create Credentials`.
3. Selecione `OAuth client ID`.
4. Tipo da aplicação: `Web application`.
5. Use um nome claro, por exemplo `IkkonAdmin Local`.
6. Em `Authorized redirect URIs`, adicione exatamente:

```text
http://localhost:5037/admin/agenda/google/callback
```

Se o `dotnet run` estiver usando outra porta, troque `5037` pela porta correta.

7. Baixe o JSON do OAuth Client.

O formato esperado é parecido com:

```json
{
  "web": {
    "client_id": "xxxx.apps.googleusercontent.com",
    "project_id": "seu-projeto",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_secret": "seu-client-secret"
  }
}
```

## 5. Salvar o JSON no projeto

Crie a pasta `.secrets` na raiz do repositório e salve o arquivo como:

```text
.secrets/google-oauth-client.json
```

A pasta `.secrets/` está no `.gitignore` e não deve ser commitada.

## 6. Configurar appsettings

No arquivo `IkkonAdmin.Web/appsettings.Development.json`, configure:

```json
"GoogleAgenda": {
  "ApplicationName": "IkkonAdmin",
  "CalendarId": "primary",
  "CredentialsPath": "",
  "OAuthClientSecretsPath": ".secrets/google-oauth-client.json",
  "RedirectUri": "http://localhost:5037/admin/agenda/google/callback",
  "TimeZone": "America/Sao_Paulo"
}
```

### Sobre o CalendarId

Use `primary` para conectar a agenda principal da conta Google autorizada.

Se quiser usar uma agenda específica:

1. Abra Google Calendar.
2. Vá em configurações da agenda.
3. Procure `Integrar agenda`.
4. Copie o `ID da agenda`.
5. Substitua `primary` pelo ID copiado.

Exemplo:

```json
"CalendarId": "id-da-agenda@group.calendar.google.com"
```

## 7. Aplicar migrations

Na raiz do projeto:

```bash
dotnet ef database update --project IkkonAdmin.Web/IkkonAdmin.Web.csproj --startup-project IkkonAdmin.Web/IkkonAdmin.Web.csproj
```

No Windows PowerShell:

```powershell
dotnet ef database update --project .\IkkonAdmin.Web\IkkonAdmin.Web.csproj --startup-project .\IkkonAdmin.Web\IkkonAdmin.Web.csproj
```

## 8. Conectar pelo sistema

1. Rode o sistema:

```powershell
cd .\IkkonAdmin.Web
dotnet run
```

2. Acesse:

```text
http://localhost:5037/auth/login
```

3. Faça login como administrador.
4. Acesse:

```text
http://localhost:5037/admin/agenda
```

5. Clique em `Conectar Google Agenda`.
6. Selecione a conta Google autorizada.
7. Confirme as permissões.
8. O sistema deve voltar para `/admin/agenda`.

Depois disso, a agenda deve permitir:

- listar eventos;
- criar eventos;
- editar eventos;
- excluir eventos, conforme permissões do usuário.

## 9. Permissões necessárias no IkkonAdmin

Para administrar a conexão OAuth:

```text
GOOGLE_AGENDA_MANAGE
```

Para usar a agenda:

```text
GOOGLE_AGENDA_VIEW
GOOGLE_AGENDA_CREATE
GOOGLE_AGENDA_EDIT
GOOGLE_AGENDA_DELETE
```

Administradores têm acesso total automaticamente.

Após alterar permissões de um usuário, faça logout e login novamente para atualizar as claims do cookie.

## 10. Erros comuns

### Access blocked: app has not completed verification

Causa:

- O app está em modo `Testing`.
- O e-mail usado no login Google não está em `Test users`.

Correção:

1. Vá em `OAuth consent screen`.
2. Adicione o e-mail em `Test users`.
3. Salve.
4. Aguarde alguns minutos.
5. Tente novamente em aba anônima.

### Error 400: redirect_uri_mismatch

Causa:

- A URL de callback no Google Cloud não é igual à URL configurada no sistema.

Correção:

Garanta que as duas estejam idênticas:

```text
http://localhost:5037/admin/agenda/google/callback
```

Verifique:

- `Authorized redirect URIs` no Google Cloud.
- `GoogleAgenda:RedirectUri` no `appsettings.Development.json`.

### Não foi possível obter refresh token

Causa comum:

- A conta já autorizou o app antes e o Google não retornou novo refresh token.

Correção:

1. Acesse `https://myaccount.google.com/permissions`.
2. Remova o acesso do app de teste.
3. Volte ao IkkonAdmin.
4. Clique em `Conectar Google Agenda` novamente.

## 11. Cuidados de segurança

- Não commitar arquivos dentro de `.secrets/`.
- Não colar `client_secret` em código fonte.
- Não enviar credenciais reais em prints, issues ou commits.
- Se o `client_secret` vazar, revogue e gere outro no Google Cloud.
