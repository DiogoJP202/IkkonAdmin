using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAdminPainelQueryService
{
    Task<AdminPainelViewModel> ObterPainelAsync(CancellationToken cancellationToken = default);

    Task<AdminUsuariosIndexViewModel> ListarUsuariosAsync(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        bool incluirExcluidos,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);

    Task<List<AdminRoleSelectItemViewModel>> ListarRolesAtivasAsync(
        TipoAcessoEnum? tipoAcesso,
        CancellationToken cancellationToken = default);

    Task<AdminUsuarioFormViewModel?> ObterUsuarioParaEdicaoAsync(int id, CancellationToken cancellationToken = default);

    Task<AdminAcessosViewModel?> ObterAcessosAsync(int usuarioId, CancellationToken cancellationToken = default);

    Task<AdminRolesIndexViewModel> ListarRolesAsync(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);

    Task<AdminRoleFormViewModel> ObterRoleParaCriacaoAsync(CancellationToken cancellationToken = default);

    Task<AdminRoleFormViewModel?> ObterRoleParaEdicaoAsync(int id, CancellationToken cancellationToken = default);

    Task<AdminLogsIndexViewModel> ListarLogsAsync(
        string? busca,
        int? usuarioResponsavelId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);
}
