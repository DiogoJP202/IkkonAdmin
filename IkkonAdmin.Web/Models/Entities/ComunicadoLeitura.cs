namespace IkkonAdmin.Web.Models.Entities;

public class ComunicadoLeitura
{
    public int ComunicadoId { get; set; }
    public Comunicado Comunicado { get; set; } = null!;

    public int AlunoId { get; set; }
    public Aluno Aluno { get; set; } = null!;

    public DateTime LidoEmUtc { get; set; } = DateTime.UtcNow;
}
