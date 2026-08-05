using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogCategoriaService
{
    Task<BlogCategoryIndexViewModel> ListarAsync(CancellationToken cancellationToken = default);
    Task<BlogCategoryFormViewModel> ObterParaCriacaoAsync(CancellationToken cancellationToken = default);
    Task<BlogCategoryFormViewModel?> ObterParaEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BlogCategorySelectItemViewModel>> ListarOpcoesAtivasAsync(int? categoriaAtualId = null, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> CriarAsync(BlogCategoryFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> AtualizarAsync(int id, BlogCategoryFormViewModel model, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> AlterarStatusAsync(int id, bool ativo, CancellationToken cancellationToken = default);
    Task<OperationResult<int>> ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
