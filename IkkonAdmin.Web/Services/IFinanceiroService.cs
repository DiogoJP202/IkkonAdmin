using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IFinanceiroService
{
    Task<FinanceiroIndexViewModel> ObterResumoAsync(
        string? buscaAluno = null,
        StatusMensalidadeEnum? statusFiltro = null,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default);

    Task<FinanceiroAtrasadosViewModel> ObterAtrasadosAsync(CancellationToken cancellationToken = default);
    Task<RegistrarPagamentoViewModel?> ObterFormularioPagamentoAsync(int mensalidadeId, CancellationToken cancellationToken = default);
    Task<bool> RegistrarPagamentoAsync(RegistrarPagamentoViewModel model, CancellationToken cancellationToken = default);
    Task<FinanceiroGeracaoResultadoViewModel> GerarMensalidadesAsync(int ano, int mes, CancellationToken cancellationToken = default);
    Task<bool> AtualizarValorFinalAsync(int mensalidadeId, decimal valorFinal, CancellationToken cancellationToken = default);
    Task<bool> AlterarStatusMensalidadeAsync(int mensalidadeId, StatusMensalidadeEnum status, CancellationToken cancellationToken = default);
    Task<FinanceiroHistoricoAlunoViewModel?> ObterHistoricoAlunoAsync(int alunoId, CancellationToken cancellationToken = default);
}
