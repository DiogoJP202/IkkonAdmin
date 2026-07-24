using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class UserSettingsQueryService(ApplicationDbContext dbContext) : IUserSettingsQueryService
{
    public async Task<UserSettingsPageViewModel?> GetPageAsync(
        int userId,
        CancellationToken cancellationToken = default)
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

    private static AccountTypeInfoViewModel BuildAccountType(TipoAcessoEnum tipoAcesso)
    {
        return tipoAcesso switch
        {
            TipoAcessoEnum.Admin => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Administrador",
                PermissoesBasicas =
                [
                    "Acesso total ao painel administrativo",
                    "Gestão de usuários e permissões",
                    "Controle de configurações e auditoria"
                ]
            },
            TipoAcessoEnum.Funcionario => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Funcionário",
                PermissoesBasicas =
                [
                    "Acesso ao painel administrativo interno",
                    "Gestão de alunos, turmas e financeiro",
                    "Visualização de indicadores operacionais"
                ]
            },
            TipoAcessoEnum.Aluno => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Aluno",
                PermissoesBasicas =
                [
                    "Acesso à área exclusiva do aluno",
                    "Consulta de dados e histórico pessoal",
                    "Recebimento de notificações e comunicados"
                ]
            },
            _ => new AccountTypeInfoViewModel
            {
                TipoAcesso = tipoAcesso,
                NomeTipoConta = "Conta",
                PermissoesBasicas = Array.Empty<string>()
            }
        };
    }
}
