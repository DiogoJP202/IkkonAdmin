using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.ToTable("BlogCategories");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.IsActive);

        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(140).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(400);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.UpdatedAtUtc).HasColumnType("datetime2");
    }
}
