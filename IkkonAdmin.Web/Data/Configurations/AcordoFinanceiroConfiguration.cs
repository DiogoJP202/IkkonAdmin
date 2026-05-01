using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class AcordoFinanceiroConfiguration : IEntityTypeConfiguration<AcordoFinanceiro>
{
    public void Configure(EntityTypeBuilder<AcordoFinanceiro> builder)
    {
        builder.ToTable("AcordosFinanceiros");

        builder.Property(x => x.Descricao).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ValorMensalAcordado).HasPrecision(10, 2);
        builder.Property(x => x.InicioVigencia).HasColumnType("date");
        builder.Property(x => x.FimVigencia).HasColumnType("date");

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.AcordosFinanceiros)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
