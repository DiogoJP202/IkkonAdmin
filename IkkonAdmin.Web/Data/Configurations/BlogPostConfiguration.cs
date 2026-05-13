using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.ToTable("BlogPosts");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.AuthorUserId);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.Status, x.PublishedAtUtc, x.DeletedAtUtc });
        builder.HasIndex(x => new { x.IsFeatured, x.PublishedAtUtc });
        builder.HasIndex(x => new { x.IsWeeklyHighlight, x.PublishedAtUtc });

        builder.Property(x => x.Title).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(500);
        builder.Property(x => x.CoverImageUrl).HasMaxLength(300);
        builder.Property(x => x.AuthorDisplayName).HasMaxLength(200);
        builder.Property(x => x.Status).HasDefaultValue(BlogPostStatusEnum.Draft);
        builder.Property(x => x.CreatedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.UpdatedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.PublishedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.ScheduledAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.ArchivedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.DeletedAtUtc).HasColumnType("datetime2");
        builder.Property(x => x.SeoTitle).HasMaxLength(180);
        builder.Property(x => x.SeoDescription).HasMaxLength(320);
        builder.Property(x => x.ReadingTimeMinutes).HasDefaultValue(0);

        builder.HasOne(x => x.AuthorUser)
            .WithMany()
            .HasForeignKey(x => x.AuthorUserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
