using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IUserSettingsService
{
    Task<UserSettingsPageViewModel?> GetPageAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserSettingsOperationResult> UpdateAccountInfoAsync(int userId, UpdateAccountInfoRequest request, CancellationToken cancellationToken = default);
    Task<UserSettingsOperationResult> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<UserSettingsOperationResult> UpdatePreferencesAsync(int userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default);
}
