using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class InventarioItemConfiguration : IEntityTypeConfiguration<InventarioItem>
{
    public void Configure(EntityTypeBuilder<InventarioItem> builder)
    {
        builder.ToTable("InventarioItens");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CodigoInterno)
            .IsUnique()
            .HasFilter("[CodigoInterno] IS NOT NULL");
        builder.HasIndex(x => x.Categoria);
        builder.HasIndex(x => x.Tipo);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Ativo);
        builder.HasIndex(x => new { x.Categoria, x.Status, x.Ativo });

        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CodigoInterno).HasMaxLength(60);
        builder.Property(x => x.Tipo).HasMaxLength(80);
        builder.Property(x => x.Descricao).HasMaxLength(500);
        builder.Property(x => x.Localizacao).HasMaxLength(120);
        builder.Property(x => x.ValorEstimado).HasColumnType("decimal(12,2)");
        builder.Property(x => x.Observacoes).HasMaxLength(1000);
        builder.Property(x => x.CriadoEmUtc).HasColumnType("datetime2");
        builder.Property(x => x.AtualizadoEmUtc).HasColumnType("datetime2");
        builder.Property(x => x.Ativo).HasDefaultValue(true);

        builder.HasOne(x => x.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.CriadoPorUsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.AtualizadoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.AtualizadoPorUsuarioId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
