using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class BlogTagConfiguration : IEntityTypeConfiguration<BlogTag>
{
    public void Configure(EntityTypeBuilder<BlogTag> builder)
    {
        builder.ToTable("BlogTags");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.IsActive);

        builder.Property(x => x.Name).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2");
    }
}
