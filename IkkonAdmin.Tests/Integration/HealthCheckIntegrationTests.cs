using System.Net;

namespace IkkonAdmin.Tests.Integration;

public sealed class HealthCheckIntegrationTests
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/health/live")]
    public async Task Liveness_RespondeSemConsultarDependencias(string path)
    {
        await using var factory = new IkkonWebApplicationFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("application", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Readiness_VerificaSqlEStorageSeparadamente()
    {
        await using var factory = new IkkonWebApplicationFactory();
        await factory.SeedAsync(_ => Task.CompletedTask);
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health/ready");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("sql", body);
        Assert.Contains("storage", body);
    }
}
