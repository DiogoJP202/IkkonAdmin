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

Em `Development`, documentos do aluno são salvos fora do `wwwroot`:

- `IkkonAdmin.Web/App_Data/uploads/documentos`.

Em `Production`, documentos usam bucket privado S3 compatível. O banco guarda
somente a chave lógica do objeto; o navegador nunca recebe caminho físico nem
URL pública do bucket.

O caminho público salvo no banco segue o formato:

- `/uploads/perfis/{fileName}`.

## Implementação atual de upload

Abstrações de infraestrutura:

- mídia pública: `IFileStorageService` e `LocalFileStorageService`;
- documentos privados: `IPrivateFileStorageService`;
- implementação local: `LocalPrivateFileStorageService`;
- implementação de produção: `S3PrivateFileStorageService`;
- validação de documentos: `IDocumentFileValidator` e `DocumentFileValidator`.

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
- arquivo não vazio;
- assinatura binária de PDF, JPEG, PNG e WebP para documentos privados;
- MIME canônico determinado pelo backend, sem confiar no `ContentType` do cliente;
- nome gerado pelo sistema;
- raiz/chave controlada pelo service de storage correspondente;
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
- Não há varredura antivírus externa; a extensão futura deve ocorrer antes de o
  objeto ficar disponível para download.
- Não há upload direto de vídeo.
- Mídia pública de blog/perfil continua em `wwwroot/uploads`; hosts com
  filesystem efêmero precisam persistir e incluir essa pasta no backup.
- O provider privado implementado é S3 compatível; Azure Blob exigiria uma nova
  implementação de `IPrivateFileStorageService`.

## Cuidados de segurança

- Validar extensão e tamanho no backend.
- Para documentos privados, validar a assinatura binária real.
- Não confiar apenas em `accept` do input.
- Não confiar no `ContentType` enviado pelo navegador.
- Gerar nome seguro; nunca usar nome original diretamente.
- Salvar arquivos públicos em `wwwroot/uploads/...`.
- Salvar arquivos privados fora de `wwwroot` em Development e em bucket privado
  S3 em Production.
- Salvar no banco URL pública relativa somente para mídia pública; para arquivos
  privados, guardar apenas chave lógica.
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
6. Usar `IFileStorageService` para mídia pública ou
   `IPrivateFileStorageService` para conteúdo privado.
7. Validar assinatura binária quando o formato for sensível.
8. Persistir URL pública relativa apenas quando o arquivo for público; persistir
   chave lógica quando for privado.
9. Apagar arquivo anterior somente se ele pertencer ao diretório/prefixo esperado.
10. Definir retenção, backup, auditoria e necessidade de antivírus.
11. Não implementar upload de vídeo local sem uma estratégia dedicada.

