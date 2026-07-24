using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IConfiguracaoQueryService
{
    Task<ConfiguracoesIndexViewModel> ObterPainelAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracoesFormViewModel> ObterFormularioAsync(CancellationToken cancellationToken = default);
}
