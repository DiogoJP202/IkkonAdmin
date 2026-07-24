using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class GoogleAgendaConnectionService(
    ApplicationDbContext dbContext,
    IDataProtectionProvider dataProtectionProvider,
    IClock clock) : IGoogleAgendaConnectionService
{
    private readonly IDataProtector refreshTokenProtector =
        dataProtectionProvider.CreateProtector("IkkonAdmin.GoogleAgenda.RefreshToken.v1");

    public Task<bool> PossuiConexaoOAuthAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.GoogleAgendaConexoes
            .AsNoTracking()
            .AnyAsync(x => x.Ativa, cancellationToken);
    }

    public async Task<string?> ObterRefreshTokenAtivoAsync(CancellationToken cancellationToken = default)
    {
        var conexao = await dbContext.GoogleAgendaConexoes
            .AsNoTracking()
            .Where(x => x.Ativa)
            .OrderByDescending(x => x.CriadoEmUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return conexao is null
            ? null
            : refreshTokenProtector.Unprotect(conexao.RefreshTokenProtegido);
    }

    public async Task SubstituirConexaoAtivaAsync(
        string refreshToken,
        string? escopos,
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var conexoesAtivas = await dbContext.GoogleAgendaConexoes
            .Where(x => x.Ativa)
            .ToListAsync(cancellationToken);

        foreach (var conexaoAtiva in conexoesAtivas)
        {
            conexaoAtiva.Ativa = false;
            conexaoAtiva.AtualizadoEmUtc = clock.UtcNow;
        }

        await dbContext.GoogleAgendaConexoes.AddAsync(new GoogleAgendaConexao
        {
            ContaEmail = null,
            RefreshTokenProtegido = refreshTokenProtector.Protect(refreshToken),
            Escopos = escopos ?? GoogleAgendaConstants.CalendarScope,
            Ativa = true,
            CriadoEmUtc = clock.UtcNow,
            ConectadoPorUsuarioId = usuarioId
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DesconectarOAuthAsync(
        int? usuarioId,
        CancellationToken cancellationToken = default)
    {
        var conexoesAtivas = await dbContext.GoogleAgendaConexoes
            .Where(x => x.Ativa)
            .ToListAsync(cancellationToken);

        foreach (var conexao in conexoesAtivas)
        {
            conexao.Ativa = false;
            conexao.AtualizadoEmUtc = clock.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
