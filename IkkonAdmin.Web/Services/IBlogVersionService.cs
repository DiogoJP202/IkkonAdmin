using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogVersionService
{
    Task<OperationResult<int>> CriarVersaoAsync(int id, string languageCode, int? usuarioAtualId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> ExcluirVersaoAsync(int id, int versionId, int? usuarioAtualId, CancellationToken cancellationToken = default);
}
