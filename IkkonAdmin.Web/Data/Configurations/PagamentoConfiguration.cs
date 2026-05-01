using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class PagamentoConfiguration : IEntityTypeConfiguration<Pagamento>
{
    public void Configure(EntityTypeBuilder<Pagamento> builder)
    {
        builder.ToTable("Pagamentos");
        builder.Property(x => x.ValorPago).HasPrecision(10, 2);

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Pagamentos)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Mensalidade)
            .WithMany(x => x.Pagamentos)
            .HasForeignKey(x => x.MensalidadeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
