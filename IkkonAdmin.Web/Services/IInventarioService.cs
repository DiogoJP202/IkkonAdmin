using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IInventarioService
{
    Task<OperationResult<int>> CriarAsync(InventarioFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> AtualizarAsync(int id, InventarioFormViewModel model, int? usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> InativarAsync(int id, int? usuarioId, CancellationToken cancellationToken = default);
}
