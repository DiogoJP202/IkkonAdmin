using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class EventoAlunoPortalConfiguration : IEntityTypeConfiguration<EventoAlunoPortal>
{
    public void Configure(EntityTypeBuilder<EventoAlunoPortal> builder)
    {
        builder.ToTable("EventosAlunoPortal");
        builder.HasIndex(x => new { x.Ativo, x.Inicio });
        builder.HasIndex(x => x.Tipo);

        builder.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(2000);
        builder.Property(x => x.Local).HasMaxLength(180);
        builder.Property(x => x.GoogleEventoId).HasMaxLength(200);
        builder.Property(x => x.Inicio).HasColumnType("datetime2");
        builder.Property(x => x.Fim).HasColumnType("datetime2");
    }
}
