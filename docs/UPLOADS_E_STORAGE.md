# Uploads e storage

Este documento descreve onde o sistema salva arquivos enviados e quais cuidados são necessários em desenvolvimento, homologação e produção.

## Tipos de arquivo

Atualmente existem três grupos principais:

1. imagens públicas do blog;
2. fotos públicas de perfil;
3. documentos privados enviados por alunos.

O padrão de infraestrutura para novos uploads é:

```text
IFileStorageService
LocalFileStorageService
```

Services novos devem depender da interface, não de paths montados manualmente.

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

## Render Free

O filesystem do Render Free é efêmero. Isso significa que uploads locais não são armazenamento confiável.

Para demonstrações simples, pode funcionar temporariamente. Para uso real, use storage persistente externo.

## Azure App Service

Azure App Service possui filesystem persistente no plano da aplicação, mas ainda assim é recomendável avaliar storage externo para arquivos sensíveis ou crescimento de volume.

Opções recomendadas:

- Azure Blob Storage;
- Amazon S3;
- Cloudflare R2;
- outro storage de objetos compatível.

## Recomendação para produção

Para produção, migrar uploads para storage externo com:

- URLs públicas controladas para imagens do blog;
- URLs privadas ou assinadas para documentos de aluno;
- validação de extensão e tamanho no backend;
- nomes gerados por GUID;
- preservação do nome original apenas como metadado;
- logs/auditoria para downloads sensíveis.

## Checklist ao adicionar novo upload

1. Definir se o arquivo é público ou privado.
2. Definir extensões aceitas.
3. Definir limite de tamanho.
4. Salvar com nome gerado pelo sistema.
5. Guardar nome original como metadado quando necessário.
6. Validar autorização no download.
7. Garantir que a pasta não será commitada.
8. Documentar a pasta e a política de retenção.
