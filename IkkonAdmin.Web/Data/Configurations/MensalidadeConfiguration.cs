using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class MensalidadeConfiguration : IEntityTypeConfiguration<Mensalidade>
{
    public void Configure(EntityTypeBuilder<Mensalidade> builder)
    {
        builder.ToTable("Mensalidades");
        builder.HasIndex(x => new { x.AlunoId, x.Competencia }).IsUnique();

        builder.Property(x => x.Competencia).HasColumnType("date");
        builder.Property(x => x.DataVencimento).HasColumnType("date");
        builder.Property(x => x.DataPagamento).HasColumnType("date");
        builder.Property(x => x.ValorBase).HasPrecision(10, 2);
        builder.Property(x => x.ValorFinal).HasPrecision(10, 2);

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Mensalidades)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
