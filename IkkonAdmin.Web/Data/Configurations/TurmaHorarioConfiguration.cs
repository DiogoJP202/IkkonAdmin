using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class TurmaHorarioConfiguration : IEntityTypeConfiguration<TurmaHorario>
{
    public void Configure(EntityTypeBuilder<TurmaHorario> builder)
    {
        builder.ToTable("TurmaHorarios");
        builder.HasIndex(x => new { x.TurmaId, x.DiaSemana, x.HoraInicio });

        builder.Property(x => x.HoraInicio).HasColumnType("time");
        builder.Property(x => x.HoraFim).HasColumnType("time");
        builder.Property(x => x.Local).HasMaxLength(150);

        builder.HasOne(x => x.Turma)
            .WithMany(x => x.Horarios)
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
