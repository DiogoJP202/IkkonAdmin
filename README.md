# IkkonAdmin

<p align="center">
  <img src="./IkkonAdmin.Web/wwwroot/Images/Ikkon_Icon.png" alt="Logo IKKON SPTD" width="120" />
</p>

<h3 align="center">Sistema administrativo, portal do aluno, blog e site institucional para o IKKON SPTD | São Paulo Taiko Dojo</h3>

<p align="center">
  <strong>ASP.NET Core MVC · Entity Framework Core · SQL Server · Razor Views · Bootstrap 5</strong>
</p>

<p align="center">
  <a href="#visão-geral">Visão geral</a> ·
  <a href="#módulos">Módulos</a> ·
  <a href="#arquitetura">Arquitetura</a> ·
  <a href="#rodando-localmente">Rodar local</a> ·
  <a href="#documentação">Documentação</a>
</p>

---

## Visão geral

O **IkkonAdmin** é uma aplicação web criada para apoiar a operação do **IKKON SPTD | Ikkon São Paulo Taiko Dojo**, escola de taiko em São Paulo dedicada ao ensino de percussão japonesa, fue, teoria musical e apresentações culturais.

O sistema centraliza rotinas que antes dependiam de planilhas, Notion, WhatsApp e controles financeiros separados:

- cadastro e acompanhamento de alunos;
- turmas, aulas, frequência e instrutores;
- financeiro, mensalidades, pagamentos, atrasos e acordos;
- admissões, desligamentos e graduações;
- inventário de instrumentos e equipamentos;
- agenda operacional com Google Agenda;
- portal do aluno;
- comunicados, documentos, eventos e conquistas;
- blog público com versões por idioma;
- usuários, cargos, permissões e auditoria.

Este projeto nasceu como uma iniciativa voluntária para organizar processos internos da escola e criar uma base técnica evolutiva para os próximos anos.

## Módulos

### Site institucional

Rotas públicas:

- `/` - apresentação institucional.
- `/escola` - aulas, cursos e experiência do aluno.
- `/eventos` - grupo artístico, apresentações e eventos.
- `/blog` - publicações, novidades e bastidores.

O site possui suporte de interface para português, inglês e japonês nas áreas públicas definidas, com seletor de idioma por cookie.

### Painel administrativo

O painel interno fica sob `/admin` e usa autenticação por cookie, roles e permissões granulares. Ele cobre:

- dashboard operacional;
- alunos, turmas, financeiro, admissões, desligamentos e graduações;
- inventário;
- agenda;
- blog;
- administração de usuários, cargos, permissões, sistema e auditoria;
- manutenção operacional da Área do Aluno.

### Área do Aluno

O aluno acessa por `/aluno/login` e usa o portal em `/area-do-aluno`. O portal consulta dados pelo usuário autenticado, usando o vínculo `UsuarioSistema.AlunoId`, sem receber `AlunoId` sensível por rota.

Funcionalidades principais:

- dashboard do aluno;
- perfil;
- financeiro;
- turmas, aulas e horários;
- frequência;
- documentos;
- comunicados;
- eventos;
- conquistas.

### Blog

O blog possui painel editorial em `/admin/blog` e área pública em `/blog`. Recursos atuais:

- editor rico com texto, imagem e vídeo do YouTube;
- imagem de capa;
- categorias e tags;
- rascunho, agendamento, publicação e arquivamento;
- destaque e blog da semana;
- versões por idioma: `pt-BR`, `en-US` e `ja-JP`;
- SEO básico por post.

## Arquitetura

O projeto segue MVC server-side em uma solução ASP.NET Core:

```text
IkkonAdmin
├── IkkonAdmin.Web
│   ├── Controllers
│   ├── Data
│   │   ├── Configurations
│   │   └── Migrations
│   ├── Enums
│   ├── Models
│   │   ├── Entities
│   │   └── ViewModels
│   ├── Security
│   ├── Services
│   ├── Views
│   └── wwwroot
├── docs
├── DEPLOYMENT.md
├── GOOGLE_AGENDA_SETUP.md
├── USUARIOS_ACESSO.md
└── README.md
```

Stack principal:

- **.NET 10 / ASP.NET Core MVC**.
- **Razor Views** para renderização server-side.
- **Entity Framework Core 10** com SQL Server.
- **Bootstrap 5** e CSS customizado.
- **Data Protection** para tokens sensíveis.
- **Google Calendar API** via `HttpClient`.

Princípios usados:

- regras de negócio concentradas em services;
- ViewModels por tela;
- Entity Framework Core com configurations por entidade;
- policies e claims para autorização granular;
- separação clara entre site público, painel administrativo e portal do aluno;
- migrations versionadas.

## Rodando localmente

### Pré-requisitos

- .NET SDK compatível com `net10.0`.
- SQL Server ou SQL Server Express LocalDB.
- Git.
- Opcional: `dotnet-ef`.

### Restaurar pacotes

```bash
dotnet restore IkkonAdmin.Web/IkkonAdmin.Web.csproj
```

### Configurar banco

Arquivo:

```text
IkkonAdmin.Web/appsettings.json
```

Exemplo:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=IkkonAdminDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### Aplicar migrations

```bash
dotnet ef database update --project IkkonAdmin.Web/IkkonAdmin.Web.csproj --startup-project IkkonAdmin.Web/IkkonAdmin.Web.csproj
```

### Rodar aplicação

```bash
dotnet run --project IkkonAdmin.Web/IkkonAdmin.Web.csproj
```

URL comum em desenvolvimento:

```text
http://localhost:5037
```

## Docker Compose

O projeto inclui `compose.yaml` para subir app e SQL Server local em containers:

```bash
docker compose up --build
```

Aplicação:

```text
http://localhost:8080
```

SQL Server local:

```text
localhost,14333
```

Credenciais padrão do container de desenvolvimento:

```text
User ID: sa
Password: IkkonLocal!2026
Database: IkkonAdminDb
```

## Usuários de demonstração

As credenciais de desenvolvimento estão em [USUARIOS_ACESSO.md](./USUARIOS_ACESSO.md).

Resumo:

| Perfil | Login | Senha | Destino |
|---|---|---|---|
| Admin | `funcionario.admin` | `Ikkon@123` | `/admin/painel` |
| Funcionário | `funcionario.operacional` | `Func@123` | `/admin` |
| Aluno | `aluno.demo` | `Aluno@123` | `/area-do-aluno` |

> Credenciais apenas para desenvolvimento e demonstração. Em produção, altere todas as senhas.

## Documentação

Documentação técnica e funcional separada:

- [Arquitetura e convenções](./docs/ARQUITETURA.md)
- [Área do Aluno](./docs/AREA_DO_ALUNO.md)
- [Blog e idiomas](./docs/BLOG_E_IDIOMAS.md)
- [Módulos administrativos e permissões](./docs/MODULOS_ADMINISTRATIVOS.md)
- [Uploads e storage](./docs/UPLOADS_E_STORAGE.md)
- [Deploy](./DEPLOYMENT.md)
- [Google Agenda](./GOOGLE_AGENDA_SETUP.md)
- [Usuários de acesso](./USUARIOS_ACESSO.md)

## Deploy

O guia de publicação fica em [DEPLOYMENT.md](./DEPLOYMENT.md). O endpoint de healthcheck disponível é:

```text
/health
```

Para ambientes com uploads reais, leia também [Uploads e storage](./docs/UPLOADS_E_STORAGE.md).

## Segurança

Cuidados já considerados:

- autenticação separada para administração e aluno;
- autorização por roles, claims e policies;
- validação server-side de ações protegidas;
- query filter para usuários excluídos;
- área do aluno sempre resolvida pelo usuário autenticado;
- dados financeiros e documentos protegidos por permissão e vínculo;
- tokens do Google protegidos com Data Protection;
- documentos de aluno armazenados fora de `wwwroot`;
- secrets e uploads ignorados no Git.

## Licença e observação

Este projeto foi criado como uma iniciativa voluntária para apoiar a organização administrativa do IKKON SPTD.

O código e os assets institucionais devem respeitar a autorização da escola antes de qualquer uso externo, redistribuição ou publicação comercial.
