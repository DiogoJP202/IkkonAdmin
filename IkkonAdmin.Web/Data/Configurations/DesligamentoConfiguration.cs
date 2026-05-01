using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class DesligamentoConfiguration : IEntityTypeConfiguration<Desligamento>
{
    public void Configure(EntityTypeBuilder<Desligamento> builder)
    {
        builder.ToTable("Desligamentos");

        builder.Property(x => x.DataSolicitacao).HasColumnType("date");
        builder.Property(x => x.DataConfirmacao).HasColumnType("date");
        builder.Property(x => x.Motivo).HasMaxLength(400).IsRequired();
        builder.Property(x => x.PendenciaFinanceira).HasPrecision(10, 2);
        builder.Property(x => x.MultaRescisoria).HasPrecision(10, 2);

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Desligamentos)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
