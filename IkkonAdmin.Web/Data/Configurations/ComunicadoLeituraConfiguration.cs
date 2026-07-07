using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class ComunicadoLeituraConfiguration : IEntityTypeConfiguration<ComunicadoLeitura>
{
    public void Configure(EntityTypeBuilder<ComunicadoLeitura> builder)
    {
        builder.ToTable("ComunicadosLeituras");
        builder.HasKey(x => new { x.ComunicadoId, x.AlunoId });
        builder.Property(x => x.LidoEmUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.Comunicado)
            .WithMany(x => x.Leituras)
            .HasForeignKey(x => x.ComunicadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.ComunicadosLidos)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
