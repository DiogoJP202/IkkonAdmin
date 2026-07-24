using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class UserSettingsService(
    ApplicationDbContext dbContext,
    IPasswordHasher<UsuarioSistema> passwordHasher,
    IFileStorageService fileStorageService,
    IUserSettingsQueryService queryService) : IUserSettingsService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxImageSizeBytes = 2 * 1024 * 1024;

    public Task<UserSettingsPageViewModel?> GetPageAsync(int userId, CancellationToken cancellationToken = default)
    {
        return queryService.GetPageAsync(userId, cancellationToken);
    }

    public async Task<UserSettingsOperationResult> UpdateAccountInfoAsync(
        int userId,
        UpdateAccountInfoRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.UsuariosSistema
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return UserSettingsOperationResult.Fail("Conta não encontrada.");
        }

        var normalizedEmail = Normalize(request.Email);
        var emailAlreadyUsed = await dbContext.UsuariosSistema
            .AnyAsync(
                x => x.Id != userId && x.EmailNormalizado == normalizedEmail,
                cancellationToken);

        if (emailAlreadyUsed)
        {
            return UserSettingsOperationResult.Fail("Este e-mail já está em uso por outra conta.");
        }

        user.NomeExibicao = request.NomeCompleto.Trim();
        user.Email = request.Email.Trim();
        user.EmailNormalizado = normalizedEmail;
        user.Telefone = string.IsNullOrWhiteSpace(request.Telefone) ? null : request.Telefone.Trim();

        if (request.FotoPerfil is not null && request.FotoPerfil.Length > 0)
        {
            var uploadResult = await SaveProfilePhotoAsync(user, request.FotoPerfil, cancellationToken);
            if (!uploadResult.Success)
            {
                return uploadResult;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return UserSettingsOperationResult.Ok("Dados da conta atualizados com sucesso.");
    }

    public async Task<UserSettingsOperationResult> ChangePasswordAsync(
        int userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.UsuariosSistema
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return UserSettingsOperationResult.Fail("Conta não encontrada.");
        }

        var verifyResult = passwordHasher.VerifyHashedPassword(user, user.SenhaHash, request.SenhaAtual);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return UserSettingsOperationResult.Fail("Não foi possível alterar a senha. Verifique os dados informados.");
        }

        if (!IsStrongPassword(request.NovaSenha))
        {
            return UserSettingsOperationResult.Fail("A nova senha deve ter 8+ caracteres, com letra maiúscula, minúscula, número e símbolo.");
        }

        if (request.SenhaAtual == request.NovaSenha)
        {
            return UserSettingsOperationResult.Fail("A nova senha deve ser diferente da senha atual.");
        }

        user.SenhaHash = passwordHasher.HashPassword(user, request.NovaSenha);
        await dbContext.SaveChangesAsync(cancellationToken);

        return UserSettingsOperationResult.Ok("Senha alterada com sucesso.");
    }

    public async Task<UserSettingsOperationResult> UpdatePreferencesAsync(
        int userId,
        UpdatePreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await dbContext.UsuariosSistema
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return UserSettingsOperationResult.Fail("Conta não encontrada.");
        }

        user.TemaPreferencia = request.TemaPreferencia;
        user.IdiomaPreferencia = request.IdiomaPreferencia;
        user.NotificarEmail = request.NotificarEmail;
        user.NotificarSistema = request.NotificarSistema;

        await dbContext.SaveChangesAsync(cancellationToken);
        return UserSettingsOperationResult.Ok("Preferências atualizadas com sucesso.");
    }

    private async Task<UserSettingsOperationResult> SaveProfilePhotoAsync(
        UsuarioSistema user,
        IFormFile photo,
        CancellationToken cancellationToken)
    {
        if (photo.Length > MaxImageSizeBytes)
        {
            return UserSettingsOperationResult.Fail("A foto de perfil deve ter no máximo 2 MB.");
        }

        var extension = Path.GetExtension(photo.FileName ?? string.Empty);
        if (!AllowedImageExtensions.Contains(extension))
        {
            return UserSettingsOperationResult.Fail("Formato de imagem inválido. Use JPG, PNG ou WEBP.");
        }

        if (!string.IsNullOrWhiteSpace(user.FotoPerfilUrl) &&
            user.FotoPerfilUrl.StartsWith("/uploads/perfis/", StringComparison.OrdinalIgnoreCase))
        {
            var oldFilePath = fileStorageService.GetPublicFilePath(
                user.FotoPerfilUrl,
                "/uploads/perfis/",
                "uploads",
                "perfis");

            if (oldFilePath is not null)
            {
                fileStorageService.DeleteIfExists(oldFilePath);
            }
        }

        var fileName = $"{user.Id}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var result = await fileStorageService.SaveToWebRootAsync(
            photo,
            ["uploads", "perfis"],
            "/uploads/perfis",
            fileName,
            cancellationToken);

        user.FotoPerfilUrl = result.PublicUrl ?? $"/uploads/perfis/{fileName}";
        return UserSettingsOperationResult.Ok("Foto atualizada.");
    }

    private static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return false;
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSymbol;
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
