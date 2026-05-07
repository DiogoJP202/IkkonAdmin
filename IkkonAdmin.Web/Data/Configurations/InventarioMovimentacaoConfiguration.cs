using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class InventarioMovimentacaoConfiguration : IEntityTypeConfiguration<InventarioMovimentacao>
{
    public void Configure(EntityTypeBuilder<InventarioMovimentacao> builder)
    {
        builder.ToTable("InventarioMovimentacoes");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.InventarioItemId);
        builder.HasIndex(x => x.GoogleEventId);
        builder.HasIndex(x => x.TipoMovimentacao);
        builder.HasIndex(x => x.DataInicioUtc);

        builder.Property(x => x.GoogleEventId).HasMaxLength(160);
        builder.Property(x => x.Observacoes).HasMaxLength(800);
        builder.Property(x => x.DataInicioUtc).HasColumnType("datetime2");
        builder.Property(x => x.DataFimUtc).HasColumnType("datetime2");
        builder.Property(x => x.CriadoEmUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.InventarioItem)
            .WithMany(x => x.Movimentacoes)
            .HasForeignKey(x => x.InventarioItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ResponsavelUsuario)
            .WithMany()
            .HasForeignKey(x => x.ResponsavelUsuarioId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
