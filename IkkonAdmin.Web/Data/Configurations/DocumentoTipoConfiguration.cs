using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class DocumentoTipoConfiguration : IEntityTypeConfiguration<DocumentoTipo>
{
    public void Configure(EntityTypeBuilder<DocumentoTipo> builder)
    {
        builder.ToTable("DocumentoTipos");
        builder.HasIndex(x => x.Nome).IsUnique();

        builder.Property(x => x.Nome).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(500);
    }
}
