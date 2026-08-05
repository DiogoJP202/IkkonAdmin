using System.Net;
using System.Text.RegularExpressions;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;

namespace IkkonAdmin.Tests.Integration;

public sealed class InstructorScopeIntegrationTests
{
    [Fact]
    public async Task Instrutor_NaoListaAbreOuAlteraAulaDeOutroInstrutor_MasAdminAcessaTudo()
    {
        await using var factory = new IkkonWebApplicationFactory();
        var ids = new InstructorScenarioIds();
        await factory.SeedAsync(async dbContext =>
        {
            var instructorA = CreateEmployee("instrutor.a", "Instrutor A");
            var instructorB = CreateEmployee("instrutor.b", "Instrutor B");
            var classA = new Turma { Nome = "Turma exclusiva A", Modalidade = "Taiko", Ativa = true };
            var classB = new Turma { Nome = "Turma sigilosa B", Modalidade = "Taiko", Ativa = true };
            dbContext.AddRange(instructorA, instructorB, classA, classB);
            await dbContext.SaveChangesAsync();

            var lessonA = new Aula
            {
                TurmaId = classA.Id,
                InstrutorUsuarioId = instructorA.Id,
                Inicio = DateTime.Today.AddHours(18),
                Fim = DateTime.Today.AddHours(19)
            };
            var lessonB = new Aula
            {
                TurmaId = classB.Id,
                InstrutorUsuarioId = instructorB.Id,
                Inicio = DateTime.Today.AddHours(20),
                Fim = DateTime.Today.AddHours(21)
            };
            dbContext.Aulas.AddRange(lessonA, lessonB);
            await dbContext.SaveChangesAsync();

            ids.InstructorA = instructorA.Id;
            ids.LessonA = lessonA.Id;
            ids.LessonB = lessonB.Id;
        });

        var permissions = new[]
        {
            AppPermissions.FrequenciaView,
            AppPermissions.FrequenciaCreate,
            AppPermissions.FrequenciaEdit
        };
        using var instructorClient = factory.CreateAuthenticatedClient(
            ids.InstructorA,
            [AppRoles.Funcionario],
            permissions);

        var listHtml = await instructorClient.GetStringAsync("/admin/area-aluno/frequencia");
        Assert.Contains("Turma exclusiva A", listHtml);
        Assert.DoesNotContain("Turma sigilosa B", listHtml);

        using var ownLesson = await instructorClient.GetAsync($"/admin/area-aluno/frequencia/{ids.LessonA}");
        Assert.Equal(HttpStatusCode.OK, ownLesson.StatusCode);
        var antiforgeryToken = ExtractAntiforgeryToken(await ownLesson.Content.ReadAsStringAsync());

        using var missingAntiforgery = await instructorClient.PostAsync(
            $"/admin/area-aluno/frequencia/{ids.LessonA}",
            new FormUrlEncodedContent([]));
        Assert.Equal(HttpStatusCode.BadRequest, missingAntiforgery.StatusCode);

        using var deniedLesson = await instructorClient.GetAsync($"/admin/area-aluno/frequencia/{ids.LessonB}");
        Assert.Equal(HttpStatusCode.NotFound, deniedLesson.StatusCode);

        using var deniedPost = await instructorClient.PostAsync(
            $"/admin/area-aluno/frequencia/{ids.LessonB}",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = antiforgeryToken
            }));
        Assert.Equal(HttpStatusCode.NotFound, deniedPost.StatusCode);

        using var adminClient = factory.CreateAuthenticatedClient(999, [AppRoles.Admin]);
        var adminListHtml = await adminClient.GetStringAsync("/admin/area-aluno/frequencia");
        Assert.Contains("Turma exclusiva A", adminListHtml);
        Assert.Contains("Turma sigilosa B", adminListHtml);
        using var adminLesson = await adminClient.GetAsync($"/admin/area-aluno/frequencia/{ids.LessonB}");
        Assert.Equal(HttpStatusCode.OK, adminLesson.StatusCode);
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

    private static UsuarioSistema CreateEmployee(string login, string name)
    {
        return new UsuarioSistema
        {
            Login = login,
            LoginNormalizado = login.ToUpperInvariant(),
            NomeExibicao = name,
            SenhaHash = "integration-test",
            TipoAcesso = TipoAcessoEnum.Funcionario,
            Ativo = true
        };
    }

    private sealed class InstructorScenarioIds
    {
        public int InstructorA { get; set; }
        public int LessonA { get; set; }
        public int LessonB { get; set; }
    }
}
