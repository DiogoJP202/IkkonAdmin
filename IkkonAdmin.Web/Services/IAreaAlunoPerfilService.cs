using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IAreaAlunoPerfilService
{
    Task<AreaAlunoPerfilViewModel?> ObterPerfilAsync(int usuarioId, CancellationToken cancellationToken = default);
    Task<AreaAlunoPerfilBase?> ObterPerfilBaseAsync(int alunoId, CancellationToken cancellationToken = default);
}
