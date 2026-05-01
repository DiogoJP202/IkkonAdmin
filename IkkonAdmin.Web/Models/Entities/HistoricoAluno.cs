using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class HistoricoAluno
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public DateTime DataEvento { get; set; } = DateTime.Now;

    [Required, StringLength(80)]
    public string TipoEvento { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Descricao { get; set; } = string.Empty;
}
