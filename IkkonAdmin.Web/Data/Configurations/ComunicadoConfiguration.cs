using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class ComunicadoConfiguration : IEntityTypeConfiguration<Comunicado>
{
    public void Configure(EntityTypeBuilder<Comunicado> builder)
    {
        builder.ToTable("Comunicados");
        builder.HasIndex(x => new { x.Ativo, x.PublicadoEmUtc, x.ExpiraEmUtc });

        builder.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Conteudo).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.PublicadoEmUtc).HasColumnType("datetime2");
        builder.Property(x => x.ExpiraEmUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
