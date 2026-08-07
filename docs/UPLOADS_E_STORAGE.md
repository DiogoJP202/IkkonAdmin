# Uploads e storage

Este documento descreve onde o sistema salva arquivos enviados e quais cuidados são necessários em desenvolvimento, homologação e produção.

## Tipos de arquivo

Atualmente existem três grupos principais:

1. imagens públicas do blog;
2. fotos públicas de perfil;
3. documentos privados enviados por alunos.

Os contratos de infraestrutura são:

```text
Mídia pública: IFileStorageService -> LocalFileStorageService
Documento privado (Development): IPrivateFileStorageService -> LocalPrivateFileStorageService
Documento privado (Production): IPrivateFileStorageService -> S3PrivateFileStorageService
```

Services novos devem depender da interface, não de paths montados manualmente. Documentos privados usam `IPrivateFileStorageService`; arquivos públicos continuam usando `IFileStorageService`.

## Blog

Serviço responsável:

```text
BlogMediaService
```

### Imagem de capa

Pasta:

```text
IkkonAdmin.Web/wwwroot/uploads/blog/capas
```

URL pública:

```text
/uploads/blog/capas/<arquivo>
```

Limite:

```text
3 MB
```

### Imagem de conteúdo

Pasta:

```text
IkkonAdmin.Web/wwwroot/uploads/blog/conteudo
```

URL pública:

```text
/uploads/blog/conteudo/<arquivo>
```

Limite:

```text
2 MB
```

### Formatos aceitos

- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

### Observações de segurança

Arquivos do blog ficam em `wwwroot`, portanto são públicos. Não usar o upload do blog para documentos privados, comprovantes ou qualquer dado sensível.

## Fotos de perfil

Serviço responsável:

```text
UserSettingsService
```

Pasta:

```text
IkkonAdmin.Web/wwwroot/uploads/perfis
```

URL pública:

```text
/uploads/perfis/<arquivo>
```

Limite:

```text
2 MB
```

Formatos aceitos:

- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

Além de extensão, tamanho e arquivo vazio, documentos são validados pela
assinatura binária esperada de PDF, JPEG, PNG ou WebP. O `ContentType` enviado
pelo navegador não é considerado prova do formato.

Fotos de perfil são públicas porque aparecem na interface autenticada e podem ser servidas diretamente pelo app.

## Documentos do aluno

Serviços responsáveis:

```text
AreaAlunoDocumentosService
AreaAlunoDocumentoAdminService
```

Pasta:

```text
IkkonAdmin.Web/App_Data/uploads/documentos
```

Essa implementação local é exclusiva de Development. Em Production, o startup exige `PrivateFileStorage:Provider=S3` e usa `S3PrivateFileStorageService`.

Limite:

```text
10 MB
```

Formatos aceitos:

- `.pdf`
- `.jpg`
- `.jpeg`
- `.png`
- `.webp`

## Por que documentos ficam fora do wwwroot?

Documentos de aluno são privados. Eles não devem ter URL pública direta.

O download passa por controller/service para validar:

- usuário autenticado;
- vínculo com o aluno;
- permissão administrativa, quando for download interno;
- existencia do arquivo.

## Git

Uploads não devem ser versionados.

Se novas pastas de upload forem adicionadas, confirme que estão cobertas por `.gitignore`.

## Desenvolvimento local

Em desenvolvimento, storage local é suficiente:

- imagens do blog em `wwwroot/uploads`;
- fotos de perfil em `wwwroot/uploads/perfis`;
- documentos em `App_Data/uploads/documentos`.

Ao limpar `bin/obj`, não apagar uploads reais do ambiente local sem querer.

## Docker

Em Docker Compose local, use volumes quando quiser preservar uploads entre recriações de container.

Sem volume, arquivos criados dentro do container podem desaparecer ao recriar a imagem/container.

## Hospedagens com filesystem efêmero

Documentos privados não dependem do disco da aplicação em `Production`, pois o
startup exige S3. Imagens públicas do blog e fotos de perfil ainda usam
`wwwroot/uploads`; portanto, em Render e hosts equivalentes, essa pasta precisa
estar em volume persistente e entrar no backup. Sem isso, uploads públicos podem
desaparecer após deploy, recriação ou movimentação do container.

## Azure App Service

Azure App Service possui filesystem persistente no plano da aplicação, mas a
pasta pública de uploads ainda deve entrar no backup e ser validada após cada
estratégia de deploy.

Para documentos privados, a implementação atual suporta Amazon S3, Cloudflare
R2 e outros serviços compatíveis com a API S3. Azure Blob exigiria uma nova
implementação de `IPrivateFileStorageService`.

## Recomendação para produção

Em produção, documentos privados usam storage S3 compatível com:

- bucket sem acesso público;
- criptografia SSE-S3 e versionamento;
- download por stream autenticado, sem URL do objeto exposta ao navegador;
- credencial de privilégio mínimo limitada ao prefixo de documentos;
- validação de extensão e tamanho no backend;
- nomes gerados por GUID;
- preservação do nome original apenas como metadado;
- logs/auditoria para downloads sensíveis.

Consulte [Runbook de produção](./PRODUCTION_RUNBOOK.md) para variáveis, backup, restore e rotação.

## Checklist ao adicionar novo upload

1. Definir se o arquivo é público ou privado.
2. Definir extensões aceitas.
3. Definir limite de tamanho.
4. Salvar com nome gerado pelo sistema.
5. Guardar nome original como metadado quando necessário.
6. Validar autorização no download.
7. Garantir que a pasta não será commitada.
8. Documentar a pasta e a política de retenção.
9. Para formato sensível, validar assinatura binária e não confiar no MIME do cliente.
10. Definir se a política exige varredura antivírus; essa integração ainda não está implementada.
