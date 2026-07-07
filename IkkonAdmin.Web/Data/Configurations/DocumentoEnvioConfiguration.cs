using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class DocumentoEnvioConfiguration : IEntityTypeConfiguration<DocumentoEnvio>
{
    public void Configure(EntityTypeBuilder<DocumentoEnvio> builder)
    {
        builder.ToTable("DocumentoEnvios");
        builder.HasIndex(x => x.DocumentoSolicitacaoId);

        builder.Property(x => x.ArquivoUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.NomeArquivoOriginal).HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(120);
        builder.Property(x => x.EnviadoEmUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.DocumentoSolicitacao)
            .WithMany(x => x.Envios)
            .HasForeignKey(x => x.DocumentoSolicitacaoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.EnviadoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.EnviadoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
