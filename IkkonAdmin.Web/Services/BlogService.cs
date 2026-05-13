using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class BlogService(
    ApplicationDbContext dbContext,
    IBlogMediaService blogMediaService) : IBlogService
{
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
            Title = post.Title,
            Slug = post.Slug,
            Summary = post.Summary,
            ContentInput = post.ContentText,
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
                ContentHtml = x.ContentHtml ?? "<p>Conteudo ainda nao informado.</p>",
                Tags = x.PostTags
                    .OrderBy(t => t.BlogTag.Name)
                    .Select(t => t.BlogTag.Name)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
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
        var contentText = NormalizarConteudoTexto(model.ContentInput);
        var contentHtml = GerarHtmlSeguroBasico(model.ContentInput);
        var slug = await GarantirSlugUnicoAsync(GerarSlug(model.Slug, model.Title), null, cancellationToken);

        var post = new BlogPost
        {
            Title = model.Title.Trim(),
            Slug = slug,
            Summary = summary,
            ContentHtml = contentHtml,
            ContentJson = null,
            ContentText = contentText,
            CoverImageUrl = coverImageUrl,
            AuthorUserId = author?.Id ?? usuarioAtualId,
            AuthorDisplayName = author?.NomeExibicao ?? author?.Login ?? "Equipe Ikkon",
            CategoryId = model.CategoryId,
            Status = validacao.Status,
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
            return BlogOperationResult.Fail("Post nao encontrado.");
        }

        var author = await ObterAutorValidoAsync(model.AuthorUserId, cancellationToken);
        var validacao = await ValidarFluxoEditorialAsync(
            model,
            author,
            post.CoverImageUrl,
            post.Status,
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
        var contentText = NormalizarConteudoTexto(model.ContentInput);
        var contentHtml = GerarHtmlSeguroBasico(model.ContentInput);
        var slug = await GarantirSlugUnicoAsync(GerarSlug(model.Slug, model.Title), id, cancellationToken);

        post.Title = model.Title.Trim();
        post.Slug = slug;
        post.Summary = summary;
        post.ContentHtml = contentHtml;
        post.ContentJson = null;
        post.ContentText = contentText;
        post.CoverImageUrl = coverImageUrl;
        post.AuthorUserId = author?.Id ?? usuarioAtualId;
        post.AuthorDisplayName = author?.NomeExibicao ?? author?.Login ?? post.AuthorDisplayName;
        post.CategoryId = model.CategoryId;
        post.Status = validacao.Status;
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
            return BlogOperationResult.Fail("Post nao encontrado.");
        }

        post.DeletedAtUtc = DateTime.UtcNow;
        post.UpdatedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return BlogOperationResult.Ok("Post excluido com sucesso.", post.Id);
    }

    private async Task<BlogWorkflowValidation> ValidarFluxoEditorialAsync(
        BlogPostFormViewModel model,
        UsuarioSistema? author,
        string? currentCoverUrl,
        BlogPostStatusEnum currentStatus,
        CancellationToken cancellationToken)
    {
        var acao = (model.SubmissionAction ?? "Draft").Trim().ToLowerInvariant();
        var publicationDateUtc = model.PublicationDateLocal.HasValue
            ? ConvertSaoPauloLocalToUtc(model.PublicationDateLocal.Value)
            : (DateTime?)null;
        var now = DateTime.UtcNow;
        var summary = LimparOpcional(model.Summary);
        var contentText = NormalizarConteudoTexto(model.ContentInput);
        var hasCover = model.CoverImage is not null ||
                       (!model.RemoveCoverImage && !string.IsNullOrWhiteSpace(currentCoverUrl));

        if (!await CategoriaValidaAsync(model.CategoryId, cancellationToken))
        {
            return BlogWorkflowValidation.Fail("Selecione uma categoria ativa.");
        }

        if (await dbContext.BlogPosts.AnyAsync(
                x => x.Id != model.Id &&
                     x.Slug == GerarSlug(model.Slug, model.Title),
                cancellationToken))
        {
            return BlogWorkflowValidation.Fail("Ja existe um post com esse slug.");
        }

        switch (acao)
        {
            case "publish":
                if (publicationDateUtc.HasValue && publicationDateUtc.Value > now)
                {
                    return BlogWorkflowValidation.Fail("Para data futura, use a acao Agendar.");
                }

                if (!PodePublicar(summary, contentText, author, hasCover, model.CategoryId))
                {
                    return BlogWorkflowValidation.Fail("Para publicar, informe resumo, conteudo, categoria, autor e imagem de capa.");
                }

                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Published,
                    publicationDateUtc ?? now,
                    null,
                    null);

            case "schedule":
                if (!publicationDateUtc.HasValue || publicationDateUtc.Value <= now)
                {
                    return BlogWorkflowValidation.Fail("Informe uma data futura para agendar a publicacao.");
                }

                if (!PodePublicar(summary, contentText, author, hasCover, model.CategoryId))
                {
                    return BlogWorkflowValidation.Fail("Para agendar, informe resumo, conteudo, categoria, autor e imagem de capa.");
                }

                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Scheduled,
                    null,
                    publicationDateUtc,
                    null);

            case "archive":
                return BlogWorkflowValidation.Ok(
                    BlogPostStatusEnum.Archived,
                    currentStatus == BlogPostStatusEnum.Published ? publicationDateUtc ?? now : publicationDateUtc,
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

    private async Task<bool> CategoriaValidaAsync(int? categoryId, CancellationToken cancellationToken)
    {
        if (!categoryId.HasValue)
        {
            return true;
        }

        return await dbContext.BlogCategories
            .AnyAsync(x => x.Id == categoryId.Value && x.IsActive, cancellationToken);
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

    private static bool PodePublicar(
        string? summary,
        string? contentText,
        UsuarioSistema? author,
        bool hasCover,
        int? categoryId)
    {
        return !string.IsNullOrWhiteSpace(summary) &&
               !string.IsNullOrWhiteSpace(contentText) &&
               hasCover &&
               categoryId.HasValue &&
               author is not null;
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

    private static string? NormalizarConteudoTexto(string? content)
    {
        var trimmed = content?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        return string.Join(
            Environment.NewLine + Environment.NewLine,
            trimmed
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(x => x.Trim()));
    }

    private static string GerarHtmlSeguroBasico(string? content)
    {
        var text = NormalizarConteudoTexto(content);
        if (string.IsNullOrWhiteSpace(text))
        {
            return "<p>Conteudo ainda nao informado.</p>";
        }

        var paragraphs = text
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(paragraph =>
            {
                var encoded = HtmlEncoder.Default.Encode(paragraph.Trim());
                var withBreaks = encoded.Replace("\n", "<br />", StringComparison.Ordinal);
                return $"<p>{withBreaks}</p>";
            });

        return string.Join(Environment.NewLine, paragraphs);
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
