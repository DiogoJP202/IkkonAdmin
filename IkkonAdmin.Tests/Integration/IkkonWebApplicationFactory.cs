using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Files;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace IkkonAdmin.Tests.Integration;

internal sealed class IkkonWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"ikkon-integration-{Guid.NewGuid():N}";

    public InMemoryPrivateFileStorage PrivateFileStorage { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IPrivateFileStorageService>();

            var inMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();
            services.AddDbContext<ApplicationDbContext>(options => options
                .UseInMemoryDatabase(databaseName)
                .UseInternalServiceProvider(inMemoryProvider));
            services.AddSingleton(PrivateFileStorage);
            services.AddSingleton<IPrivateFileStorageService>(PrivateFileStorage);
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                    options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                    options.DefaultForbidScheme = TestAuthHandler.SchemeName;
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
        });
    }

    public HttpClient CreateAuthenticatedClient(
        int userId,
        IEnumerable<string> roles,
        IEnumerable<string>? permissions = null,
        bool allowAutoRedirect = false)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = allowAutoRedirect,
            HandleCookies = true
        });
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RolesHeader, string.Join(',', roles));

        if (permissions is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.PermissionsHeader, string.Join(',', permissions));
        }

        return client;
    }

    public async Task SeedAsync(Func<ApplicationDbContext, Task> seed)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        await seed(dbContext);
        await dbContext.SaveChangesAsync();
    }

    public async Task<TResult> ExecuteDbAsync<TResult>(
        Func<ApplicationDbContext, Task<TResult>> operation)
    {
        await using var scope = Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await operation(dbContext);
    }
}
