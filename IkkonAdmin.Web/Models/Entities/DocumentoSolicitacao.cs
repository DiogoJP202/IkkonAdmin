using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class DocumentoSolicitacao
{
    public int Id { get; set; }

    public int DocumentoTipoId { get; set; }
    public DocumentoTipo? DocumentoTipo { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int? SolicitadoPorUsuarioId { get; set; }
    public UsuarioSistema? SolicitadoPorUsuario { get; set; }

    public DocumentoStatusEnum Status { get; set; } = DocumentoStatusEnum.Solicitado;
    public DateTime DataSolicitacaoUtc { get; set; } = DateTime.UtcNow;
    public DateOnly? DataLimite { get; set; }

    [StringLength(1000)]
    public string? ObservacaoAdministrativa { get; set; }

    public ICollection<DocumentoEnvio> Envios { get; } = new List<DocumentoEnvio>();
}
