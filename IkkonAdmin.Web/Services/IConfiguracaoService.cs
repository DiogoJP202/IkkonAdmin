using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IConfiguracaoService
{
    Task<ConfiguracoesIndexViewModel> ObterPainelAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracoesFormViewModel> ObterFormularioAsync(CancellationToken cancellationToken = default);
    Task SalvarAsync(ConfiguracoesFormViewModel form, CancellationToken cancellationToken = default);
    Task RestaurarPadraoAsync(CancellationToken cancellationToken = default);
    Task<ConfiguracaoSistema> ObterOuCriarAsync(CancellationToken cancellationToken = default);
}

