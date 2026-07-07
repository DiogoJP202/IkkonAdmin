using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class TurmaInstrutorConfiguration : IEntityTypeConfiguration<TurmaInstrutor>
{
    public void Configure(EntityTypeBuilder<TurmaInstrutor> builder)
    {
        builder.ToTable("TurmaInstrutores");
        builder.HasIndex(x => new { x.TurmaId, x.UsuarioSistemaId, x.DataInicio });

        builder.Property(x => x.DataInicio).HasColumnType("date");
        builder.Property(x => x.DataFim).HasColumnType("date");

        builder.HasOne(x => x.Turma)
            .WithMany(x => x.Instrutores)
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.UsuarioSistema)
            .WithMany(x => x.TurmasComoInstrutor)
            .HasForeignKey(x => x.UsuarioSistemaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
