using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Models.ViewModels;

namespace IkkonAdmin.Web.Services;

public interface IUserSettingsService
{
    Task<UserSettingsPageViewModel?> GetPageAsync(int userId, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdateAccountInfoAsync(int userId, UpdateAccountInfoRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> UpdatePreferencesAsync(int userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default);
}
