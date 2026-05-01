using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class PermissaoSistemaConfiguration : IEntityTypeConfiguration<PermissaoSistema>
{
    public void Configure(EntityTypeBuilder<PermissaoSistema> builder)
    {
        builder.ToTable("PermissoesSistema");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => new { x.Ativo, x.IsSistema });

        builder.Property(x => x.Codigo).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(140).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(400);
        builder.Property(x => x.DataCriacaoUtc).HasColumnType("datetime2");
    }
}
