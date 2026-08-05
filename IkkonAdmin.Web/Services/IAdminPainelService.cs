using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAdminPainelService
{
    Task<OperationResult> CriarUsuarioAsync(AdminUsuarioFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarUsuarioAsync(int id, AdminUsuarioFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AlterarStatusUsuarioAsync(int id, bool ativo, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirUsuarioAsync(int id, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);

    Task<OperationResult> AtualizarAcessosAsync(AdminAcessosUpdateRequest request, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> CriarRoleAsync(AdminRoleFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AtualizarRoleAsync(int id, AdminRoleFormViewModel model, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> AlterarStatusRoleAsync(int id, bool ativo, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);
    Task<OperationResult> ExcluirRoleAsync(int id, int usuarioResponsavelId, string? enderecoIp, CancellationToken cancellationToken = default);

}
