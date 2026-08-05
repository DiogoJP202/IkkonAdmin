using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests.Integration;

public sealed class StudentResourceIsolationIntegrationTests
{
    [Fact]
    public async Task Aluno_ConsultaSomenteDadosPropriosEBaixaSomenteDocumentoProprio()
    {
        await using var factory = new IkkonWebApplicationFactory();
        var ids = new StudentScenarioIds();
        await factory.SeedAsync(async dbContext =>
        {
            var alunoA = CreateStudent("Aluno A Integração", "11111111111");
            var alunoB = CreateStudent("Aluno B Sigiloso", "22222222222");
            dbContext.Alunos.AddRange(alunoA, alunoB);
            await dbContext.SaveChangesAsync();

            var usuarioA = CreateStudentUser(alunoA, "aluno.a");
            var usuarioB = CreateStudentUser(alunoB, "aluno.b");
            dbContext.UsuariosSistema.AddRange(usuarioA, usuarioB);
            dbContext.Mensalidades.AddRange(
                new Mensalidade
                {
                    Aluno = alunoA,
                    Competencia = new DateOnly(2026, 7, 1),
                    DataVencimento = new DateOnly(2026, 7, 10),
                    ValorBase = 123.45m,
                    ValorFinal = 123.45m,
                    Status = StatusMensalidadeEnum.Pendente
                },
                new Mensalidade
                {
                    Aluno = alunoB,
                    Competencia = new DateOnly(2026, 7, 1),
                    DataVencimento = new DateOnly(2026, 7, 10),
                    ValorBase = 987.65m,
                    ValorFinal = 987.65m,
                    Status = StatusMensalidadeEnum.Pendente
                });

            var tipo = new DocumentoTipo { Nome = "Documento privado", Ativo = true };
            var solicitacaoA = new DocumentoSolicitacao
            {
                Aluno = alunoA,
                DocumentoTipo = tipo,
                Status = DocumentoStatusEnum.Enviado
            };
            var solicitacaoB = new DocumentoSolicitacao
            {
                Aluno = alunoB,
                DocumentoTipo = tipo,
                Status = DocumentoStatusEnum.Enviado
            };
            dbContext.AddRange(solicitacaoA, solicitacaoB);
            await dbContext.SaveChangesAsync();

            var envioA = new DocumentoEnvio
            {
                DocumentoSolicitacaoId = solicitacaoA.Id,
                ArquivoUrl = "students/a/document.pdf",
                NomeArquivoOriginal = "documento-a.pdf",
                ContentType = "application/pdf",
                TamanhoBytes = 5
            };
            var envioB = new DocumentoEnvio
            {
                DocumentoSolicitacaoId = solicitacaoB.Id,
                ArquivoUrl = "students/b/document.pdf",
                NomeArquivoOriginal = "documento-b.pdf",
                ContentType = "application/pdf",
                TamanhoBytes = 5
            };
            dbContext.DocumentoEnvios.AddRange(envioA, envioB);
            await dbContext.SaveChangesAsync();

            ids.UserA = usuarioA.Id;
            ids.UploadA = envioA.Id;
            ids.UploadB = envioB.Id;
        });

        await factory.PrivateFileStorage.SaveAsync(
            "students/a/document.pdf",
            new MemoryStream("%PDF-A"u8.ToArray()));
        await factory.PrivateFileStorage.SaveAsync(
            "students/b/document.pdf",
            new MemoryStream("%PDF-B"u8.ToArray()));

        using var client = factory.CreateAuthenticatedClient(ids.UserA, [AppRoles.Aluno]);

        var profileHtml = WebUtility.HtmlDecode(await client.GetStringAsync("/area-do-aluno/perfil"));
        Assert.Contains("Aluno A Integração", profileHtml);
        Assert.DoesNotContain("Aluno B Sigiloso", profileHtml);

        var financeHtml = WebUtility.HtmlDecode(await client.GetStringAsync("/area-do-aluno/financeiro"));
        Assert.Contains("123,45", financeHtml);
        Assert.DoesNotContain("987,65", financeHtml);

        using var deniedDownload = await client.GetAsync($"/area-do-aluno/baixardocumento?envioId={ids.UploadB}");
        Assert.Equal(HttpStatusCode.NotFound, deniedDownload.StatusCode);

        using var ownDownload = await client.GetAsync($"/area-do-aluno/baixardocumento?envioId={ids.UploadA}");
        Assert.Equal(HttpStatusCode.OK, ownDownload.StatusCode);
        Assert.True(ownDownload.Headers.CacheControl?.NoStore);
        Assert.True(ownDownload.Headers.CacheControl?.NoCache);
        Assert.True(ownDownload.Headers.CacheControl?.MustRevalidate);
        Assert.Equal("nosniff", ownDownload.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("%PDF-A", await ownDownload.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UploadComAssinaturaForjada_ERejeitadoNoFluxoHttp()
    {
        await using var factory = new IkkonWebApplicationFactory();
        var ids = new StudentScenarioIds();
        await factory.SeedAsync(async dbContext =>
        {
            var aluno = CreateStudent("Aluno Upload", "33333333333");
            var usuario = CreateStudentUser(aluno, "aluno.upload");
            var solicitacao = new DocumentoSolicitacao
            {
                Aluno = aluno,
                DocumentoTipo = new DocumentoTipo { Nome = "Atestado", Ativo = true },
                Status = DocumentoStatusEnum.Solicitado
            };
            dbContext.AddRange(usuario, solicitacao);
            await dbContext.SaveChangesAsync();
            ids.UserA = usuario.Id;
            ids.RequestA = solicitacao.Id;
        });

        using var client = factory.CreateAuthenticatedClient(ids.UserA, [AppRoles.Aluno]);
        var formHtml = await client.GetStringAsync("/area-do-aluno/documentos");
        var antiforgeryToken = ExtractAntiforgeryToken(formHtml);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(ids.RequestA.ToString()), "solicitacaoId");
        content.Add(new StringContent(antiforgeryToken), "__RequestVerificationToken");
        var forgedPdf = new ByteArrayContent("isto nao e um pdf"u8.ToArray());
        forgedPdf.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf");
        content.Add(forgedPdf, "arquivo", "atestado.pdf");

        using var response = await client.PostAsync("/area-do-aluno/enviardocumento", content);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var uploads = await factory.ExecuteDbAsync(dbContext => dbContext.DocumentoEnvios.CountAsync());
        Assert.Equal(0, uploads);
    }

    [Fact]
    public async Task Administracao_AprovaERecusaDocumentoComAuditoria()
    {
        await using var factory = new IkkonWebApplicationFactory();
        var requestId = 0;
        await factory.SeedAsync(async dbContext =>
        {
            var aluno = CreateStudent("Aluno Avaliação", "44444444444");
            var request = new DocumentoSolicitacao
            {
                Aluno = aluno,
                DocumentoTipo = new DocumentoTipo { Nome = "Comprovante", Ativo = true },
                Status = DocumentoStatusEnum.Enviado
            };
            dbContext.DocumentoSolicitacoes.Add(request);
            await dbContext.SaveChangesAsync();
            requestId = request.Id;
        });

        using var client = factory.CreateAuthenticatedClient(
            900,
            [AppRoles.Funcionario],
            [AppPermissions.DocumentosView, AppPermissions.DocumentosApprove]);
        var pageHtml = await client.GetStringAsync("/admin/area-aluno/documentos");
        var antiforgeryToken = ExtractAntiforgeryToken(pageHtml);

        using var approved = await client.PostAsync(
            "/admin/area-aluno/documentos/avaliar",
            CreateEvaluationForm(requestId, DocumentoStatusEnum.Aprovado, "Conferido", antiforgeryToken));
        Assert.Equal(HttpStatusCode.Redirect, approved.StatusCode);

        using var rejected = await client.PostAsync(
            "/admin/area-aluno/documentos/avaliar",
            CreateEvaluationForm(requestId, DocumentoStatusEnum.Recusado, "Documento ilegível", antiforgeryToken));
        Assert.Equal(HttpStatusCode.Redirect, rejected.StatusCode);

        var result = await factory.ExecuteDbAsync(async dbContext => new
        {
            Status = await dbContext.DocumentoSolicitacoes
                .Where(x => x.Id == requestId)
                .Select(x => x.Status)
                .SingleAsync(),
            AuditActions = await dbContext.AuditoriaLogs
                .Where(x => x.EntidadeId == requestId.ToString())
                .Select(x => x.Acao)
                .ToListAsync()
        });
        Assert.Equal(DocumentoStatusEnum.Recusado, result.Status);
        Assert.Contains("DOCUMENTO_APROVADO", result.AuditActions);
        Assert.Contains("DOCUMENTO_RECUSADO", result.AuditActions);
    }

    private static FormUrlEncodedContent CreateEvaluationForm(
        int requestId,
        DocumentoStatusEnum status,
        string observation,
        string antiforgeryToken)
    {
        return new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["SolicitacaoId"] = requestId.ToString(),
            ["Status"] = status.ToString(),
            ["ObservacaoAdministrativa"] = observation,
            ["__RequestVerificationToken"] = antiforgeryToken
        });
    }

    private static string ExtractAntiforgeryToken(string html)
    {
        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
            RegexOptions.CultureInvariant);
        Assert.True(match.Success, "Token antiforgery não encontrado no formulário.");
        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    private static Aluno CreateStudent(string name, string cpf)
    {
        return new Aluno
        {
            NomeCompleto = name,
            CPF = cpf,
            DataEntrada = new DateOnly(2025, 1, 1),
            Status = StatusAlunoEnum.Ativo
        };
    }

    private static UsuarioSistema CreateStudentUser(Aluno student, string login)
    {
        return new UsuarioSistema
        {
            Aluno = student,
            Login = login,
            LoginNormalizado = login.ToUpperInvariant(),
            NomeExibicao = student.NomeCompleto,
            SenhaHash = "integration-test",
            TipoAcesso = TipoAcessoEnum.Aluno,
            Ativo = true
        };
    }

    private sealed class StudentScenarioIds
    {
        public int UserA { get; set; }
        public int RequestA { get; set; }
        public int UploadA { get; set; }
        public int UploadB { get; set; }
    }
}
