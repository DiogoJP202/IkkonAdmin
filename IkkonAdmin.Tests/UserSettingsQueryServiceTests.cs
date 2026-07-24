using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class UserSettingsQueryServiceTests
{
    [Fact]
    public async Task GetPageAsync_MontaContaPreferenciasTipoEHistorico()
    {
        await using var dbContext = CriarDbContext();
        var user = CriarUsuario(TipoAcessoEnum.Admin);
        user.Email = "admin@ikkon.local";
        user.EmailNormalizado = "ADMIN@IKKON.LOCAL";
        user.Telefone = "(11) 99999-0000";
        user.FotoPerfilUrl = "/uploads/perfis/admin.webp";
        user.TemaPreferencia = TemaPreferenciaEnum.Escuro;
        user.IdiomaPreferencia = IdiomaPreferenciaEnum.EnUs;
        user.NotificarEmail = false;
        user.NotificarSistema = true;
        user.UltimoLoginUtc = new DateTime(2026, 7, 12, 10, 0, 0, DateTimeKind.Utc);

        dbContext.UsuariosSistema.Add(user);
        await dbContext.SaveChangesAsync();

        for (var i = 0; i < 12; i++)
        {
            dbContext.AuditoriaLogs.Add(new AuditoriaLog
            {
                UsuarioAfetadoId = user.Id,
                Acao = "LOGIN_SUCESSO",
                Entidade = nameof(UsuarioSistema),
                EntidadeId = user.Id.ToString(),
                Descricao = i == 0 ? null : $"Login #{i}",
                EnderecoIp = $"127.0.0.{i}",
                DataEventoUtc = new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc).AddMinutes(i)
            });
        }

        dbContext.AuditoriaLogs.Add(new AuditoriaLog
        {
            UsuarioAfetadoId = user.Id,
            Acao = "OUTRA_ACAO",
            Entidade = nameof(UsuarioSistema),
            DataEventoUtc = new DateTime(2026, 7, 14, 12, 0, 0, DateTimeKind.Utc)
        });
        await dbContext.SaveChangesAsync();

        var service = new UserSettingsQueryService(dbContext);

        var page = await service.GetPageAsync(user.Id);

        Assert.NotNull(page);
        Assert.Equal("Administrador Ikkon", page.AccountInfo.NomeCompleto);
        Assert.Equal("admin@ikkon.local", page.AccountInfo.Email);
        Assert.Equal("/uploads/perfis/admin.webp", page.AccountInfo.FotoPerfilUrl);
        Assert.True(page.AccountInfo.ContaAtiva);
        Assert.Equal(TemaPreferenciaEnum.Escuro, page.Preferences.TemaPreferencia);
        Assert.Equal(IdiomaPreferenciaEnum.EnUs, page.Preferences.IdiomaPreferencia);
        Assert.False(page.Preferences.NotificarEmail);
        Assert.Equal("Administrador", page.AccountType.NomeTipoConta);
        Assert.Contains("Gestão de usuários e permissões", page.AccountType.PermissoesBasicas);
        Assert.Equal(10, page.SecurityStatus.HistoricoAcessos.Count);
        Assert.Equal("Login #11", page.SecurityStatus.HistoricoAcessos.First().Descricao);
        Assert.DoesNotContain(page.SecurityStatus.HistoricoAcessos, x => x.Descricao == "Login #0");
    }

    [Fact]
    public async Task GetPageAsync_RetornaNullQuandoUsuarioNaoExiste()
    {
        await using var dbContext = CriarDbContext();
        var service = new UserSettingsQueryService(dbContext);

        var page = await service.GetPageAsync(999);

        Assert.Null(page);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static UsuarioSistema CriarUsuario(TipoAcessoEnum tipoAcesso)
    {
        return new UsuarioSistema
        {
            Login = "admin",
            LoginNormalizado = "ADMIN",
            NomeExibicao = "Administrador Ikkon",
            SenhaHash = "hash",
            TipoAcesso = tipoAcesso,
            Ativo = true
        };
    }
}
