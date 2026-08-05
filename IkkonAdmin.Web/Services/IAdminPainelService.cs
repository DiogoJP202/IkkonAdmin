using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAdminPainelService
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
    Task<OperationResult> CriarUsuarioAsync(AdminUsuarioFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarUsuarioAsync(int id, AdminUsuarioFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AlterarStatusUsuarioAsync(int id, bool ativo, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirUsuarioAsync(int id, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);

    Task<AdminAcessosViewModel?> ObterAcessosAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarAcessosAsync(AdminAcessosUpdateRequest request, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);

    Task<AdminRolesIndexViewModel> ListarRolesAsync(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);

    Task<AdminRoleFormViewModel> ObterRoleParaCriacaoAsync(CancellationToken cancellationToken = default);
    Task<AdminRoleFormViewModel?> ObterRoleParaEdicaoAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult> CriarRoleAsync(AdminRoleFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarRoleAsync(int id, AdminRoleFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AlterarStatusRoleAsync(int id, bool ativo, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirRoleAsync(int id, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);

    Task<AdminLogsIndexViewModel> ListarLogsAsync(
        string? busca,
        int? usuarioResponsavelId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default);
}
