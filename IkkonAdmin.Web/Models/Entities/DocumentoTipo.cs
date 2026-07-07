using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class DocumentoTipo
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Descricao { get; set; }

    public bool Obrigatorio { get; set; }
    public bool Ativo { get; set; } = true;

    public ICollection<DocumentoSolicitacao> Solicitacoes { get; } = new List<DocumentoSolicitacao>();
}
