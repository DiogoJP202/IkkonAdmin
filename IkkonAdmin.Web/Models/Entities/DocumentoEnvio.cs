using System.ComponentModel.DataAnnotations;

namespace IkkonAdmin.Web.Models.Entities;

public class DocumentoEnvio
{
    public int Id { get; set; }

    public int DocumentoSolicitacaoId { get; set; }
    public DocumentoSolicitacao? DocumentoSolicitacao { get; set; }

    [Required, StringLength(500)]
    public string ArquivoUrl { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string NomeArquivoOriginal { get; set; } = string.Empty;

    [StringLength(120)]
    public string? ContentType { get; set; }

    public long TamanhoBytes { get; set; }
    public DateTime EnviadoEmUtc { get; set; } = DateTime.UtcNow;

    public int? EnviadoPorUsuarioId { get; set; }
    public UsuarioSistema? EnviadoPorUsuario { get; set; }
}
