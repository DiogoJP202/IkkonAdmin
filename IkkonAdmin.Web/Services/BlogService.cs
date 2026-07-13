using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class BlogService(
    ApplicationDbContext dbContext,
    IBlogMediaService blogMediaService,
    IBlogContentSanitizer blogContentSanitizer) : IBlogService
{
    private const string DefaultBlogLanguageCode = "pt-BR";
    private static readonly IReadOnlyList<BlogLanguageDefinition> SupportedBlogLanguages =
    [
        new("pt-BR", "Português", "Português", "pt", true),
        new("en-US", "Inglês", "English", "en", false),
        new("ja-JP", "Japonês", "日本語", "ja", false)
    ];

    public async Task<BlogAdminIndexViewModel> ListarAsync(
        BlogAdminFilterViewModel filtro,
        CancellationToken cancellationToken = default)
    {
        await PromoteScheduledPostsAsync(cancellationToken);

        var baseQuery = dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null);

        var query = baseQuery;

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = filtro.Busca.Trim();
            query = query.Where(x =>
                x.Title.Contains(termo) ||
                (x.Summary != null && x.Summary.Contains(termo)) ||
                (x.ContentText != null && x.ContentText.Contains(termo)) ||
                (x.Category != null && x.Category.Name.Contains(termo)) ||
                x.PostTags.Any(t => t.BlogTag.Name.Contains(termo)));
        }

        if (filtro.Status.HasValue)
        {
            query = query.Where(x => x.Status == filtro.Status.Value);
        }

        if (filtro.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == filtro.CategoryId.Value);
        }

        if (filtro.AuthorUserId.HasValue)
        {
            query = query.Where(x => x.AuthorUserId == filtro.AuthorUserId.Value);
        }

        if (filtro.IsFeatured.HasValue)
        {
            query = query.Where(x => x.IsFeatured == filtro.IsFeatured.Value);
        }

        if (filtro.IsWeeklyHighlight.HasValue)
        {
            query = query.Where(x => x.IsWeeklyHighlight == filtro.IsWeeklyHighlight.Value);
        }

        if (filtro.PublishedFrom.HasValue)
        {
            var fromUtc = ConvertSaoPauloDateOnlyToUtcStart(filtro.PublishedFrom.Value);
            query = query.Where(x => x.PublishedAtUtc >= fromUtc);
        }

        if (filtro.PublishedTo.HasValue)
        {
            var toUtcExclusive = ConvertSaoPauloDateOnlyToUtcEndExclusive(filtro.PublishedTo.Value);
            query = query.Where(x => x.PublishedAtUtc < toUtcExclusive);
        }

        var posts = await query
            .OrderByDescending(x => x.PublishedAtUtc ?? x.ScheduledAtUtc ?? x.CreatedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new BlogPostListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                LanguageCode = x.LanguageCode,
                TranslationVersionCount = dbContext.BlogPosts.Count(v =>
                    v.DeletedAtUtc == null &&
                    (v.Id == (x.TranslationGroupId ?? x.Id) || v.TranslationGroupId == (x.TranslationGroupId ?? x.Id))),
                Status = x.Status,
                CategoryName = x.Category != null ? x.Category.Name : null,
                AuthorName = x.AuthorDisplayName,
                CreatedAtUtc = x.CreatedAtUtc,
                PublishedAtUtc = x.PublishedAtUtc,
                ScheduledAtUtc = x.ScheduledAtUtc,
                IsFeatured = x.IsFeatured,
                IsWeeklyHighlight = x.IsWeeklyHighlight,
                TagCount = x.PostTags.Count
            })
            .ToListAsync(cancellationToken);

        foreach (var post in posts)
        {
            post.LanguageCode = NormalizarIdiomaBlog(post.LanguageCode);
            post.LanguageLabel = ObterRotuloIdioma(post.LanguageCode);
        }

        return new BlogAdminIndexViewModel
        {
            Filtro = filtro,
            TotalPosts = await baseQuery.CountAsync(cancellationToken),
            DraftCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Draft, cancellationToken),
            ScheduledCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Scheduled, cancellationToken),
            PublishedCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Published, cancellationToken),
            ArchivedCount = await baseQuery.CountAsync(x => x.Status == BlogPostStatusEnum.Archived, cancellationToken),
            Posts = posts,
            Categories = await ListarCategoriasFiltroAsync(cancellationToken),
            Authors = await ListarAutoresAsync(cancellationToken)
        };
    }

    public async Task<BlogPostFormViewModel> ObterFormCriacaoAsync(
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var categorias = await ListarCategoriasFormularioAsync(null, cancellationToken);
        var autores = await ListarAutoresAsync(cancellationToken);

        return new BlogPostFormViewModel
        {
            LanguageCode = DefaultBlogLanguageCode,
            LanguageLabel = ObterRotuloIdioma(DefaultBlogLanguageCode),
            AuthorUserId = autores.Any(x => x.Id == usuarioAtualId) ? usuarioAtualId : autores.FirstOrDefault()?.Id,
            CategoryOptions = categorias,
            AuthorOptions = autores,
            TagSuggestions = await ListarSugestoesTagsAsync(cancellationToken)
        };
    }

    public async Task<BlogPostFormViewModel?> ObterFormEdicaoAsync(int id, CancellationToken cancellationToken = default)
    {
        await PromoteScheduledPostsAsync(cancellationToken);

        var post = await dbContext.BlogPosts
            .AsNoTracking()
            .Include(x => x.PostTags)
            .ThenInclude(x => x.BlogTag)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);

        if (post is null)
        {
            return null;
        }

        return new BlogPostFormViewModel
        {
            Id = post.Id,
            LanguageCode = NormalizarIdiomaBlog(post.LanguageCode),
            LanguageLabel = ObterRotuloIdioma(post.LanguageCode),
            TranslationGroupId = post.TranslationGroupId,
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            ContentInput = post.ContentText,
            ContentHtmlInput = post.ContentHtml,
            ContentJsonInput = post.ContentJson,
            CategoryId = post.CategoryId,
            AuthorUserId = post.AuthorUserId,
            IsFeatured = post.IsFeatured,
            IsWeeklyHighlight = post.IsWeeklyHighlight,
            PublicationDateLocal = ConvertUtcToSaoPauloLocal(post.ScheduledAtUtc ?? post.PublishedAtUtc),
            SeoTitle = post.SeoTitle,
            SeoDescription = post.SeoDescription,
            TagsInput = string.Join(", ", post.PostTags
                .OrderBy(x => x.BlogTag.Name)
                .Select(x => x.BlogTag.Name)),
            CurrentCoverImageUrl = post.CoverImageUrl,
            CurrentStatus = post.Status,
            CreatedAtUtc = post.CreatedAtUtc,
            UpdatedAtUtc = post.UpdatedAtUtc,
            PublishedAtUtc = post.PublishedAtUtc,
            CategoryOptions = await ListarCategoriasFormularioAsync(post.CategoryId, cancellationToken),
            AuthorOptions = await ListarAutoresAsync(cancellationToken),
            TagSuggestions = await ListarSugestoesTagsAsync(cancellationToken)
        };
    }

    public async Task<BlogPublicIndexViewModel> ListarPublicoAsync(
        BlogPublicFilterViewModel filtro,
        CancellationToken cancellationToken = default)
    {
        await PromoteScheduledPostsAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var pageSize = 9;
        var currentPage = Math.Max(1, filtro.Pagina);
        var currentLanguage = ObterIdiomaAtualBlog();
        var query = AplicarFiltrosPublicos(CriarConsultaPublica(now), filtro);
        var selectedPosts = await SelecionarVersoesPublicasAsync(query, currentLanguage, cancellationToken);
        var totalPosts = selectedPosts.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalPosts / (decimal)pageSize));

        if (currentPage > totalPages)
        {
            currentPage = totalPages;
        }

        filtro.Pagina = currentPage;

        var pageIds = selectedPosts
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToList();
        var featuredIds = (await SelecionarVersoesPublicasAsync(
                CriarConsultaPublica(now).Where(x => x.IsFeatured),
                currentLanguage,
                cancellationToken))
            .Take(3)
            .Select(x => x.Id)
            .ToList();
        var weeklyIds = (await SelecionarVersoesPublicasAsync(
                CriarConsultaPublica(now).Where(x => x.IsWeeklyHighlight),
                currentLanguage,
                cancellationToken))
            .Take(2)
            .Select(x => x.Id)
            .ToList();

        return new BlogPublicIndexViewModel
        {
            Filtro = filtro,
            FeaturedPosts = await ObterCardsPublicosPorIdsAsync(now, featuredIds, cancellationToken),
            WeeklyHighlights = await ObterCardsPublicosPorIdsAsync(now, weeklyIds, cancellationToken),
            Posts = await ObterCardsPublicosPorIdsAsync(now, pageIds, cancellationToken),
            Categories = await ListarCategoriasPublicasAsync(now, filtro.Categoria, cancellationToken),
            Tags = await ListarTagsPublicasAsync(now, filtro.Tag, cancellationToken),
            TotalPosts = totalPosts,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }

    public async Task<BlogPublicDetailsViewModel?> ObterPublicoPorSlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        await PromoteScheduledPostsAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var normalizedSlug = slug.Trim();
        var currentLanguage = ObterIdiomaAtualBlog();

        var source = await CriarConsultaPublica(now)
            .Where(x => x.Slug == normalizedSlug)
            .Select(x => new
            {
                x.Id,
                x.LanguageCode,
                x.TranslationGroupId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return null;
        }

        var groupId = source.TranslationGroupId ?? source.Id;
        var versionIds = await SelecionarVersoesPublicasAsync(
            CriarConsultaPublica(now).Where(x => x.Id == groupId || x.TranslationGroupId == groupId),
            currentLanguage,
            cancellationToken);
        var selectedId = versionIds.FirstOrDefault()?.Id ?? source.Id;

        var post = await CriarConsultaPublica(now)
            .Where(x => x.Id == selectedId)
            .Select(x => new BlogPublicDetailsViewModel
            {
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                ContentHtml = x.ContentHtml ?? string.Empty,
                CoverImageUrl = x.CoverImageUrl,
                AuthorName = x.AuthorDisplayName,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CategorySlug = x.Category != null ? x.Category.Slug : null,
                PublishedAtUtc = x.PublishedAtUtc ?? x.CreatedAtUtc,
                ReadingTimeMinutes = x.ReadingTimeMinutes,
                SeoTitle = x.SeoTitle,
                SeoDescription = x.SeoDescription,
                Tags = x.PostTags
                    .OrderBy(t => t.BlogTag.Name)
                    .Select(t => new BlogPublicTagViewModel
                    {
                        Name = t.BlogTag.Name,
                        Slug = t.BlogTag.Slug
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (post is null)
        {
            return null;
        }

        var relatedQuery = CriarConsultaPublica(now)
            .Where(x => (x.TranslationGroupId ?? x.Id) != groupId);

        if (!string.IsNullOrWhiteSpace(post.CategorySlug))
        {
            relatedQuery = relatedQuery.Where(x => x.Category != null && x.Category.Slug == post.CategorySlug);
        }

        var relatedSelections = (await SelecionarVersoesPublicasAsync(relatedQuery, currentLanguage, cancellationToken))
            .Take(3)
            .ToList();
        var relatedPosts = await ObterCardsPublicosPorIdsAsync(
            now,
            relatedSelections.Select(x => x.Id).ToList(),
            cancellationToken);

        if (relatedPosts.Count < 3)
        {
            var excludedGroupIds = relatedSelections
                .Select(x => x.GroupId)
                .Append(groupId)
                .ToList();

            var fallbackSelections = (await SelecionarVersoesPublicasAsync(
                    CriarConsultaPublica(now)
                        .Where(x => !excludedGroupIds.Contains(x.TranslationGroupId ?? x.Id)),
                    currentLanguage,
                    cancellationToken))
                .Take(3 - relatedPosts.Count)
                .ToList();
            var fallbackPosts = await ObterCardsPublicosPorIdsAsync(
                now,
                fallbackSelections.Select(x => x.Id).ToList(),
                cancellationToken);

            relatedPosts.AddRange(fallbackPosts);
        }

        post.RelatedPosts = relatedPosts;
        return post;
    }

    public async Task<BlogPreviewViewModel?> ObterPreviewAsync(int id, CancellationToken cancellationToken = default)
    {
        await PromoteScheduledPostsAsync(cancellationToken);

        return await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAtUtc == null)
            .Select(x => new BlogPreviewViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                Summary = x.Summary,
                CoverImageUrl = x.CoverImageUrl,
                CategoryName = x.Category != null ? x.Category.Name : null,
                AuthorName = x.AuthorDisplayName,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc,
                PublishedAtUtc = x.PublishedAtUtc,
                IsFeatured = x.IsFeatured,
                IsWeeklyHighlight = x.IsWeeklyHighlight,
                ContentHtml = x.ContentHtml ?? "<p>Conteúdo ainda não informado.</p>",
                Tags = x.PostTags
                    .OrderBy(t => t.BlogTag.Name)
                    .Select(t => t.BlogTag.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BlogVersionOverviewViewModel?> ObterVersoesAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var source = await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAtUtc == null)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.LanguageCode,
                x.TranslationGroupId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return null;
        }

        var groupId = source.TranslationGroupId ?? source.Id;
        var versions = await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null &&
                        (x.Id == groupId || x.TranslationGroupId == groupId))
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.Slug,
                x.LanguageCode,
                x.Status,
                x.UpdatedAtUtc,
                x.PublishedAtUtc,
                x.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        return new BlogVersionOverviewViewModel
        {
            SourcePostId = source.Id,
            TranslationGroupId = groupId,
            SourceTitle = source.Title,
            SourceLanguageCode = NormalizarIdiomaBlog(source.LanguageCode),
            Versions = SupportedBlogLanguages
                .Select(language =>
                {
                    var version = versions
                        .Where(x => string.Equals(NormalizarIdiomaBlog(x.LanguageCode), language.Code, StringComparison.OrdinalIgnoreCase))
                        .OrderByDescending(x => x.Id == source.Id)
                        .ThenBy(x => x.Id)
                        .FirstOrDefault();

                    return new BlogVersionItemViewModel
                    {
                        LanguageCode = language.Code,
                        LanguageLabel = language.Label,
                        NativeLabel = language.NativeLabel,
                        SlugSuffix = language.SlugSuffix,
                        IsDefault = language.IsDefault,
                        IsCurrent = version?.Id == source.Id,
                        PostId = version?.Id,
                        Title = version?.Title,
                        Slug = version?.Slug,
                        Status = version?.Status,
                        UpdatedAtUtc = version?.UpdatedAtUtc ?? version?.CreatedAtUtc,
                        PublishedAtUtc = version?.PublishedAtUtc
                    };
                })
                .ToList()
        };
    }

    public async Task<BlogOperationResult> CriarAsync(
        BlogPostFormViewModel model,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var author = await ObterAutorValidoAsync(model.AuthorUserId, cancellationToken);
        var validacao = await ValidarFluxoEditorialAsync(
            model,
            author,
            currentCoverUrl: null,
            currentStatus: BlogPostStatusEnum.Draft,
            currentPublishedAtUtc: null,
            cancellationToken);

        if (!validacao.Success)
        {
            return BlogOperationResult.Fail(validacao.Message);
        }

        string? coverImageUrl = null;
        if (model.CoverImage is not null)
        {
            var saveResult = await blogMediaService.SaveCoverImageAsync(model.CoverImage, null, cancellationToken);
            if (!saveResult.Success)
            {
                return BlogOperationResult.Fail(saveResult.Message);
            }

            coverImageUrl = saveResult.PublicUrl;
        }

        var now = DateTime.UtcNow;
        var summary = LimparOpcional(model.Summary);
        var contentText = ObterConteudoTexto(model);
        var contentHtml = GerarConteudoHtmlSeguro(model);
        var contentJson = LimparOpcional(model.ContentJsonInput);
        var slug = await GarantirSlugUnicoAsync(GerarSlug(model.Slug, model.Title), null, cancellationToken);
        var languageCode = NormalizarIdiomaBlog(model.LanguageCode);

        var post = new BlogPost
        {
            Title = model.Title.Trim(),
            Slug = slug,
            Summary = summary,
            ContentHtml = contentHtml,
            ContentJson = contentJson,
            ContentText = contentText,
            CoverImageUrl = coverImageUrl,
            AuthorUserId = author?.Id ?? usuarioAtualId,
            AuthorDisplayName = author?.NomeExibicao ?? author?.Login ?? "Equipe Ikkon",
            CategoryId = model.CategoryId,
            Status = validacao.Status,
            LanguageCode = languageCode,
            IsFeatured = model.IsFeatured || model.IsWeeklyHighlight,
            IsWeeklyHighlight = model.IsWeeklyHighlight,
            PublishedAtUtc = validacao.PublishedAtUtc,
            ScheduledAtUtc = validacao.ScheduledAtUtc,
            ArchivedAtUtc = validacao.ArchivedAtUtc,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            SeoTitle = LimparOpcional(model.SeoTitle),
            SeoDescription = LimparOpcional(model.SeoDescription) ?? summary,
            ReadingTimeMinutes = CalcularTempoLeitura(contentText)
        };

        await dbContext.BlogPosts.AddAsync(post, cancellationToken);
        await SincronizarTagsAsync(post, model.TagsInput, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok(
            validacao.Status == BlogPostStatusEnum.Scheduled ? "Post criado e agendado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Published ? "Post criado e publicado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Archived ? "Post criado e arquivado com sucesso." :
            "Rascunho criado com sucesso.",
            post.Id);
    }

    public async Task<BlogOperationResult> CriarVersaoAsync(
        int id,
        string languageCode,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var language = ObterDefinicaoIdioma(languageCode);
        if (language is null)
        {
            return BlogOperationResult.Fail("Idioma não suportado para versões do blog.");
        }

        var source = await dbContext.BlogPosts
            .Include(x => x.PostTags)
            .ThenInclude(x => x.BlogTag)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);

        if (source is null)
        {
            return BlogOperationResult.Fail("Post não encontrado.");
        }

        var groupId = source.TranslationGroupId ?? source.Id;
        var versionExists = await dbContext.BlogPosts.AnyAsync(
            x => x.DeletedAtUtc == null &&
                 (x.Id == groupId || x.TranslationGroupId == groupId) &&
                 x.LanguageCode == language.Code,
            cancellationToken);

        if (versionExists)
        {
            return BlogOperationResult.Fail("Já existe uma versão neste idioma.");
        }

        var now = DateTime.UtcNow;
        var sourceSlug = string.IsNullOrWhiteSpace(source.Slug)
            ? GerarSlug(null, source.Title)
            : source.Slug;
        var slug = await GarantirSlugUnicoAsync(
            GerarSlug($"{sourceSlug}-{language.SlugSuffix}", $"{source.Title}-{language.SlugSuffix}"),
            null,
            cancellationToken);
        var tagsInput = string.Join(", ", source.PostTags
            .OrderBy(x => x.BlogTag.Name)
            .Select(x => x.BlogTag.Name));

        var version = new BlogPost
        {
            Title = source.Title,
            Slug = slug,
            Summary = source.Summary,
            ContentHtml = source.ContentHtml,
            ContentJson = source.ContentJson,
            ContentText = source.ContentText,
            CoverImageUrl = source.CoverImageUrl,
            AuthorUserId = source.AuthorUserId ?? usuarioAtualId,
            AuthorDisplayName = source.AuthorDisplayName,
            CategoryId = source.CategoryId,
            Status = BlogPostStatusEnum.Draft,
            LanguageCode = language.Code,
            TranslationGroupId = groupId,
            IsFeatured = source.IsFeatured,
            IsWeeklyHighlight = source.IsWeeklyHighlight,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            SeoTitle = source.SeoTitle,
            SeoDescription = source.SeoDescription,
            ReadingTimeMinutes = source.ReadingTimeMinutes
        };

        await dbContext.BlogPosts.AddAsync(version, cancellationToken);
        await SincronizarTagsAsync(version, tagsInput, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok("Versão criada como rascunho. Revise a tradução antes de publicar.", version.Id);
    }

    public async Task<BlogOperationResult> AtualizarAsync(
        int id,
        BlogPostFormViewModel model,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        await PromoteScheduledPostsAsync(cancellationToken);

        var post = await dbContext.BlogPosts
            .Include(x => x.PostTags)
            .ThenInclude(x => x.BlogTag)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);

        if (post is null)
        {
            return BlogOperationResult.Fail("Post não encontrado.");
        }

        var author = await ObterAutorValidoAsync(model.AuthorUserId, cancellationToken);
        var validacao = await ValidarFluxoEditorialAsync(
            model,
            author,
            post.CoverImageUrl,
            post.Status,
            post.PublishedAtUtc,
            cancellationToken);

        if (!validacao.Success)
        {
            return BlogOperationResult.Fail(validacao.Message);
        }

        var coverImageUrl = post.CoverImageUrl;

        if (model.RemoveCoverImage && model.CoverImage is null)
        {
            await blogMediaService.RemoveCoverImageAsync(post.CoverImageUrl, cancellationToken);
            coverImageUrl = null;
        }

        if (model.CoverImage is not null)
        {
            var saveResult = await blogMediaService.SaveCoverImageAsync(model.CoverImage, post.CoverImageUrl, cancellationToken);
            if (!saveResult.Success)
            {
                return BlogOperationResult.Fail(saveResult.Message);
            }

            coverImageUrl = saveResult.PublicUrl;
        }

        var summary = LimparOpcional(model.Summary);
        var contentText = ObterConteudoTexto(model);
        var contentHtml = GerarConteudoHtmlSeguro(model);
        var contentJson = LimparOpcional(model.ContentJsonInput);
        var slug = await GarantirSlugUnicoAsync(GerarSlug(model.Slug, model.Title), id, cancellationToken);

        post.Title = model.Title.Trim();
        post.Slug = slug;
        post.Summary = summary;
        post.ContentHtml = contentHtml;
        post.ContentJson = contentJson;
        post.ContentText = contentText;
        post.CoverImageUrl = coverImageUrl;
        post.AuthorUserId = author?.Id ?? usuarioAtualId;
        post.AuthorDisplayName = author?.NomeExibicao ?? author?.Login ?? post.AuthorDisplayName;
        post.CategoryId = model.CategoryId;
        post.Status = validacao.Status;
        post.LanguageCode = NormalizarIdiomaBlog(post.LanguageCode);
        post.IsFeatured = model.IsFeatured || model.IsWeeklyHighlight;
        post.IsWeeklyHighlight = model.IsWeeklyHighlight;
        post.PublishedAtUtc = validacao.PublishedAtUtc;
        post.ScheduledAtUtc = validacao.ScheduledAtUtc;
        post.ArchivedAtUtc = validacao.ArchivedAtUtc;
        post.UpdatedAtUtc = DateTime.UtcNow;
        post.SeoTitle = LimparOpcional(model.SeoTitle);
        post.SeoDescription = LimparOpcional(model.SeoDescription) ?? summary;
        post.ReadingTimeMinutes = CalcularTempoLeitura(contentText);

        await SincronizarTagsAsync(post, model.TagsInput, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok(
            validacao.Status == BlogPostStatusEnum.Scheduled ? "Post atualizado e agendado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Published ? "Post atualizado e publicado com sucesso." :
            validacao.Status == BlogPostStatusEnum.Archived ? "Post arquivado com sucesso." :
            "Rascunho atualizado com sucesso.",
            post.Id);
    }

    public async Task<BlogOperationResult> ExcluirAsync(
        int id,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        var post = await dbContext.BlogPosts.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAtUtc == null, cancellationToken);
        if (post is null)
        {
            return BlogOperationResult.Fail("Post não encontrado.");
        }

        post.DeletedAtUtc = DateTime.UtcNow;
        post.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok("Post excluído com sucesso.", post.Id);
    }

    public async Task<BlogOperationResult> ExcluirVersaoAsync(
        int id,
        int versionId,
        int? usuarioAtualId,
        CancellationToken cancellationToken = default)
    {
        if (id == versionId)
        {
            return BlogOperationResult.Fail("Use a ação principal do post para excluir a versão aberta.");
        }

        var source = await dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAtUtc == null)
            .Select(x => new { x.Id, x.TranslationGroupId })
            .FirstOrDefaultAsync(cancellationToken);

        var version = await dbContext.BlogPosts
            .FirstOrDefaultAsync(x => x.Id == versionId && x.DeletedAtUtc == null, cancellationToken);

        if (source is null || version is null)
        {
            return BlogOperationResult.Fail("Versão não encontrada.");
        }

        var sourceGroupId = source.TranslationGroupId ?? source.Id;
        var versionGroupId = version.TranslationGroupId ?? version.Id;
        if (sourceGroupId != versionGroupId)
        {
            return BlogOperationResult.Fail("Esta versão não pertence ao post atual.");
        }

        var now = DateTime.UtcNow;
        version.DeletedAtUtc = now;
        version.UpdatedAtUtc = now;
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok("Versão excluída com sucesso.", version.Id);
    }

    private async Task<BlogWorkflowValidation> ValidarFluxoEditorialAsync(
        BlogPostFormViewModel model,
        UsuarioSistema? author,
        string? currentCoverUrl,
        BlogPostStatusEnum currentStatus,
        DateTime? currentPublishedAtUtc,
        CancellationToken cancellationToken)
    {
        var acao = (model.SubmissionAction ?? "Draft").Trim().ToLowerInvariant();
        var publicationDateUtc = model.PublicationDateLocal.HasValue
            ? ConvertSaoPauloLocalToUtc(model.PublicationDateLocal.Value)
            : (DateTime?)null;
        var now = DateTime.UtcNow;
        var summary = LimparOpcional(model.Summary);
        var contentText = ObterConteudoTexto(model);
        var hasContent = !string.IsNullOrWhiteSpace(contentText) ||
                         ConteudoHtmlTemMidia(SanitizarHtmlParaValidacao(model.ContentHtmlInput));
        var hasCover = model.CoverImage is not null ||
                       (!model.RemoveCoverImage && !string.IsNullOrWhiteSpace(currentCoverUrl));
        var slugBase = GerarSlug(model.Slug, model.Title);

        if (!await CategoriaValidaAsync(model.CategoryId, cancellationToken))
        {
            return BlogWorkflowValidation.Fail("Selecione uma categoria ativa.");
        }

        if (!string.IsNullOrWhiteSpace(model.Slug) &&
            await SlugExisteAsync(slugBase, model.Id, cancellationToken))
        {
            return BlogWorkflowValidation.Fail("Já existe um post com esse slug.");
        }

        switch (acao)
        {
            case "publish":
                if (publicationDateUtc.HasValue && publicationDateUtc.Value > now)
                {
                    return BlogWorkflowValidation.Fail("Para data futura, use a ação Agendar.");
                }

                var pendenciasPublicacao = ListarPendenciasPublicacao(summary, hasContent, author, hasCover, model.CategoryId);
                if (pendenciasPublicacao.Count > 0)
                {
                    return BlogWorkflowValidation.Fail($"Para publicar, informe {FormatarListaPendencias(pendenciasPublicacao)}.");
                }

                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Published,
                    publicationDateUtc ?? now,
                    null,
                    null);

            case "schedule":
                if (!publicationDateUtc.HasValue || publicationDateUtc.Value <= now)
                {
                    return BlogWorkflowValidation.Fail("Informe uma data futura para agendar a publicação.");
                }

                var pendenciasAgendamento = ListarPendenciasPublicacao(summary, hasContent, author, hasCover, model.CategoryId);
                if (pendenciasAgendamento.Count > 0)
                {
                    return BlogWorkflowValidation.Fail($"Para agendar, informe {FormatarListaPendencias(pendenciasAgendamento)}.");
                }

                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Scheduled,
                    null,
                    publicationDateUtc,
                    null);

            case "archive":
                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Archived,
                    currentStatus == BlogPostStatusEnum.Published ? currentPublishedAtUtc : null,
                    null,
                    now);

            default:
                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Draft,
                    null,
                    null,
                    null);
        }
    }

    private async Task SincronizarTagsAsync(BlogPost post, string? tagsInput, CancellationToken cancellationToken)
    {
        var names = ParseTags(tagsInput);
        var desiredSlugs = names
            .Select(name => new { Name = name, Slug = GerarSlug(null, name) })
            .Where(x => !string.IsNullOrWhiteSpace(x.Slug))
            .DistinctBy(x => x.Slug, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingTags = desiredSlugs.Count == 0
            ? []
            : await dbContext.BlogTags
                .Where(x => desiredSlugs.Select(y => y.Slug).Contains(x.Slug))
                .ToListAsync(cancellationToken);

        post.PostTags.Clear();

        foreach (var item in desiredSlugs)
        {
            var tag = existingTags.FirstOrDefault(x => x.Slug == item.Slug);
            if (tag is null)
            {
                tag = new BlogTag
                {
                    Name = item.Name,
                    Slug = item.Slug,
                    IsActive = true,
                    CreatedAtUtc = DateTime.UtcNow
                };

                dbContext.BlogTags.Add(tag);
                existingTags.Add(tag);
            }

            post.PostTags.Add(new BlogPostTag
            {
                BlogPost = post,
                BlogTag = tag
            });
        }
    }

    private async Task PromoteScheduledPostsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var scheduledPosts = await dbContext.BlogPosts
            .Where(x => x.DeletedAtUtc == null &&
                        x.Status == BlogPostStatusEnum.Scheduled &&
                        x.ScheduledAtUtc.HasValue &&
                        x.ScheduledAtUtc <= now)
            .ToListAsync(cancellationToken);

        if (scheduledPosts.Count == 0)
        {
            return;
        }

        foreach (var post in scheduledPosts)
        {
            post.Status = BlogPostStatusEnum.Published;
            post.PublishedAtUtc = post.ScheduledAtUtc ?? now;
            post.ScheduledAtUtc = null;
            post.ArchivedAtUtc = null;
            post.UpdatedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<UsuarioSistema?> ObterAutorValidoAsync(int? authorUserId, CancellationToken cancellationToken)
    {
        if (!authorUserId.HasValue)
        {
            return null;
        }

        return await dbContext.UsuariosSistema
            .FirstOrDefaultAsync(
                x => x.Id == authorUserId.Value &&
                     x.Ativo &&
                     x.TipoAcesso != TipoAcessoEnum.Aluno,
                cancellationToken);
    }

    private async Task<List<BlogCategorySelectItemViewModel>> ListarCategoriasFiltroAsync(CancellationToken cancellationToken)
    {
        return await dbContext.BlogCategories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategorySelectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<BlogCategorySelectItemViewModel>> ListarCategoriasFormularioAsync(int? categoriaAtualId, CancellationToken cancellationToken)
    {
        return await dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.IsActive || x.Id == categoriaAtualId)
            .OrderBy(x => x.Name)
            .Select(x => new BlogCategorySelectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<BlogAuthorSelectItemViewModel>> ListarAutoresAsync(CancellationToken cancellationToken)
    {
        return await dbContext.UsuariosSistema
            .AsNoTracking()
            .Where(x => x.Ativo && x.TipoAcesso != TipoAcessoEnum.Aluno)
            .OrderBy(x => x.NomeExibicao)
            .Select(x => new BlogAuthorSelectItemViewModel
            {
                Id = x.Id,
                Nome = x.NomeExibicao
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> ListarSugestoesTagsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.BlogTags
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .Take(30)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<BlogPublicPostSelection>> SelecionarVersoesPublicasAsync(
        IQueryable<BlogPost> query,
        string languageCode,
        CancellationToken cancellationToken)
    {
        var candidates = await query
            .Select(x => new BlogPublicPostSelection(
                x.Id,
                x.TranslationGroupId ?? x.Id,
                x.LanguageCode,
                x.PublishedAtUtc,
                x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return candidates
            .GroupBy(x => x.GroupId)
            .Select(group => EscolherMelhorVersaoPublica(group, languageCode))
            .OrderByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    private async Task<List<BlogPublicPostCardViewModel>> ObterCardsPublicosPorIdsAsync(
        DateTime nowUtc,
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var order = ids
            .Select((id, index) => new { id, index })
            .ToDictionary(x => x.id, x => x.index);
        var cards = await SelecionarCardsPublicos(CriarConsultaPublica(nowUtc).Where(x => ids.Contains(x.Id)))
            .ToListAsync(cancellationToken);

        return cards
            .OrderBy(x => order.TryGetValue(x.Id, out var index) ? index : int.MaxValue)
            .ToList();
    }

    private IQueryable<BlogPost> CriarConsultaPublica(DateTime nowUtc)
    {
        return dbContext.BlogPosts
            .AsNoTracking()
            .Where(x => x.DeletedAtUtc == null &&
                        x.Status == BlogPostStatusEnum.Published &&
                        x.PublishedAtUtc.HasValue &&
                        x.PublishedAtUtc <= nowUtc)
            .OrderByDescending(x => x.PublishedAtUtc)
            .ThenByDescending(x => x.CreatedAtUtc);
    }

    private static IQueryable<BlogPost> AplicarFiltrosPublicos(
        IQueryable<BlogPost> query,
        BlogPublicFilterViewModel filtro)
    {
        if (!string.IsNullOrWhiteSpace(filtro.Q))
        {
            var termo = filtro.Q.Trim();
            query = query.Where(x =>
                x.Title.Contains(termo) ||
                (x.Summary != null && x.Summary.Contains(termo)) ||
                (x.ContentText != null && x.ContentText.Contains(termo)) ||
                (x.Category != null && x.Category.Name.Contains(termo)) ||
                x.PostTags.Any(t => t.BlogTag.Name.Contains(termo)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Categoria))
        {
            var categoriaSlug = filtro.Categoria.Trim();
            query = query.Where(x => x.Category != null && x.Category.Slug == categoriaSlug);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tag))
        {
            var tagSlug = filtro.Tag.Trim();
            query = query.Where(x => x.PostTags.Any(t => t.BlogTag.Slug == tagSlug));
        }

        return query;
    }

    private static IQueryable<BlogPublicPostCardViewModel> SelecionarCardsPublicos(IQueryable<BlogPost> query)
    {
        return query.Select(x => new BlogPublicPostCardViewModel
        {
            Id = x.Id,
            Title = x.Title,
            Slug = x.Slug,
            Summary = x.Summary,
            CoverImageUrl = x.CoverImageUrl,
            AuthorName = x.AuthorDisplayName,
            CategoryName = x.Category != null ? x.Category.Name : null,
            CategorySlug = x.Category != null ? x.Category.Slug : null,
            PublishedAtUtc = x.PublishedAtUtc ?? x.CreatedAtUtc,
            ReadingTimeMinutes = x.ReadingTimeMinutes,
            IsFeatured = x.IsFeatured,
            IsWeeklyHighlight = x.IsWeeklyHighlight,
            Tags = x.PostTags
                .OrderBy(t => t.BlogTag.Name)
                .Select(t => new BlogPublicTagViewModel
                {
                    Name = t.BlogTag.Name,
                    Slug = t.BlogTag.Slug
                })
                .Take(5)
                .ToList()
        });
    }

    private async Task<List<BlogPublicTaxonomyItemViewModel>> ListarCategoriasPublicasAsync(
        DateTime nowUtc,
        string? selectedSlug,
        CancellationToken cancellationToken)
    {
        var categorias = await dbContext.BlogCategories
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new BlogPublicTaxonomyItemViewModel
            {
                Name = x.Name,
                Slug = x.Slug,
                Count = x.Posts.Count(p =>
                    p.DeletedAtUtc == null &&
                    p.Status == BlogPostStatusEnum.Published &&
                    p.PublishedAtUtc.HasValue &&
                    p.PublishedAtUtc <= nowUtc),
                IsActive = x.Slug == selectedSlug
            })
            .ToListAsync(cancellationToken);

        return categorias.Where(x => x.Count > 0 || x.IsActive).ToList();
    }

    private async Task<List<BlogPublicTaxonomyItemViewModel>> ListarTagsPublicasAsync(
        DateTime nowUtc,
        string? selectedSlug,
        CancellationToken cancellationToken)
    {
        var tags = await dbContext.BlogTags
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new BlogPublicTaxonomyItemViewModel
            {
                Name = x.Name,
                Slug = x.Slug,
                Count = x.PostTags.Count(t =>
                    t.BlogPost.DeletedAtUtc == null &&
                    t.BlogPost.Status == BlogPostStatusEnum.Published &&
                    t.BlogPost.PublishedAtUtc.HasValue &&
                    t.BlogPost.PublishedAtUtc <= nowUtc),
                IsActive = x.Slug == selectedSlug
            })
            .Take(24)
            .ToListAsync(cancellationToken);

        return tags.Where(x => x.Count > 0 || x.IsActive).ToList();
    }

    private async Task<bool> CategoriaValidaAsync(int? categoryId, CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue)
        {
            return true;
        }

        return await dbContext.BlogCategories
            .AnyAsync(x => x.Id == categoryId.Value && x.IsActive, cancellationToken);
    }

    private async Task<bool> SlugExisteAsync(string slug, int? ignoreId, CancellationToken cancellationToken)
    {
        return await dbContext.BlogPosts.AnyAsync(
            x => x.Slug == slug &&
                 (!ignoreId.HasValue || x.Id != ignoreId.Value),
            cancellationToken);
    }

    private async Task<string> GarantirSlugUnicoAsync(string baseSlug, int? ignoreId, CancellationToken cancellationToken)
    {
        var slug = baseSlug;
        var suffix = 2;

        while (await dbContext.BlogPosts.AnyAsync(
                   x => x.Slug == slug &&
                        (!ignoreId.HasValue || x.Id != ignoreId.Value),
                   cancellationToken))
        {
            slug = $"{baseSlug}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static List<string> ListarPendenciasPublicacao(
        string? summary,
        bool hasContent,
        UsuarioSistema? author,
        bool hasCover,
        int? categoryId)
    {
        var pendencias = new List<string>();

        if (string.IsNullOrWhiteSpace(summary))
        {
            pendencias.Add("resumo");
        }

        if (!hasContent)
        {
            pendencias.Add("conteúdo");
        }

        if (!categoryId.HasValue)
        {
            pendencias.Add("categoria");
        }

        if (author is null)
        {
            pendencias.Add("autor");
        }

        if (!hasCover)
        {
            pendencias.Add("imagem de capa");
        }

        return pendencias;
    }

    private static string FormatarListaPendencias(IReadOnlyList<string> pendencias)
    {
        return pendencias.Count switch
        {
            0 => "os campos obrigatórios",
            1 => pendencias[0],
            2 => $"{pendencias[0]} e {pendencias[1]}",
            _ => $"{string.Join(", ", pendencias.Take(pendencias.Count - 1))} e {pendencias[^1]}"
        };
    }

    private static string GerarSlug(string? slug, string fallback)
    {
        var source = string.IsNullOrWhiteSpace(slug) ? fallback : slug;
        var normalized = source.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(ch))
            {
                builder.Append(char.ToLowerInvariant(ch));
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var finalSlug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(finalSlug) ? "post" : finalSlug;
    }

    private static string? LimparOpcional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private string GerarConteudoHtmlSeguro(BlogPostFormViewModel model)
    {
        return !string.IsNullOrWhiteSpace(model.ContentHtmlInput)
            ? blogContentSanitizer.SanitizeHtml(model.ContentHtmlInput)
            : blogContentSanitizer.ConvertPlainTextToSafeHtml(model.ContentInput);
    }

    private string? ObterConteudoTexto(BlogPostFormViewModel model)
    {
        var contentText = blogContentSanitizer.ExtractPlainText(model.ContentInput);
        if (!string.IsNullOrWhiteSpace(contentText))
        {
            return contentText;
        }

        return ExtrairTextoDeHtml(model.ContentHtmlInput);
    }

    private static string? ExtrairTextoDeHtml(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return null;
        }

        var semScripts = Regex.Replace(html, "<(script|style)\\b[^>]*>.*?</\\1>", " ", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        var comEspacos = Regex.Replace(semScripts, "</?(p|div|br|li|h[1-6]|blockquote|tr|td|th)\\b[^>]*>", " ", RegexOptions.IgnoreCase);
        var semTags = Regex.Replace(comEspacos, "<[^>]+>", " ");
        var decodificado = WebUtility.HtmlDecode(semTags);
        var normalizado = Regex.Replace(decodificado, "\\s+", " ").Trim();

        return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
    }

    private static bool ConteudoHtmlTemMidia(string? html)
    {
        return !string.IsNullOrWhiteSpace(html) &&
               Regex.IsMatch(html, "<(img|iframe)\\b", RegexOptions.IgnoreCase);
    }

    private string? SanitizarHtmlParaValidacao(string? html)
    {
        return string.IsNullOrWhiteSpace(html)
            ? null
            : blogContentSanitizer.SanitizeHtml(html);
    }

    private static int CalcularTempoLeitura(string? contentText)
    {
        if (string.IsNullOrWhiteSpace(contentText))
        {
            return 0;
        }

        var wordCount = contentText
            .Split([' ', '\r', '\n', '\t'], StringSplitOptions.RemoveEmptyEntries)
            .Length;

        return Math.Max(1, (int)Math.Ceiling(wordCount / 200m));
    }

    private static List<string> ParseTags(string? tagsInput)
    {
        return (tagsInput ?? string.Empty)
            .Split([',', ';', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(12)
            .ToList();
    }

    private static DateTime ConvertSaoPauloLocalToUtc(DateTime localDateTime)
    {
        var timeZone = GetSaoPauloTimeZone();
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }

    private static DateTime? ConvertUtcToSaoPauloLocal(DateTime? utcDateTime)
    {
        if (!utcDateTime.HasValue)
        {
            return null;
        }

        var timeZone = GetSaoPauloTimeZone();
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcDateTime.Value, DateTimeKind.Utc), timeZone);
    }

    private static DateTime ConvertSaoPauloDateOnlyToUtcStart(DateOnly date)
    {
        return ConvertSaoPauloLocalToUtc(date.ToDateTime(TimeOnly.MinValue));
    }

    private static DateTime ConvertSaoPauloDateOnlyToUtcEndExclusive(DateOnly date)
    {
        return ConvertSaoPauloLocalToUtc(date.AddDays(1).ToDateTime(TimeOnly.MinValue));
    }

    private static TimeZoneInfo GetSaoPauloTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
        }
    }

    private static BlogPublicPostSelection EscolherMelhorVersaoPublica(
        IEnumerable<BlogPublicPostSelection> versions,
        string languageCode)
    {
        var normalizedLanguage = NormalizarIdiomaBlog(languageCode);

        return versions
            .OrderByDescending(x => string.Equals(NormalizarIdiomaBlog(x.LanguageCode), normalizedLanguage, StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(x => string.Equals(NormalizarIdiomaBlog(x.LanguageCode), DefaultBlogLanguageCode, StringComparison.OrdinalIgnoreCase))
            .ThenBy(x => x.Id == x.GroupId ? 0 : 1)
            .ThenByDescending(x => x.PublishedAtUtc ?? x.CreatedAtUtc)
            .First();
    }

    private static string ObterIdiomaAtualBlog()
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        return NormalizarIdiomaBlog(culture);
    }

    private static string NormalizarIdiomaBlog(string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return DefaultBlogLanguageCode;
        }

        var normalized = languageCode.Trim();
        var exact = SupportedBlogLanguages.FirstOrDefault(x =>
            string.Equals(x.Code, normalized, StringComparison.OrdinalIgnoreCase));

        if (exact is not null)
        {
            return exact.Code;
        }

        var prefix = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        var byPrefix = SupportedBlogLanguages.FirstOrDefault(x =>
            x.Code.StartsWith($"{prefix}-", StringComparison.OrdinalIgnoreCase));

        return byPrefix?.Code ?? DefaultBlogLanguageCode;
    }

    private static BlogLanguageDefinition? ObterDefinicaoIdioma(string? languageCode)
    {
        var normalized = NormalizarIdiomaBlog(languageCode);
        return SupportedBlogLanguages.FirstOrDefault(x =>
            string.Equals(x.Code, normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string ObterRotuloIdioma(string? languageCode)
    {
        return ObterDefinicaoIdioma(languageCode)?.Label ?? "Português";
    }

    private sealed record BlogLanguageDefinition(
        string Code,
        string Label,
        string NativeLabel,
        string SlugSuffix,
        bool IsDefault);

    private sealed record BlogPublicPostSelection(
        int Id,
        int GroupId,
        string LanguageCode,
        DateTime? PublishedAtUtc,
        DateTime CreatedAtUtc);

    private sealed record BlogWorkflowValidation(
        bool Success,
        string Message,
        BlogPostStatusEnum Status,
        DateTime? PublishedAtUtc,
        DateTime? ScheduledAtUtc,
        DateTime? ArchivedAtUtc)
    {
        public static BlogWorkflowValidation Ok(
            BlogPostStatusEnum status,
            DateTime? publishedAtUtc,
            DateTime? scheduledAtUtc,
            DateTime? archivedAtUtc)
            => new(true, string.Empty, status, publishedAtUtc, scheduledAtUtc, archivedAtUtc);

        public static BlogWorkflowValidation Fail(string message)
            => new(false, message, BlogPostStatusEnum.Draft, null, null, null);
    }
}
