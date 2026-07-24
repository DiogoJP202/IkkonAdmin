using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogVersionService
{
    Task<BlogVersionOverviewViewModel?> ObterVersoesAsync(int id, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> CriarVersaoAsync(int id, string languageCode, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> ExcluirVersaoAsync(int id, int versionId, int? usuarioAtualId, CancellationToken cancellationToken = default);
}
