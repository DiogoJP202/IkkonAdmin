using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class GoogleAgendaConnectionServiceTests
{
    [Fact]
    public async Task SubstituirConexaoAtivaAsync_DesativaAtualECriaTokenProtegido()
    {
        await using var dbContext = CriarDbContext();
        var conexaoAntiga = new GoogleAgendaConexao
        {
            RefreshTokenProtegido = "token-antigo-protegido",
            Escopos = "old",
            Ativa = true,
            CriadoEmUtc = new DateTime(2026, 7, 1, 12, 0, 0)
        };
        dbContext.GoogleAgendaConexoes.Add(conexaoAntiga);
        await dbContext.SaveChangesAsync();

        using var dataProtectionFixture = new DataProtectionFixture();
        var service = CriarService(dbContext, dataProtectionFixture.Provider);

        await service.SubstituirConexaoAtivaAsync(
            "refresh-token-novo",
            "calendar-scope",
            usuarioId: 7);

        var conexoes = await dbContext.GoogleAgendaConexoes
            .OrderBy(x => x.Id)
            .ToListAsync();
        var nova = conexoes.Single(x => x.Ativa);

        Assert.False(conexaoAntiga.Ativa);
        Assert.Equal(TestClock.FixedUtcNow, conexaoAntiga.AtualizadoEmUtc);
        Assert.Equal("calendar-scope", nova.Escopos);
        Assert.Equal(7, nova.ConectadoPorUsuarioId);
        Assert.Equal(TestClock.FixedUtcNow, nova.CriadoEmUtc);
        Assert.NotEqual("refresh-token-novo", nova.RefreshTokenProtegido);
        Assert.Equal("refresh-token-novo", await service.ObterRefreshTokenAtivoAsync());
    }

    [Fact]
    public async Task DesconectarOAuthAsync_DesativaConexaoAtiva()
    {
        await using var dbContext = CriarDbContext();
        using var dataProtectionFixture = new DataProtectionFixture();
        var service = CriarService(dbContext, dataProtectionFixture.Provider);

        await service.SubstituirConexaoAtivaAsync("refresh-token", null, usuarioId: 3);
        Assert.True(await service.PossuiConexaoOAuthAsync());

        await service.DesconectarOAuthAsync(usuarioId: 3);

        Assert.False(await service.PossuiConexaoOAuthAsync());
        Assert.Null(await service.ObterRefreshTokenAtivoAsync());
        Assert.All(dbContext.GoogleAgendaConexoes, x =>
        {
            Assert.False(x.Ativa);
            Assert.Equal(TestClock.FixedUtcNow, x.AtualizadoEmUtc);
        });
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static GoogleAgendaConnectionService CriarService(
        ApplicationDbContext dbContext,
        IDataProtectionProvider dataProtectionProvider)
    {
        return new GoogleAgendaConnectionService(dbContext, dataProtectionProvider, new TestClock());
    }

    private sealed class DataProtectionFixture : IDisposable
    {
        private readonly string directoryPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public DataProtectionFixture()
        {
            Directory.CreateDirectory(directoryPath);
            Provider = DataProtectionProvider.Create(new DirectoryInfo(directoryPath));
        }

        public IDataProtectionProvider Provider { get; }

        public void Dispose()
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
    }

    private sealed class TestClock : IClock
    {
        public static readonly DateTime FixedUtcNow = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);

        public DateTime UtcNow => FixedUtcNow;
        public DateTime Now => FixedUtcNow.ToLocalTime();
        public DateTime Today => FixedUtcNow.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
