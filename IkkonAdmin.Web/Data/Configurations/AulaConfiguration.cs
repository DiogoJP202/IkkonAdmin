using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class AulaConfiguration : IEntityTypeConfiguration<Aula>
{
    public void Configure(EntityTypeBuilder<Aula> builder)
    {
        builder.ToTable("Aulas");
        builder.HasIndex(x => new { x.TurmaId, x.Inicio });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => new { x.TurmaHorarioId, x.DataOcorrenciaRecorrencia })
            .IsUnique()
            .HasFilter("[TurmaHorarioId] IS NOT NULL AND [DataOcorrenciaRecorrencia] IS NOT NULL");

        builder.Property(x => x.Inicio).HasColumnType("datetime2");
        builder.Property(x => x.Fim).HasColumnType("datetime2");
        builder.Property(x => x.DataOcorrenciaRecorrencia).HasColumnType("date");
        builder.Property(x => x.Local).HasMaxLength(150);

        builder.HasOne(x => x.Turma)
            .WithMany(x => x.Aulas)
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.TurmaHorario)
            .WithMany(x => x.Aulas)
            .HasForeignKey(x => x.TurmaHorarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.InstrutorUsuario)
            .WithMany(x => x.AulasComoInstrutor)
            .HasForeignKey(x => x.InstrutorUsuarioId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
