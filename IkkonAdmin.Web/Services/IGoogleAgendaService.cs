using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IGoogleAgendaService
{
    string? CalendarId { get; }
    Task<bool> PossuiConexaoOAuthAsync(CancellationToken cancellationToken = default);
    Task<string> GerarUrlAutorizacaoAsync(string redirectUri, string state, CancellationToken cancellationToken = default);
    Task ConcluirAutorizacaoOAuthAsync(string code, string redirectUri, int? usuarioId, CancellationToken cancellationToken = default);
    Task DesconectarOAuthAsync(int? usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GoogleAgendaEventoViewModel>> ListarEventosAsync(GoogleAgendaFiltroViewModel filtro, CancellationToken cancellationToken = default);
    Task<GoogleAgendaEventoViewModel?> ObterEventoAsync(string eventoId, CancellationToken cancellationToken = default);
    Task<GoogleAgendaEventoViewModel> CriarEventoAsync(GoogleAgendaEventoFormViewModel model, CancellationToken cancellationToken = default);
    Task<GoogleAgendaEventoViewModel> AtualizarEventoAsync(string eventoId, GoogleAgendaEventoFormViewModel model, CancellationToken cancellationToken = default);
    Task ExcluirEventoAsync(string eventoId, CancellationToken cancellationToken = default);
}
