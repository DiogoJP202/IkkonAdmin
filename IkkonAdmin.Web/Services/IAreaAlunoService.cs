using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoService
{
    Task<AreaAlunoDashboardViewModel?> ObterDashboardAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoPerfilViewModel?> ObterPerfilAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoFinanceiroViewModel?> ObterFinanceiroAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoTurmasViewModel?> ObterTurmasAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoAulasViewModel?> ObterAulasAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoFrequenciaViewModel?> ObterFrequenciaAsync(int usuarioId, DateOnly? inicio, DateOnly? fim, CancellationToken cancellationToken = default);
    Task<AreaAlunoEventosViewModel?> ObterEventosAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoDocumentosViewModel?> ObterDocumentosAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoOperacaoResult> EnviarDocumentoAsync(int usuarioId, int solicitacaoId, IFormFile arquivo, CancellationToken cancellationToken = default);
    Task<AreaAlunoDocumentoDownload?> ObterDocumentoParaDownloadAsync(int usuarioId, int envioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoComunicadosViewModel?> ObterComunicadosAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<bool> MarcarComunicadoComoLidoAsync(int usuarioId, int comunicadoId, CancellationToken cancellationToken = default);
    Task<AreaAlunoConquistasViewModel?> ObterConquistasAsync(int usuarioId, CancellationToken cancellationToken = default);
}

public sealed record AreaAlunoOperacaoResult(bool Sucesso, string Mensagem)
{
    public static AreaAlunoOperacaoResult Ok(string mensagem) => new(true, mensagem);
    public static AreaAlunoOperacaoResult Falha(string mensagem) => new(false, mensagem);
}

public sealed record AreaAlunoDocumentoDownload(
    string CaminhoArquivo,
    string NomeArquivoOriginal,
    string? ContentType);
