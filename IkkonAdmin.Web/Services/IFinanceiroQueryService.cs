using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IFinanceiroQueryService
{
    Task<FinanceiroIndexViewModel> ObterResumoAsync(
        string? buscaAluno = null,
        StatusMensalidadeEnum? statusFiltro = null,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default);

    Task<FinanceiroAtrasadosViewModel> ObterAtrasadosAsync(CancellationToken cancellationToken = default);

    Task<RegistrarPagamentoViewModel?> ObterFormularioPagamentoAsync(
        int mensalidadeId,
        CancellationToken cancellationToken = default);

    Task<FinanceiroHistoricoAlunoViewModel?> ObterHistoricoAlunoAsync(
        int alunoId,
        CancellationToken cancellationToken = default);
}
