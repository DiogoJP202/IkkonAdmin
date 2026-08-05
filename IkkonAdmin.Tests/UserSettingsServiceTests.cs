using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class UserSettingsServiceTests
{
    [Fact]
    public async Task UpdateAccountInfoAsync_NormalizaDadosEBloqueiaEmailDuplicado()
    {
        await using var dbContext = CriarDbContext();
        var usuario = CriarUsuario("rafael", "rafael@ikkon.local");
        var outro = CriarUsuario("outro", "ocupado@ikkon.local");
        dbContext.UsuariosSistema.AddRange(usuario, outro);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var duplicado = await service.UpdateAccountInfoAsync(usuario.Id, new UpdateAccountInfoRequest
        {
            NomeCompleto = "Rafael Sato",
            Email = "OCUPADO@ikkon.local",
            Telefone = "(11) 99999-0001"
        });

        Assert.False(duplicado.Success);

        var resultado = await service.UpdateAccountInfoAsync(usuario.Id, new UpdateAccountInfoRequest
        {
            NomeCompleto = "  Rafael Sato  ",
            Email = " rafael.novo@ikkon.local ",
            Telefone = " (11) 99999-0001 "
        });

        var atualizado = await dbContext.UsuariosSistema.FindAsync(usuario.Id);

        Assert.True(resultado.Success);
        Assert.Equal("Rafael Sato", atualizado?.NomeExibicao);
        Assert.Equal("rafael.novo@ikkon.local", atualizado?.Email);
        Assert.Equal("RAFAEL.NOVO@IKKON.LOCAL", atualizado?.EmailNormalizado);
        Assert.Equal("(11) 99999-0001", atualizado?.Telefone);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidaSenhaAtualForcaEDiferenca()
    {
        await using var dbContext = CriarDbContext();
        var hasher = new PasswordHasher<UsuarioSistema>();
        var usuario = CriarUsuario("marina", "marina@ikkon.local");
        usuario.SenhaHash = hasher.HashPassword(usuario, "Senha@123");
        dbContext.UsuariosSistema.Add(usuario);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext, hasher);

        var senhaAtualErrada = await service.ChangePasswordAsync(usuario.Id, new ChangePasswordRequest
        {
            SenhaAtual = "Errada@123",
            NovaSenha = "Nova@1234",
            ConfirmacaoNovaSenha = "Nova@1234"
        });

        var senhaFraca = await service.ChangePasswordAsync(usuario.Id, new ChangePasswordRequest
        {
            SenhaAtual = "Senha@123",
            NovaSenha = "senhafraca",
            ConfirmacaoNovaSenha = "senhafraca"
        });

        var mesmaSenha = await service.ChangePasswordAsync(usuario.Id, new ChangePasswordRequest
        {
            SenhaAtual = "Senha@123",
            NovaSenha = "Senha@123",
            ConfirmacaoNovaSenha = "Senha@123"
        });

        var sucesso = await service.ChangePasswordAsync(usuario.Id, new ChangePasswordRequest
        {
            SenhaAtual = "Senha@123",
            NovaSenha = "Nova@1234",
            ConfirmacaoNovaSenha = "Nova@1234"
        });

        var atualizado = await dbContext.UsuariosSistema.FindAsync(usuario.Id);

        Assert.False(senhaAtualErrada.Success);
        Assert.False(senhaFraca.Success);
        Assert.False(mesmaSenha.Success);
        Assert.True(sucesso.Success);
        Assert.Equal(
            PasswordVerificationResult.Success,
            hasher.VerifyHashedPassword(atualizado!, atualizado!.SenhaHash, "Nova@1234"));
    }

    [Fact]
    public async Task UpdatePreferencesAsync_AtualizaPreferencias()
    {
        await using var dbContext = CriarDbContext();
        var usuario = CriarUsuario("ana", "ana@ikkon.local");
        dbContext.UsuariosSistema.Add(usuario);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var resultado = await service.UpdatePreferencesAsync(usuario.Id, new UpdatePreferencesRequest
        {
            TemaPreferencia = TemaPreferenciaEnum.Escuro,
            IdiomaPreferencia = IdiomaPreferenciaEnum.EnUs,
            NotificarEmail = false,
            NotificarSistema = false
        });

        var atualizado = await dbContext.UsuariosSistema.FindAsync(usuario.Id);

        Assert.True(resultado.Success);
        Assert.Equal(TemaPreferenciaEnum.Escuro, atualizado?.TemaPreferencia);
        Assert.Equal(IdiomaPreferenciaEnum.EnUs, atualizado?.IdiomaPreferencia);
        Assert.False(atualizado?.NotificarEmail);
        Assert.False(atualizado?.NotificarSistema);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static UserSettingsService CriarService(
        ApplicationDbContext dbContext,
        IPasswordHasher<UsuarioSistema>? passwordHasher = null)
    {
        return new UserSettingsService(
            dbContext,
            passwordHasher ?? new PasswordHasher<UsuarioSistema>(),
            new FakeFileStorageService());
    }

    private static UsuarioSistema CriarUsuario(string login, string email)
    {
        return new UsuarioSistema
        {
            Login = login,
            LoginNormalizado = login.ToUpperInvariant(),
            NomeExibicao = login,
            Email = email,
            EmailNormalizado = email.ToUpperInvariant(),
            SenhaHash = "hash",
            TipoAcesso = TipoAcessoEnum.Funcionario,
            Ativo = true
        };
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public string GetAppDataPath(params string[] relativeSegments)
        {
            return Path.Combine(relativeSegments);
        }

        public string GetWebRootPath(params string[] relativeSegments)
        {
            return Path.Combine(relativeSegments);
        }

        public string? GetPublicFilePath(string publicUrl, string expectedPublicPrefix, params string[] rootSegments)
        {
            return null;
        }

        public bool Exists(string physicalPath)
        {
            return false;
        }

        public void DeleteIfExists(string physicalPath)
        {
        }

        public Task<FileStorageResult> SaveToAppDataAsync(
            IFormFile file,
            string[] relativeFolderSegments,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new FileStorageResult(fileName, fileName, null));
        }

        public Task<FileStorageResult> SaveToWebRootAsync(
            IFormFile file,
            string[] relativeFolderSegments,
            string publicBaseUrl,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new FileStorageResult(fileName, fileName, $"{publicBaseUrl}/{fileName}"));
        }
    }
}
