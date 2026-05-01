using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class ExameGraduacaoConfiguration : IEntityTypeConfiguration<ExameGraduacao>
{
    public void Configure(EntityTypeBuilder<ExameGraduacao> builder)
    {
        builder.ToTable("ExamesGraduacao");
        builder.Property(x => x.DataExame).HasColumnType("date");
        builder.Property(x => x.Local).HasMaxLength(150);
    }
}
