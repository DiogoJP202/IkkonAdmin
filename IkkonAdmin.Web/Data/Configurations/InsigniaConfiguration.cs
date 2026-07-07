using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class InsigniaConfiguration : IEntityTypeConfiguration<Insignia>
{
    public void Configure(EntityTypeBuilder<Insignia> builder)
    {
        builder.ToTable("Insignias");
        builder.HasIndex(x => x.Nome).IsUnique();

        builder.Property(x => x.Nome).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.Property(x => x.Icone).HasMaxLength(80);
        builder.Property(x => x.Categoria).HasMaxLength(80);
        builder.Property(x => x.RegraAutomatica).HasMaxLength(120);
    }
}
