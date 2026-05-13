using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class BlogPost
{
    public int Id { get; set; }

    [Required, StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(180)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Summary { get; set; }

    public string? ContentHtml { get; set; }
    public string? ContentJson { get; set; }
    public string? ContentText { get; set; }

    [StringLength(300)]
    public string? CoverImageUrl { get; set; }

    public int? AuthorUserId { get; set; }
    public UsuarioSistema? AuthorUser { get; set; }

    [StringLength(200)]
    public string? AuthorDisplayName { get; set; }

    public int? CategoryId { get; set; }
    public BlogCategory? Category { get; set; }

    public BlogPostStatusEnum Status { get; set; } = BlogPostStatusEnum.Draft;
    public bool IsFeatured { get; set; }
    public bool IsWeeklyHighlight { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime? ScheduledAtUtc { get; set; }
    public DateTime? ArchivedAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    [StringLength(180)]
    public string? SeoTitle { get; set; }

    [StringLength(320)]
    public string? SeoDescription { get; set; }

    [Range(0, 10000)]
    public int ReadingTimeMinutes { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
}
