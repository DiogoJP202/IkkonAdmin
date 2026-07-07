using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IkkonAdmin.Web.Data.Configurations;

public class EventoAlunoPortalAlvoConfiguration : IEntityTypeConfiguration<EventoAlunoPortalAlvo>
{
    public void Configure(EntityTypeBuilder<EventoAlunoPortalAlvo> builder)
    {
        builder.ToTable("EventosAlunoPortalAlvos");
        builder.HasIndex(x => new { x.EventoAlunoPortalId, x.AlunoId, x.TurmaId, x.Todos });

        builder.HasOne(x => x.EventoAlunoPortal)
            .WithMany(x => x.Alvos)
            .HasForeignKey(x => x.EventoAlunoPortalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Aluno)
            .WithMany()
            .HasForeignKey(x => x.AlunoId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Turma)
            .WithMany()
            .HasForeignKey(x => x.TurmaId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
