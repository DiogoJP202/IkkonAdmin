using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class UsuarioPermissaoConfiguration : IEntityTypeConfiguration<UsuarioPermissao>
{
    public void Configure(EntityTypeBuilder<UsuarioPermissao> builder)
    {
        builder.ToTable("UsuariosPermissoes");
        builder.HasKey(x => new { x.UsuarioId, x.PermissaoId });

        builder.HasIndex(x => x.PermissaoId);
        builder.Property(x => x.DataConcessaoUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.UsuarioPermissoes)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permissao)
            .WithMany(x => x.UsuarioPermissoes)
            .HasForeignKey(x => x.PermissaoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
