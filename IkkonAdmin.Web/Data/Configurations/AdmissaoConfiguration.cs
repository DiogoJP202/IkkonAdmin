using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class AdmissaoConfiguration : IEntityTypeConfiguration<Admissao>
{
    public void Configure(EntityTypeBuilder<Admissao> builder)
    {
        builder.ToTable("Admissoes");
        builder.Property(x => x.NomeInteressado).HasMaxLength(150).IsRequired();
        builder.Property(x => x.DataAulaExperimental).HasColumnType("date");
        builder.Property(x => x.DataMatricula).HasColumnType("date");

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Admissoes)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
