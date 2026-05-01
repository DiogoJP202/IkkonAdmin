using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class UserSettingsService(
    ApplicationDbContext dbContext,
    IPasswordHasher<UsuarioSistema> passwordHasher,
    IWebHostEnvironment webHostEnvironment) : IUserSettingsService
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxImageSizeBytes = 2 * 1024 * 1024;

    public async Task<UserSettingsPageViewModel?> GetPageAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.UsuariosSistema
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        var historicoAcessos = await dbContext.AuditoriaLogs
            .AsNoTracking()
            .Where(x => x.UsuarioAfetadoId == userId && x.Acao == "LOGIN_SUCESSO")
            .OrderByDescending(x => x.DataEventoUtc)
            .Take(10)
            .Select(x => new HistoricoAcessoViewModel
            {
                DataAcessoUtc = x.DataEventoUtc,
                EnderecoIp = x.EnderecoIp,
                Descricao = x.Descricao ?? "Login realizado com sucesso."
            })
            .ToListAsync(cancellationToken);

        return new UserSettingsPageViewModel
        {
            AccountInfo = new AccountInfoViewModel
            {
                NomeCompleto = user.NomeExibicao,
                Email = user.Email ?? string.Empty,
                Telefone = user.Telefone,
                FotoPerfilUrl = user.FotoPerfilUrl,
                ContaAtiva = user.Ativo
            },
            SecurityStatus = new SecurityStatusViewModel
            {
                ContaAtiva = user.Ativo,
                TwoFactorEnabled = false,
                UltimoLoginUtc = user.UltimoLoginUtc,
                HistoricoAcessos = historicoAcessos
            },
            Preferences = new PreferencesViewModel
            {
                TemaPreferencia = user.TemaPreferencia,
                IdiomaPreferencia = user.IdiomaPreferencia,
                NotificarEmail = user.NotificarEmail,
                NotificarSistema = user.NotificarSistema
            },
            AccountType = BuildAccountType(user.TipoAcesso)
        };
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

        var uploadsFolder = Path.Combine(webHostEnvironment.WebRootPath, "uploads", "perfis");
        Directory.CreateDirectory(uploadsFolder);

        if (!string.IsNullOrWhiteSpace(user.FotoPerfilUrl) &&
            user.FotoPerfilUrl.StartsWith("/uploads/perfis/", StringComparison.OrdinalIgnoreCase))
        {
            var oldFileName = Path.GetFileName(user.FotoPerfilUrl);
            var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        var fileName = $"{user.Id}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await photo.CopyToAsync(stream, cancellationToken);
        }

        user.FotoPerfilUrl = $"/uploads/perfis/{fileName}";
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

    private static AccountTypeInfoViewModel BuildAccountType(TipoAcessoEnum tipoAcesso)
    {
        return tipoAcesso switch
        {
            TipoAcessoEnum.Admin => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Administrador",
                PermissoesBasicas = new[]
                {
                    "Acesso total ao painel administrativo",
                    "Gestão de usuários e permissões",
                    "Controle de configurações e auditoria"
                }
            },
            TipoAcessoEnum.Funcionario => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Funcionário",
                PermissoesBasicas = new[]
                {
                    "Acesso ao painel administrativo interno",
                    "Gestão de alunos, turmas e financeiro",
                    "Visualização de indicadores operacionais"
                }
            },
            TipoAcessoEnum.Aluno => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Aluno",
                PermissoesBasicas = new[]
                {
                    "Acesso à área exclusiva do aluno",
                    "Consulta de dados e histórico pessoal",
                    "Recebimento de notificações e comunicados"
                }
            },
            _ => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Conta",
                PermissoesBasicas = Array.Empty<string>()
            }
        };
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }
}
