using System.ComponentModel.DataAnnotations;
using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class AlunoInsignia
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int InsigniaId { get; set; }
    public Insignia? Insignia { get; set; }

    public DateTime ConcedidaEmUtc { get; set; } = DateTime.UtcNow;

    public int? ConcedidaPorUsuarioId { get; set; }
    public UsuarioSistema? ConcedidaPorUsuario { get; set; }

    public InsigniaOrigemEnum Origem { get; set; } = InsigniaOrigemEnum.Manual;

    [StringLength(500)]
    public string? Observacao { get; set; }
}
