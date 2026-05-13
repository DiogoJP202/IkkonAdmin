using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IBlogCategoriaService
{
    Task<BlogCategoryIndexViewModel> ListarAsync(CancellationToken cancellationToken = default);
    Task<BlogCategoryFormViewModel> ObterParaCriacaoAsync(CancellationToken cancellationToken = default);
    Task<BlogCategoryFormViewModel?> ObterParaEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BlogCategorySelectItemViewModel>> ListarOpcoesAtivasAsync(int? categoriaAtualId = null, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> CriarAsync(BlogCategoryFormViewModel model, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> AtualizarAsync(int id, BlogCategoryFormViewModel model, CancellationToken cancellationToken = default);
    Task<BlogOperationResult> AlterarStatusAsync(int id, bool ativo, CancellationToken cancellationToken = default);
}
