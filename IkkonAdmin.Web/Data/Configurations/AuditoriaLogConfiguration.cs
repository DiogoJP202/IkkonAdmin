using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class AuditoriaLogConfiguration : IEntityTypeConfiguration<AuditoriaLog>
{
    public void Configure(EntityTypeBuilder<AuditoriaLog> builder)
    {
        builder.ToTable("AuditoriaLogs");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DataEventoUtc);
        builder.HasIndex(x => x.UsuarioResponsavelId);
        builder.HasIndex(x => x.UsuarioAfetadoId);
        builder.HasIndex(x => new { x.Entidade, x.Acao });

        builder.Property(x => x.Acao).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Entidade).HasMaxLength(80).IsRequired();
        builder.Property(x => x.EntidadeId).HasMaxLength(80);
        builder.Property(x => x.Descricao).HasMaxLength(400);
        builder.Property(x => x.EnderecoIp).HasMaxLength(64);
        builder.Property(x => x.DataEventoUtc).HasColumnType("datetime2");
    }
}
