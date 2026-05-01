using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class DescontoConfiguration : IEntityTypeConfiguration<Desconto>
{
    public void Configure(EntityTypeBuilder<Desconto> builder)
    {
        builder.ToTable("Descontos");

        builder.Property(x => x.Nome).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Tipo).HasMaxLength(80);
        builder.Property(x => x.Percentual).HasPrecision(5, 2);
        builder.Property(x => x.ValorFixo).HasPrecision(10, 2);
        builder.Property(x => x.VigenciaInicio).HasColumnType("date");
        builder.Property(x => x.VigenciaFim).HasColumnType("date");

        builder.HasOne(x => x.Aluno)
            .WithMany(x => x.Descontos)
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
