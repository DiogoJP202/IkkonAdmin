using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class AlunoTurmaConfiguration : IEntityTypeConfiguration<AlunoTurma>
{
    public void Configure(EntityTypeBuilder<AlunoTurma> builder)
    {
        builder.ToTable("AlunosTurmas");

        builder.HasKey(x => new { x.AlunoId, x.TurmaId });

        builder.Property(x => x.DataVinculo)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.AlunoTurmas)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Turma)
            .WithMany(x => x.AlunoTurmas)
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
