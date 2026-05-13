using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class BlogTag
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Slug { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<BlogPostTag> PostTags { get; set; } = new List<BlogPostTag>();
}
