using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class HistoricoAlunoConfiguration : IEntityTypeConfiguration<HistoricoAluno>
{
    public void Configure(EntityTypeBuilder<HistoricoAluno> builder)
    {
        builder.ToTable("HistoricoAlunos");
        builder.Property(x => x.TipoEvento).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(500).IsRequired();

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Historicos)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
