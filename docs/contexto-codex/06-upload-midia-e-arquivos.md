# Upload, mídia e arquivos

## Estado atual

O upload identificado no projeto atual é o upload de foto de perfil do usuário em `UserSettingsService`.

Arquivos estáticos institucionais ficam em:

- `IkkonAdmin.Web/wwwroot/Images`.

Uploads de perfil são salvos em:

- `IkkonAdmin.Web/wwwroot/uploads/perfis`.

O caminho público salvo no banco segue o formato:

- `/uploads/perfis/{fileName}`.

## Implementação atual de upload

Arquivo:

- `IkkonAdmin.Web/Services/UserSettingsService.cs`.

Entrada:

- `IFormFile FotoPerfil` em `UpdateAccountInfoRequest`.

Validações:

- Extensões permitidas: `.jpg`, `.jpeg`, `.png`, `.webp`.
- Tamanho máximo: 2 MB.
- Nome gerado com `{user.Id}-{Guid:N}{extension}`.
- Usa `IWebHostEnvironment.WebRootPath`.
- Cria diretório com `Directory.CreateDirectory`.
- Apaga foto anterior quando o caminho começa com `/uploads/perfis/`.

## Mídia pública atual

Imagens usadas pelo site institucional:

- `Ikkon_Icon.png`.
- `FotoAlunos.jpg`.
- `AulaTaiko.png`.
- `Apresentação1.jpg`.
- `Apresentação2_LargeWidth.jpg`.
- `Apresentação3.jpg`.
- `Alunos2.jfif`.

Vídeos públicos são incorporados por YouTube nas views/partials públicos. Upload direto de vídeo não foi identificado.

## Google Agenda e arquivos de credenciais

`GoogleAgendaService` lê credenciais via `GoogleAgendaOptions`.

Configurações relacionadas:

- `GoogleAgenda:CredentialsPath`.
- `GoogleAgenda:OAuthClientSecretsPath`.
- `GoogleAgenda:CalendarId`.
- `GoogleAgenda:RedirectUri`.
- `GoogleAgenda:TimeZone`.

Credenciais reais não devem ser commitadas.

## Limitações atuais

- Não existe serviço genérico de mídia.
- Não existe biblioteca de imagens ou media manager.
- Não foi identificado processamento de imagem, thumbnail ou compressão.
- Não foi identificado antivírus ou verificação MIME profunda.
- Não há upload direto de vídeo.
- Não há armazenamento externo, como Azure Blob ou S3, identificado no projeto atual.

## Cuidados de segurança

- Validar extensão e tamanho no backend.
- Não confiar apenas em `accept` do input.
- Gerar nome seguro; nunca usar nome original diretamente.
- Salvar apenas dentro de `wwwroot/uploads/...`.
- Salvar no banco apenas URL pública relativa, não caminho físico.
- Não permitir sobrescrita de arquivos existentes.
- Evitar renderizar uploads como HTML executável.
- Para imagens em conteúdo rico, validar também tipo e tamanho.
- Para vídeos, preferir embed de YouTube com validação de URL/ID.

## Padrão recomendado para novos módulos

Para um novo módulo com upload:

1. Criar pasta específica em `wwwroot/uploads/{modulo}`.
2. Usar `IFormFile`.
3. Validar extensão permitida.
4. Definir limite de tamanho explícito.
5. Gerar nome com `Guid`.
6. Salvar com `FileStream`.
7. Persistir URL pública relativa.
8. Apagar arquivo anterior somente se ele pertencer ao diretório esperado.
9. Não implementar upload de vídeo local sem uma estratégia dedicada.

