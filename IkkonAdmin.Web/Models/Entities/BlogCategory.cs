using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class BlogCategory
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(140)]
    public string Slug { get; set; } = string.Empty;

    [StringLength(400)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}
