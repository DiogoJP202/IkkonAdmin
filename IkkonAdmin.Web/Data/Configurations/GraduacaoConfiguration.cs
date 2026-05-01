using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class GraduacaoConfiguration : IEntityTypeConfiguration<Graduacao>
{
    public void Configure(EntityTypeBuilder<Graduacao> builder)
    {
        builder.ToTable("Graduacoes");
        builder.Property(x => x.DataResultado).HasColumnType("date");

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Graduacoes)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ExameGraduacao)
            .WithMany(x => x.Graduacoes)
            .HasForeignKey(x => x.ExameGraduacaoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
