using System.Text.Json;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Files;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace IkkonAdmin.Tests;

public sealed class ProductionBootstrapTests
{
    [Fact]
    public void InitializeStructural_CriaSomenteConfiguracaoRolesEPermissoes()
    {
        using var dbContext = CreateDbContext();

        SeedData.InitializeStructural(dbContext);

        Assert.Single(dbContext.ConfiguracoesSistema);
        Assert.Equal(3, dbContext.RolesSistema.Count());
        Assert.Equal(AppPermissions.Definicoes.Count, dbContext.PermissoesSistema.Count());
        Assert.Empty(dbContext.Alunos);
        Assert.Empty(dbContext.UsuariosSistema);
        Assert.Empty(dbContext.InventarioItens);
    }

    [Fact]
    public void InitialAdminBootstrap_CriaUmaUnicaVezEAtribuiRoleAdmin()
    {
        using var dbContext = CreateDbContext();
        SeedData.InitializeStructural(dbContext);
        var bootstrap = new InitialAdminBootstrap(
            dbContext,
            new PasswordHasher<UsuarioSistema>(),
            Options.Create(new InitialAdminBootstrapOptions
            {
                Login = "admin.inicial",
                Email = "admin@example.com",
                DisplayName = "Admin Inicial",
                Password = "SenhaInicial@123"
            }),
            NullLogger<InitialAdminBootstrap>.Instance);

        bootstrap.CreateOnlyWhenNoAdminExists();
        bootstrap.CreateOnlyWhenNoAdminExists();

        var admin = Assert.Single(dbContext.UsuariosSistema);
        Assert.Equal(TipoAcessoEnum.Admin, admin.TipoAcesso);
        Assert.NotEqual("SenhaInicial@123", admin.SenhaHash);
        Assert.Contains(
            dbContext.UsuariosRoles.Include(x => x.Role),
            link => link.UsuarioId == admin.Id && link.Role!.Codigo == AppRoles.Admin);
    }

    [Fact]
    public void AppSettingsBase_NaoContemSecretsOuConnectionStringOperacional()
    {
        var appSettingsPath = Path.Combine(FindRepositoryRoot(), "IkkonAdmin.Web", "appsettings.json");
        using var document = JsonDocument.Parse(File.ReadAllText(appSettingsPath));

        Assert.False(document.RootElement.TryGetProperty("ConnectionStrings", out _));
        var google = document.RootElement.GetProperty("GoogleAgenda");
        Assert.False(google.TryGetProperty("OAuthClientSecretsPath", out _));
        Assert.False(google.TryGetProperty("RedirectUri", out _));
        Assert.False(document.RootElement.TryGetProperty("InitialAdminBootstrap", out _));
        Assert.False(document.RootElement.TryGetProperty("PrivateFileStorage", out _));
    }

    [Fact]
    public void PrivateStorage_RejeitaCredencialS3Parcial()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PrivateFileStorage:Provider"] = "S3",
                ["PrivateFileStorage:BucketName"] = "private-documents",
                ["PrivateFileStorage:AccessKeyId"] = "access-key"
            })
            .Build();
        var services = new ServiceCollection();
        services.AddPrivateFileStorage(configuration, new TestWebHostEnvironment());
        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<OptionsValidationException>(() =>
            provider.GetRequiredService<IOptions<PrivateFileStorageOptions>>().Value);

        Assert.Contains("devem ser informados juntos", exception.Message);
    }

    [Fact]
    public void PrivateStorage_RejeitaProviderLocalEmProducao()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PrivateFileStorage:Provider"] = "Local"
            })
            .Build();
        var services = new ServiceCollection();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddPrivateFileStorage(
                configuration,
                new TestWebHostEnvironment { EnvironmentName = Environments.Production }));

        Assert.Contains("Provider=S3", exception.Message);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "IkkonAdmin.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Raiz do repositório não encontrada.");
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "IkkonAdmin.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
