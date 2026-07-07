using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class DocumentoSolicitacaoConfiguration : IEntityTypeConfiguration<DocumentoSolicitacao>
{
    public void Configure(EntityTypeBuilder<DocumentoSolicitacao> builder)
    {
        builder.ToTable("DocumentoSolicitacoes");
        builder.HasIndex(x => new { x.AlunoId, x.Status });
        builder.HasIndex(x => x.DataSolicitacaoUtc);

        builder.Property(x => x.DataSolicitacaoUtc).HasColumnType("datetime2");
        builder.Property(x => x.DataLimite).HasColumnType("date");
        builder.Property(x => x.ObservacaoAdministrativa).HasMaxLength(1000);

        builder.HasOne(x => x.DocumentoTipo)
            .WithMany(x => x.Solicitacoes)
            .HasForeignKey(x => x.DocumentoTipoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.DocumentosSolicitados)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SolicitadoPorUsuario)
            .WithMany()
            .HasForeignKey(x => x.SolicitadoPorUsuarioId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
