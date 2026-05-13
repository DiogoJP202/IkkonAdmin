using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class BlogPostTagConfiguration : IEntityTypeConfiguration<BlogPostTag>
{
    public void Configure(EntityTypeBuilder<BlogPostTag> builder)
    {
        builder.ToTable("BlogPostTags");
        builder.HasKey(x => new { x.BlogPostId, x.BlogTagId });

        builder.HasIndex(x => x.BlogTagId);

        builder.HasOne(x => x.BlogPost)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.BlogPostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.BlogTag)
            .WithMany(x => x.PostTags)
            .HasForeignKey(x => x.BlogTagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
