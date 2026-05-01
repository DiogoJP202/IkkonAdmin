using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class RoleSistemaConfiguration : IEntityTypeConfiguration<RoleSistema>
{
    public void Configure(EntityTypeBuilder<RoleSistema> builder)
    {
        builder.ToTable("RolesSistema");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Codigo).IsUnique();
        builder.HasIndex(x => new { x.Ativo, x.IsSistema });
        builder.HasIndex(x => new { x.TipoAcesso, x.Ativo });

        builder.Property(x => x.Codigo).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(300);
        builder.Property(x => x.TipoAcesso)
            .HasConversion<int>()
            .HasDefaultValue(TipoAcessoEnum.Funcionario)
            .IsRequired();
        builder.Property(x => x.DataCriacaoUtc).HasColumnType("datetime2");
    }
}
