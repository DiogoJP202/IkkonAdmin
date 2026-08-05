using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IFinanceiroService
{
    Task<OperationResult> RegistrarPagamentoAsync(RegistrarPagamentoViewModel model, CancellationToken cancellationToken = default);
    Task<FinanceiroGeracaoResultadoViewModel> GerarMensalidadesAsync(int ano, int mes, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarValorFinalAsync(int mensalidadeId, decimal valorFinal, CancellationToken cancellationToken = default);
    Task<OperationResult> AlterarStatusMensalidadeAsync(int mensalidadeId, StatusMensalidadeEnum status, CancellationToken cancellationToken = default);
    Task<int> AtualizarAtrasosAsync(CancellationToken cancellationToken = default);
}
