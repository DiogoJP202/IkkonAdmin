using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Services;

public interface IConfiguracaoSistemaProvider
{
    Task<ConfiguracaoSistema> ObterOuCriarAsync(CancellationToken cancellationToken = default);
}
