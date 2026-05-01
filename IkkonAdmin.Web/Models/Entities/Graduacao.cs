using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Models.Entities;

public class Graduacao
{
    public int Id { get; set; }

    public int AlunoId { get; set; }
    public Aluno? Aluno { get; set; }

    public int? ExameGraduacaoId { get; set; }
    public ExameGraduacao? ExameGraduacao { get; set; }

    public DateOnly DataResultado { get; set; }
    public bool ResultadoAprovado { get; set; }

    public NivelGraduacaoEnum NivelAnterior { get; set; } = NivelGraduacaoEnum.Iniciante;
    public NivelGraduacaoEnum? NivelNovo { get; set; }

    public bool CertificadoEmitido { get; set; }
    public bool OmamoriAtualizado { get; set; }

    public string? Observacoes { get; set; }
}
