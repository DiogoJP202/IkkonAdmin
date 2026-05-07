using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class GoogleAgendaConexaoConfiguration : IEntityTypeConfiguration<GoogleAgendaConexao>
{
    public void Configure(EntityTypeBuilder<GoogleAgendaConexao> builder)
    {
        builder.ToTable("GoogleAgendaConexoes");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Ativa);
        builder.HasIndex(x => x.ContaEmail);

        builder.Property(x => x.ContaEmail).HasMaxLength(180);
        builder.Property(x => x.RefreshTokenProtegido).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Escopos).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CriadoEmUtc).HasColumnType("datetime2");
        builder.Property(x => x.AtualizadoEmUtc).HasColumnType("datetime2");
        builder.Property(x => x.Ativa).HasDefaultValue(true);

        builder.HasOne(x => x.ConectadoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.ConectadoPorUsuarioId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
