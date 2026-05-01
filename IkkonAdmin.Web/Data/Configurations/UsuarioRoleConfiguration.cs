using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class UsuarioRoleConfiguration : IEntityTypeConfiguration<UsuarioRole>
{
    public void Configure(EntityTypeBuilder<UsuarioRole> builder)
    {
        builder.ToTable("UsuariosRoles");
        builder.HasKey(x => new { x.UsuarioId, x.RoleId });

        builder.HasIndex(x => x.RoleId);

        builder.Property(x => x.DataVinculoUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.UsuarioRoles)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UsuarioRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
