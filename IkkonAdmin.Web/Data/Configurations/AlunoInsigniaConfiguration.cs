using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class AlunoInsigniaConfiguration : IEntityTypeConfiguration<AlunoInsignia>
{
    public void Configure(EntityTypeBuilder<AlunoInsignia> builder)
    {
        builder.ToTable("AlunoInsignias");
        builder.HasIndex(x => new { x.AlunoId, x.InsigniaId }).IsUnique();
        builder.Property(x => x.ConcedidaEmUtc).HasColumnType("datetime2");
        builder.Property(x => x.Observacao).HasMaxLength(500);

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Insignias)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Insignia)
            .WithMany(x => x.Alunos)
            .HasForeignKey(x => x.InsigniaId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.ConcedidaPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.ConcedidaPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
