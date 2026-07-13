using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.ViewModels;

public class BlogAdminFilterViewModel
{
    public string? Busca { get; set; }
    public BlogPostStatusEnum? Status { get; set; }
    public int? CategoryId { get; set; }
    public int? AuthorUserId { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsWeeklyHighlight { get; set; }
    public DateOnly? PublishedFrom { get; set; }
    public DateOnly? PublishedTo { get; set; }
}

public class BlogAdminIndexViewModel
{
    public BlogAdminFilterViewModel Filtro { get; set; } = new();
    public int TotalPosts { get; set; }
    public int DraftCount { get; set; }
    public int ScheduledCount { get; set; }
    public int PublishedCount { get; set; }
    public int ArchivedCount { get; set; }
    public List<BlogPostListItemViewModel> Posts { get; set; } = new();
    public List<BlogCategorySelectItemViewModel> Categories { get; set; } = new();
    public List<BlogAuthorSelectItemViewModel> Authors { get; set; } = new();
}

public class BlogPostListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "pt-BR";
    public string LanguageLabel { get; set; } = "Português";
    public int TranslationVersionCount { get; set; }
    public BlogPostStatusEnum Status { get; set; }
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsWeeklyHighlight { get; set; }
    public int TagCount { get; set; }
}

public class BlogAuthorSelectItemViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

public class BlogCategorySelectItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class BlogPostFormViewModel
{
    public int? Id { get; set; }
    public string LanguageCode { get; set; } = "pt-BR";
    public string LanguageLabel { get; set; } = "Português";
    public int? TranslationGroupId { get; set; }

    [Display(Name = "Título")]
    [Required(ErrorMessage = "Informe o título do post.")]
    [StringLength(180, ErrorMessage = "Título deve ter no máximo 180 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Slug")]
    [StringLength(180, ErrorMessage = "Slug deve ter no máximo 180 caracteres.")]
    public string? Slug { get; set; }

    [Display(Name = "Resumo")]
    [StringLength(500, ErrorMessage = "Resumo deve ter no máximo 500 caracteres.")]
    public string? Summary { get; set; }

    [Display(Name = "Conteúdo")]
    public string? ContentInput { get; set; }

    public string? ContentHtmlInput { get; set; }
    public string? ContentJsonInput { get; set; }

    [Display(Name = "Categoria")]
    public int? CategoryId { get; set; }

    [Display(Name = "Autor responsavel")]
    public int? AuthorUserId { get; set; }

    [Display(Name = "Destaque")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Blog da semana")]
    public bool IsWeeklyHighlight { get; set; }

    [Display(Name = "Data de publicação/agendamento")]
    public DateTime? PublicationDateLocal { get; set; }

    [Display(Name = "Meta title")]
    [StringLength(180, ErrorMessage = "Meta title deve ter no máximo 180 caracteres.")]
    public string? SeoTitle { get; set; }

    [Display(Name = "Meta description")]
    [StringLength(320, ErrorMessage = "Meta description deve ter no máximo 320 caracteres.")]
    public string? SeoDescription { get; set; }

    [Display(Name = "Tags")]
    [StringLength(500, ErrorMessage = "Tags devem ter no máximo 500 caracteres.")]
    public string? TagsInput { get; set; }

    [Display(Name = "Imagem de capa")]
    public IFormFile? CoverImage { get; set; }

    [Display(Name = "Remover capa atual")]
    public bool RemoveCoverImage { get; set; }

    public string? CurrentCoverImageUrl { get; set; }
    public BlogPostStatusEnum CurrentStatus { get; set; } = BlogPostStatusEnum.Draft;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public string SubmissionAction { get; set; } = "Draft";

    public List<BlogCategorySelectItemViewModel> CategoryOptions { get; set; } = new();
    public List<BlogAuthorSelectItemViewModel> AuthorOptions { get; set; } = new();
    public List<string> TagSuggestions { get; set; } = new();
}

public class BlogVersionOverviewViewModel
{
    public int SourcePostId { get; set; }
    public int TranslationGroupId { get; set; }
    public string SourceTitle { get; set; } = string.Empty;
    public string SourceLanguageCode { get; set; } = "pt-BR";
    public List<BlogVersionItemViewModel> Versions { get; set; } = new();
}

public class BlogVersionItemViewModel
{
    public string LanguageCode { get; set; } = "pt-BR";
    public string LanguageLabel { get; set; } = string.Empty;
    public string NativeLabel { get; set; } = string.Empty;
    public string SlugSuffix { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsCurrent { get; set; }
    public bool Exists => PostId.HasValue;
    public int? PostId { get; set; }
    public string? Title { get; set; }
    public string? Slug { get; set; }
    public BlogPostStatusEnum? Status { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
}

public class BlogPreviewViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? CategoryName { get; set; }
    public string? AuthorName { get; set; }
    public BlogPostStatusEnum Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsWeeklyHighlight { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class BlogCategoryIndexViewModel
{
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
    public int InactiveCategories { get; set; }
    public List<BlogCategoryListItemViewModel> Categories { get; set; } = new();
}

public class BlogCategoryListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int TotalPosts { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class BlogCategoryFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Informe o nome da categoria.")]
    [StringLength(120, ErrorMessage = "Nome deve ter no máximo 120 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Slug")]
    [StringLength(140, ErrorMessage = "Slug deve ter no máximo 140 caracteres.")]
    public string? Slug { get; set; }

    [Display(Name = "Descrição")]
    [StringLength(400, ErrorMessage = "Descrição deve ter no máximo 400 caracteres.")]
    public string? Description { get; set; }

    [Display(Name = "Categoria ativa")]
    public bool IsActive { get; set; } = true;
}

public sealed record BlogOperationResult(bool Success, string Message, int? EntityId = null)
{
    public static BlogOperationResult Ok(string message, int? entityId = null) => new(true, message, entityId);
    public static BlogOperationResult Fail(string message) => new(false, message);
}

public sealed record BlogMediaSaveResult(bool Success, string Message, string? PublicUrl = null)
{
    public static BlogMediaSaveResult Ok(string publicUrl) => new(true, "Imagem salva com sucesso.", publicUrl);
    public static BlogMediaSaveResult Fail(string message) => new(false, message);
}
