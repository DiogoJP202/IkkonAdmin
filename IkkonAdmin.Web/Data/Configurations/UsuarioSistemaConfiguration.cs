using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class UsuarioSistemaConfiguration : IEntityTypeConfiguration<UsuarioSistema>
{
    public void Configure(EntityTypeBuilder<UsuarioSistema> builder)
    {
        builder.ToTable("UsuariosSistema");

        builder.HasIndex(x => x.LoginNormalizado).IsUnique();
        builder.HasIndex(x => x.EmailNormalizado).IsUnique();
        builder.HasIndex(x => new { x.TipoAcesso, x.Ativo });
        builder.HasIndex(x => x.AlunoId).IsUnique().HasFilter("[AlunoId] IS NOT NULL");

        builder.Property(x => x.Login).HasMaxLength(80).IsRequired();
        builder.Property(x => x.LoginNormalizado).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.EmailNormalizado).HasMaxLength(150);
        builder.Property(x => x.Telefone).HasMaxLength(30);
        builder.Property(x => x.FotoPerfilUrl).HasMaxLength(300);
        builder.Property(x => x.NomeExibicao).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SenhaHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.TemaPreferencia)
            .HasDefaultValue(TemaPreferenciaEnum.Claro)
            .HasSentinel((TemaPreferenciaEnum)0);
        builder.Property(x => x.IdiomaPreferencia)
            .HasDefaultValue(IdiomaPreferenciaEnum.PtBr)
            .HasSentinel((IdiomaPreferenciaEnum)0);
        builder.Property(x => x.NotificarEmail).HasDefaultValue(true);
        builder.Property(x => x.NotificarSistema).HasDefaultValue(true);
        builder.Property(x => x.Excluido).HasDefaultValue(false);
        builder.Property(x => x.DataCriacaoUtc).HasColumnType("datetime2");
        builder.Property(x => x.UltimoLoginUtc).HasColumnType("datetime2");
        builder.Property(x => x.DataExclusaoUtc).HasColumnType("datetime2");

        builder.HasOne(x => x.Aluno)
            .WithOne()
            .HasForeignKey<UsuarioSistema>(x => x.AlunoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.LogsComoAutor)
            .WithOne(x => x.UsuarioResponsavel)
            .HasForeignKey(x => x.UsuarioResponsavelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(x => x.LogsComoAfetado)
            .WithOne(x => x.UsuarioAfetado)
            .HasForeignKey(x => x.UsuarioAfetadoId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
