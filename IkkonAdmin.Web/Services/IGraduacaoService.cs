using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IGraduacaoService
{
    Task<OperationResult<int>> CriarExameAsync(ExameGraduacao exame, CancellationToken cancellationToken = default);

    Task<OperationResult<GraduacaoRegistroResultado>> RegistrarResultadoAsync(
        GraduacaoRegistroInput input,
        CancellationToken cancellationToken = default);
}

public sealed class GraduacaoRegistroInput
{
    public int AlunoId { get; set; }
    public int? ExameGraduacaoId { get; set; }
    public DateOnly? DataExameNovo { get; set; }
    public string? LocalExameNovo { get; set; }
    public NivelGraduacaoEnum NivelPretendidoExameNovo { get; set; } = NivelGraduacaoEnum.Basico;
    public DateOnly DataResultado { get; set; }
    public bool ResultadoAprovado { get; set; }
    public NivelGraduacaoEnum? NivelNovo { get; set; }
    public bool CertificadoEmitido { get; set; }
    public bool OmamoriAtualizado { get; set; }
    public string? Observacoes { get; set; }
}

public sealed class GraduacaoRegistroResultado
{
    public int GraduacaoId { get; set; }
    public int? ExameGraduacaoId { get; set; }
}
