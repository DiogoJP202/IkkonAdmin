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

    [Display(Name = "Titulo")]
    [Required(ErrorMessage = "Informe o titulo do post.")]
    [StringLength(180, ErrorMessage = "Titulo deve ter no maximo 180 caracteres.")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Slug")]
    [StringLength(180, ErrorMessage = "Slug deve ter no maximo 180 caracteres.")]
    public string? Slug { get; set; }

    [Display(Name = "Resumo")]
    [StringLength(500, ErrorMessage = "Resumo deve ter no maximo 500 caracteres.")]
    public string? Summary { get; set; }

    [Display(Name = "Conteudo")]
    public string? ContentInput { get; set; }

    [Display(Name = "Categoria")]
    public int? CategoryId { get; set; }

    [Display(Name = "Autor responsavel")]
    public int? AuthorUserId { get; set; }

    [Display(Name = "Destaque")]
    public bool IsFeatured { get; set; }

    [Display(Name = "Blog da semana")]
    public bool IsWeeklyHighlight { get; set; }

    [Display(Name = "Data de publicacao/agendamento")]
    public DateTime? PublicationDateLocal { get; set; }

    [Display(Name = "Meta title")]
    [StringLength(180, ErrorMessage = "Meta title deve ter no maximo 180 caracteres.")]
    public string? SeoTitle { get; set; }

    [Display(Name = "Meta description")]
    [StringLength(320, ErrorMessage = "Meta description deve ter no maximo 320 caracteres.")]
    public string? SeoDescription { get; set; }

    [Display(Name = "Tags")]
    [StringLength(500, ErrorMessage = "Tags devem ter no maximo 500 caracteres.")]
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
    [StringLength(120, ErrorMessage = "Nome deve ter no maximo 120 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Slug")]
    [StringLength(140, ErrorMessage = "Slug deve ter no maximo 140 caracteres.")]
    public string? Slug { get; set; }

    [Display(Name = "Descricao")]
    [StringLength(400, ErrorMessage = "Descricao deve ter no maximo 400 caracteres.")]
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
