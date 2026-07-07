using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class ComunicadoAlvoConfiguration : IEntityTypeConfiguration<ComunicadoAlvo>
{
    public void Configure(EntityTypeBuilder<ComunicadoAlvo> builder)
    {
        builder.ToTable("ComunicadosAlvos");
        builder.HasIndex(x => new { x.ComunicadoId, x.AlunoId, x.TurmaId, x.Todos });

        builder.HasOne(x => x.Comunicado)
            .WithMany(x => x.Alvos)
            .HasForeignKey(x => x.ComunicadoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Aluno)
            .WithMany()
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Turma)
            .WithMany()
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
