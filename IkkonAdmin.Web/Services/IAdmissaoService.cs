using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IAdmissaoService
{
    Task<OperationResult<int>> CriarAsync(Admissao admissao, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarProcessoAsync(
        int id,
        StatusAdmissaoEnum status,
        bool contratoAssinado,
        bool pagamentoInicialConfirmado,
        bool integracaoConcluida,
        string? checklistObservacoes,
        CancellationToken cancellationToken = default);

    Task<OperationResult<AdmissaoMatriculaResultado>> CriarMatriculaAsync(
        int admissaoId,
        AdmissaoMatriculaInput input,
        CancellationToken cancellationToken = default);
}

public sealed class AdmissaoMatriculaInput
{
    public string CPF { get; set; } = string.Empty;
    public string? RG { get; set; }
    public DateOnly? DataNascimento { get; set; }
    public string? Endereco { get; set; }
    public string? Celular { get; set; }
    public string? Email { get; set; }
    public string? ContatoEmergencia { get; set; }
    public int? TurmaId { get; set; }
    public string? ObservacoesAluno { get; set; }
}

public sealed class AdmissaoMatriculaResultado
{
    public int AlunoId { get; set; }
}
