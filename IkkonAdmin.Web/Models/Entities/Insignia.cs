using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class Insignia
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    [StringLength(80)]
    public string? Icone { get; set; }

    [StringLength(80)]
    public string? Categoria { get; set; }

    public bool Ativa { get; set; } = true;

    [StringLength(120)]
    public string? RegraAutomatica { get; set; }

    public ICollection<AlunoInsignia> Alunos { get; } = new List<AlunoInsignia>();
}
