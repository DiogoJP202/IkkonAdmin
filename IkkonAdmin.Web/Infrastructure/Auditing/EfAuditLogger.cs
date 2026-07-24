using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;

namespace IkkonAdmin.Web.Infrastructure.Auditing;

public sealed class EfAuditLogger(
    ApplicationDbContext dbContext,
    IClock clock) : IAuditLogger
{
    public async Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        await dbContext.AuditoriaLogs.AddAsync(new AuditoriaLog
        {
            UsuarioResponsavelId = entry.UsuarioResponsavelId,
            UsuarioAfetadoId = entry.UsuarioAfetadoId,
            Acao = entry.Acao,
            Entidade = entry.Entidade,
            EntidadeId = entry.EntidadeId,
            Descricao = entry.Descricao,
            DadosAntesJson = entry.DadosAntesJson,
            DadosDepoisJson = entry.DadosDepoisJson,
            EnderecoIp = LimparIp(entry.EnderecoIp),
            DataEventoUtc = clock.UtcNow
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? LimparIp(string? ip)
    {
        var valor = ip?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }
}
