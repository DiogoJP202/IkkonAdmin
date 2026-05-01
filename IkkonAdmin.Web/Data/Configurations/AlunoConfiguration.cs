using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class AlunoConfiguration : IEntityTypeConfiguration<Aluno>
{
    public void Configure(EntityTypeBuilder<Aluno> builder)
    {
        builder.ToTable("Alunos");
        builder.HasIndex(x => x.CPF).IsUnique();

        builder.Property(x => x.NomeCompleto).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CPF).HasMaxLength(14).IsRequired();
        builder.Property(x => x.RG).HasMaxLength(20);
        builder.Property(x => x.Celular).HasMaxLength(20);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Endereco).HasMaxLength(200);
        builder.Property(x => x.ContatoEmergencia).HasMaxLength(150);
        builder.Property(x => x.DataNascimento).HasColumnType("date");
        builder.Property(x => x.DataEntrada).HasColumnType("date");

        builder.HasOne(x => x.Turma)
            .WithMany(x => x.Alunos)
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
