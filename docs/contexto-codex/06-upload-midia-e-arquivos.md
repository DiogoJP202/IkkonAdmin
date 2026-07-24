# Upload, mídia e arquivos

## Estado atual

Uploads reais no projeto:

- foto de perfil do usuário;
- imagens públicas do blog;
- documentos privados do aluno.

Arquivos estáticos institucionais ficam em:

- `IkkonAdmin.Web/wwwroot/Images`.

Uploads de perfil são salvos em:

- `IkkonAdmin.Web/wwwroot/uploads/perfis`.

Imagens do blog são salvas em:

- `IkkonAdmin.Web/wwwroot/uploads/blog/capas`;
- `IkkonAdmin.Web/wwwroot/uploads/blog/conteudo`.

Documentos do aluno são salvos fora do `wwwroot`:

- `IkkonAdmin.Web/App_Data/uploads/documentos`.

O caminho público salvo no banco segue o formato:

- `/uploads/perfis/{fileName}`.

## Implementação atual de upload

Abstração de infraestrutura:

- `IFileStorageService`;
- `LocalFileStorageService`.

Services responsáveis:

- `UserSettingsService`;
- `BlogMediaService`;
- `AreaAlunoDocumentosService`;
- `AreaAlunoDocumentoAdminService`.

Entradas comuns:

- `IFormFile FotoPerfil` em configurações de conta;
- `IFormFile` de capa e imagem de conteúdo no blog;
- `IFormFile` de documento solicitado no portal do aluno.

Validações:

- extensões permitidas por módulo;
- tamanho máximo por módulo;
- nome gerado pelo sistema;
- pasta controlada por `IFileStorageService`;
- exclusão do arquivo anterior somente quando o caminho pertence ao prefixo esperado.

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

- Não existe biblioteca de imagens ou media manager.
- Não foi identificado processamento de imagem, thumbnail ou compressão.
- Não foi identificado antivírus ou verificação MIME profunda.
- Não há upload direto de vídeo.
- Não há armazenamento externo, como Azure Blob ou S3, identificado no projeto atual.

## Cuidados de segurança

- Validar extensão e tamanho no backend.
- Não confiar apenas em `accept` do input.
- Gerar nome seguro; nunca usar nome original diretamente.
- Salvar arquivos públicos em `wwwroot/uploads/...`.
- Salvar arquivos privados fora de `wwwroot`, como `App_Data/uploads/documentos`.
- Salvar no banco apenas URL pública relativa, não caminho físico.
- Não permitir sobrescrita de arquivos existentes.
- Evitar renderizar uploads como HTML executável.
- Para imagens em conteúdo rico, validar também tipo e tamanho.
- Para vídeos, preferir embed de YouTube com validação de URL/ID.

## Padrão recomendado para novos módulos

Para um novo módulo com upload:

1. Definir se o arquivo é público ou privado.
2. Usar `IFormFile`.
3. Validar extensão permitida.
4. Definir limite de tamanho explícito.
5. Gerar nome com `Guid`.
6. Salvar com `IFileStorageService`.
7. Persistir URL pública relativa apenas quando o arquivo for público.
8. Apagar arquivo anterior somente se ele pertencer ao diretório esperado.
9. Não implementar upload de vídeo local sem uma estratégia dedicada.

