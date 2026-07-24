using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class ConfiguracaoSistemaProvider(
    ApplicationDbContext dbContext,
    IClock clock) : IConfiguracaoSistemaProvider
{
    public async Task<ConfiguracaoSistema> ObterOuCriarAsync(CancellationToken cancellationToken = default)
    {
        var config = await dbContext.ConfiguracoesSistema.FirstOrDefaultAsync(cancellationToken);
        if (config is not null)
        {
            return config;
        }

        config = new ConfiguracaoSistema
        {
            UltimaAtualizacaoUtc = clock.UtcNow
        };

        await dbContext.ConfiguracoesSistema.AddAsync(config, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return config;
    }
}
