using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class RolePermissaoConfiguration : IEntityTypeConfiguration<RolePermissao>
{
    public void Configure(EntityTypeBuilder<RolePermissao> builder)
    {
        builder.ToTable("RolesPermissoes");
        builder.HasKey(x => new { x.RoleId, x.PermissaoId });

        builder.HasIndex(x => x.PermissaoId);
        builder.Property(x => x.DataVinculoUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.Role)
            .WithMany(x => x.RolePermissoes)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permissao)
            .WithMany(x => x.RolePermissoes)
            .HasForeignKey(x => x.PermissaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
