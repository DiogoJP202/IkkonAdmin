using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class BlogAdminQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_NormalizaIdiomaEAplicaBusca()
    {
        await using var dbContext = CriarDbContext();
        dbContext.BlogCategories.Add(new BlogCategory { Name = "Cultura", Slug = "cultura" });
        dbContext.UsuariosSistema.Add(CriarAutor());
        dbContext.BlogPosts.Add(new BlogPost
        {
            Title = "English post",
            Slug = "english-post",
            LanguageCode = "en",
            Status = BlogPostStatusEnum.Published,
            AuthorDisplayName = "Equipe Ikkon",
            PublishedAtUtc = DateTime.UtcNow.AddDays(-1)
        });
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.ListarAsync(new BlogAdminFilterViewModel { Busca = "English" });

        var post = Assert.Single(resultado.Posts);
        Assert.Equal("en-US", post.LanguageCode);
        Assert.Equal("Inglês", post.LanguageLabel);
        Assert.Equal(1, resultado.TotalPosts);
        Assert.Equal(1, resultado.PublishedCount);
    }

    [Fact]
    public async Task ObterFormEdicaoAsync_MontaOpcoesETags()
    {
        await using var dbContext = CriarDbContext();
        var categoria = new BlogCategory { Name = "Eventos", Slug = "eventos" };
        var autor = CriarAutor();
        var tag = new BlogTag { Name = "Taiko", Slug = "taiko" };
        var post = new BlogPost
        {
            Title = "Post com tag",
            Slug = "post-com-tag",
            LanguageCode = "pt-BR",
            ContentText = "Conteúdo em texto",
            ContentHtml = "<p>Conteúdo em texto</p>",
            Status = BlogPostStatusEnum.Draft,
            Category = categoria,
            AuthorUser = autor,
            AuthorDisplayName = autor.NomeExibicao,
            CreatedAtUtc = DateTime.UtcNow.AddDays(-2)
        };

        post.PostTags.Add(new BlogPostTag
        {
            BlogPost = post,
            BlogTag = tag
        });

        dbContext.Add(post);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var form = await service.ObterFormEdicaoAsync(post.Id);

        Assert.NotNull(form);
        Assert.Equal("Português", form.LanguageLabel);
        Assert.Equal("Taiko", form.TagsInput);
        Assert.Contains(form.CategoryOptions, x => x.Id == categoria.Id);
        Assert.Contains(form.AuthorOptions, x => x.Id == autor.Id);
        Assert.Contains("Taiko", form.TagSuggestions);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static BlogAdminQueryService CriarService(ApplicationDbContext dbContext)
    {
        var lookupService = new BlogLookupService(dbContext);
        var languageService = new BlogLanguageService();
        var dateTimeService = new BlogDateTimeService();
        var textService = new BlogTextService(new BlogContentSanitizer());
        var slugService = new BlogSlugService(dbContext);
        var workflowService = new BlogWorkflowService(
            dbContext,
            lookupService,
            slugService,
            textService,
            dateTimeService);

        return new BlogAdminQueryService(
            dbContext,
            workflowService,
            lookupService,
            languageService,
            dateTimeService);
    }

    private static UsuarioSistema CriarAutor()
    {
        return new UsuarioSistema
        {
            Login = "autor",
            LoginNormalizado = "AUTOR",
            NomeExibicao = "Autor Ikkon",
            SenhaHash = "hash",
            TipoAcesso = TipoAcessoEnum.Funcionario,
            Ativo = true
        };
    }
}
