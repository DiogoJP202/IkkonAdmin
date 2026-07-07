using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class FrequenciaAlunoConfiguration : IEntityTypeConfiguration<FrequenciaAluno>
{
    public void Configure(EntityTypeBuilder<FrequenciaAluno> builder)
    {
        builder.ToTable("FrequenciasAlunos");
        builder.HasIndex(x => new { x.AulaId, x.AlunoId }).IsUnique();
        builder.HasIndex(x => new { x.AlunoId, x.Status });

        builder.Property(x => x.Justificativa).HasMaxLength(500);
        builder.Property(x => x.RegistradoEmUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.Aula)
            .WithMany(x => x.Frequencias)
            .HasForeignKey(x => x.AulaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Frequencias)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.RegistradoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.RegistradoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
