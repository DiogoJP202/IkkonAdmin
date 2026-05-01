using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class ConfiguracaoSistemaConfiguration : IEntityTypeConfiguration<ConfiguracaoSistema>
{
    public void Configure(EntityTypeBuilder<ConfiguracaoSistema> builder)
    {
        builder.ToTable("ConfiguracoesSistema");

        builder.Property(x => x.NomeEscola)
            .HasMaxLength(160)
            .IsRequired();

        builder.Property(x => x.EmailFinanceiro).HasMaxLength(150);
        builder.Property(x => x.TelefoneContato).HasMaxLength(30);

        builder.Property(x => x.ValorMensalidadePadrao).HasPrecision(10, 2);
        builder.Property(x => x.PercentualMultaAtraso).HasPrecision(5, 2);
        builder.Property(x => x.PercentualJurosMes).HasPrecision(5, 2);

        builder.Property(x => x.MensagemBoasVindasPadrao).HasMaxLength(1000);
        builder.Property(x => x.ChecklistAdmissaoPadrao).HasMaxLength(1000);
    }
}

