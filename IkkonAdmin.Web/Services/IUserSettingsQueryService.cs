using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IUserSettingsQueryService
{
    Task<UserSettingsPageViewModel?> GetPageAsync(int userId, CancellationToken cancellationToken = default);
}
