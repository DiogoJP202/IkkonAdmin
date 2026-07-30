namespace IkkonAdmin.Web.Models.ViewModels;

public class BlogPublicFilterViewModel
{
    public string? Q { get; set; }
    public string? Categoria { get; set; }
    public string? Tag { get; set; }
    public int Pagina { get; set; } = 1;
}

public class BlogPublicIndexViewModel
{
    public BlogPublicFilterViewModel Filtro { get; set; } = new();
    public List<BlogPublicPostCardViewModel> FeaturedPosts { get; set; } = new();
    public List<BlogPublicPostCardViewModel> WeeklyHighlights { get; set; } = new();
    public List<BlogPublicPostCardViewModel> Posts { get; set; } = new();
    public List<BlogPublicTaxonomyItemViewModel> Categories { get; set; } = new();
    public List<BlogPublicTaxonomyItemViewModel> Tags { get; set; } = new();
    public int TotalPosts { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 9;
    public int TotalPages { get; set; } = 1;
    public bool HasActiveFilter => !string.IsNullOrWhiteSpace(Filtro.Q) ||
                                   !string.IsNullOrWhiteSpace(Filtro.Categoria) ||
                                   !string.IsNullOrWhiteSpace(Filtro.Tag);
}

public class BlogPublicDetailsViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string ContentHtml { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string? AuthorName { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public string? SeoTitle { get; set; }
    public string? SeoDescription { get; set; }
    public string LanguageCode { get; set; } = "pt-BR";
    public DateTime UpdatedAtUtc { get; set; }
    public List<BlogPublicTagViewModel> Tags { get; set; } = new();
    public List<BlogPublicPostCardViewModel> RelatedPosts { get; set; } = new();
    public List<BlogPublicAlternateVersionViewModel> AlternateVersions { get; set; } = new();
}

public class BlogPublicAlternateVersionViewModel
{
    public string LanguageCode { get; set; } = "pt-BR";
    public string Slug { get; set; } = string.Empty;
}

public class BlogPublicPostCardViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? AuthorName { get; set; }
    public string? CategoryName { get; set; }
    public string? CategorySlug { get; set; }
    public DateTime PublishedAtUtc { get; set; }
    public int ReadingTimeMinutes { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsWeeklyHighlight { get; set; }
    public List<BlogPublicTagViewModel> Tags { get; set; } = new();
}

public class BlogPublicTagViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class BlogPublicTaxonomyItemViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int Count { get; set; }
    public bool IsActive { get; set; }
}
