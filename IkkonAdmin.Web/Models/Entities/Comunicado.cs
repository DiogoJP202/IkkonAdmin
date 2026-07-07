using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class Comunicado
{
    public int Id { get; set; }

    [Required, StringLength(180)]
    public string Titulo { get; set; } = string.Empty;

    [Required, StringLength(4000)]
    public string Conteudo { get; set; } = string.Empty;

    public bool Importante { get; set; }
    public bool Fixado { get; set; }
    public DateTime PublicadoEmUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiraEmUtc { get; set; }
    public bool Ativo { get; set; } = true;

    public int? CriadoPorUsuarioId { get; set; }
    public UsuarioSistema? CriadoPorUsuario { get; set; }

    public ICollection<ComunicadoAlvo> Alvos { get; } = new List<ComunicadoAlvo>();
    public ICollection<ComunicadoLeitura> Leituras { get; } = new List<ComunicadoLeitura>();
}
