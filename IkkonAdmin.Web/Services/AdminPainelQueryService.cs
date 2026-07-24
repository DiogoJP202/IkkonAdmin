using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AdminPainelQueryService(
    ApplicationDbContext dbContext,
    IClock clock) : IAdminPainelQueryService
{
    public async Task<AdminPainelViewModel> ObterPainelAsync(CancellationToken cancellationToken = default)
    {
        var agoraUtc = clock.UtcNow;
        var inicio24h = agoraUtc.AddHours(-24);

        var totalUsuarios = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .CountAsync(x => !x.Excluido, cancellationToken);

        var usuariosAtivos = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .CountAsync(x => !x.Excluido && x.Ativo, cancellationToken);

        var usuariosAdmins = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .CountAsync(x => !x.Excluido && x.TipoAcesso == TipoAcessoEnum.Admin, cancellationToken);

        var usuariosFuncionarios = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .CountAsync(x => !x.Excluido && x.TipoAcesso == TipoAcessoEnum.Funcionario, cancellationToken);

        var usuariosAlunos = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .CountAsync(x => !x.Excluido && x.TipoAcesso == TipoAcessoEnum.Aluno, cancellationToken);

        var totalCargosAtivos = await dbContext.RolesSistema
            .AsNoTracking()
            .CountAsync(x => x.Ativo, cancellationToken);

        var logsUltimas24h = await dbContext.AuditoriaLogs
            .CountAsync(x => x.DataEventoUtc >= inicio24h, cancellationToken);

        var atividadesRecentes = await dbContext.AuditoriaLogs
            .AsNoTracking()
            .OrderByDescending(x => x.DataEventoUtc)
            .Take(8)
            .Select(x => new AdminLogListItemViewModel
            {
                Id = x.Id,
                DataEventoUtc = x.DataEventoUtc,
                Acao = x.Acao,
                Entidade = x.Entidade,
                EntidadeId = x.EntidadeId,
                Descricao = x.Descricao,
                ResponsavelNome = x.UsuarioResponsavel != null ? x.UsuarioResponsavel.NomeExibicao : null,
                AfetadoNome = x.UsuarioAfetado != null ? x.UsuarioAfetado.NomeExibicao : null
            })
            .ToListAsync(cancellationToken);

        return new AdminPainelViewModel
        {
            TotalUsuarios = totalUsuarios,
            UsuariosAtivos = usuariosAtivos,
            UsuariosAdmins = usuariosAdmins,
            UsuariosFuncionarios = usuariosFuncionarios,
            UsuariosAlunos = usuariosAlunos,
            TotalCargosAtivos = totalCargosAtivos,
            LogsUltimas24h = logsUltimas24h,
            AtividadesRecentes = atividadesRecentes
        };
    }

    public async Task<AdminUsuariosIndexViewModel> ListarUsuariosAsync(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        bool incluirExcluidos,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = tamanhoPagina is 10 or 20 or 30 ? tamanhoPagina : 20;

        var query = dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AsNoTracking();

        if (!incluirExcluidos)
        {
            query = query.Where(x => !x.Excluido);
        }

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(x =>
                x.NomeExibicao.Contains(termo) ||
                (x.Email != null && x.Email.Contains(termo)) ||
                x.Login.Contains(termo));
        }

        if (tipo.HasValue)
        {
            query = query.Where(x => x.TipoAcesso == tipo.Value);
        }

        if (ativo.HasValue)
        {
            query = query.Where(x => x.Ativo == ativo.Value);
        }

        var totalRegistros = await query.CountAsync(cancellationToken);

        var usuariosBase = await query
            .OrderByDescending(x => x.DataCriacaoUtc)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new
            {
                x.Id,
                x.NomeExibicao,
                x.Email,
                x.Login,
                x.TipoAcesso,
                x.Ativo,
                x.Excluido,
                x.DataCriacaoUtc,
                x.UltimoLoginUtc
            })
            .ToListAsync(cancellationToken);

        var usuarioIds = usuariosBase.Select(x => x.Id).ToList();
        var rolesUsuarios = await dbContext.UsuariosRoles
            .AsNoTracking()
            .Where(x => usuarioIds.Contains(x.UsuarioId) && x.Role != null)
            .OrderByDescending(x => x.DataVinculoUtc)
            .Select(x => new
            {
                x.UsuarioId,
                x.RoleId,
                RoleNome = x.Role!.Nome,
                RoleCodigo = x.Role.Codigo
            })
            .ToListAsync(cancellationToken);

        var roleByUsuario = rolesUsuarios
            .GroupBy(x => x.UsuarioId)
            .ToDictionary(x => x.Key, x => x.First());

        var usuarios = usuariosBase
            .Select(x =>
            {
                roleByUsuario.TryGetValue(x.Id, out var role);
                return new AdminUsuarioListItemViewModel
                {
                    Id = x.Id,
                    Nome = x.NomeExibicao,
                    Email = x.Email,
                    Login = x.Login,
                    TipoAcesso = x.TipoAcesso,
                    RoleId = role?.RoleId,
                    RoleNome = role?.RoleNome,
                    RoleCodigo = role?.RoleCodigo,
                    Ativo = x.Ativo,
                    Excluido = x.Excluido,
                    DataCriacaoUtc = x.DataCriacaoUtc,
                    UltimoLoginUtc = x.UltimoLoginUtc
                };
            })
            .ToList();

        return new AdminUsuariosIndexViewModel
        {
            Busca = busca,
            Tipo = tipo,
            Ativo = ativo,
            IncluirExcluidos = incluirExcluidos,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            Usuarios = usuarios
        };
    }

    public async Task<List<AdminRoleSelectItemViewModel>> ListarRolesAtivasAsync(
        TipoAcessoEnum? tipoAcesso,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.RolesSistema
            .AsNoTracking()
            .Where(x => x.Ativo);

        if (tipoAcesso.HasValue)
        {
            query = query.Where(x => x.TipoAcesso == tipoAcesso.Value);
        }

        return await query
            .OrderBy(x => x.TipoAcesso)
            .ThenByDescending(x => x.IsSistema)
            .ThenBy(x => x.Nome)
            .Select(x => new AdminRoleSelectItemViewModel
            {
                Id = x.Id,
                Codigo = x.Codigo,
                Nome = x.Nome,
                TipoAcesso = x.TipoAcesso,
                Ativo = x.Ativo,
                IsSistema = x.IsSistema
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminUsuarioFormViewModel?> ObterUsuarioParaEdicaoAsync(int id, CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (usuario is null || usuario.Excluido)
        {
            return null;
        }

        var roleId = await ObterRoleAtualUsuarioAsync(usuario.Id, cancellationToken)
            ?? await ObterRolePadraoIdPorTipoAsync(usuario.TipoAcesso, cancellationToken)
            ?? 0;

        return new AdminUsuarioFormViewModel
        {
            Id = usuario.Id,
            NomeExibicao = usuario.NomeExibicao,
            Login = usuario.Login,
            Email = usuario.Email ?? string.Empty,
            Telefone = usuario.Telefone,
            TipoAcesso = usuario.TipoAcesso,
            RoleId = roleId,
            RolesDisponiveis = await ListarRolesAtivasAsync(null, cancellationToken),
            Ativo = usuario.Ativo
        };
    }

    public async Task<AdminAcessosViewModel?> ObterAcessosAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == usuarioId, cancellationToken);

        if (usuario is null || usuario.Excluido)
        {
            return null;
        }

        var roles = await ListarRolesAtivasAsync(null, cancellationToken);

        var roleSelecionadaId = await ObterRoleAtualUsuarioAsync(usuarioId, cancellationToken);

        if (!roleSelecionadaId.HasValue)
        {
            roleSelecionadaId = await ObterRolePadraoIdPorTipoAsync(usuario.TipoAcesso, cancellationToken);
        }

        var permissoesRole = roleSelecionadaId.HasValue
            ? await dbContext.RolesPermissoes
                .Where(x => x.RoleId == roleSelecionadaId.Value)
                .Select(x => x.PermissaoId)
                .ToListAsync(cancellationToken)
            : new List<int>();

        var permissoesDiretas = await dbContext.UsuariosPermissoes
            .Where(x => x.UsuarioId == usuarioId)
            .Select(x => x.PermissaoId)
            .ToListAsync(cancellationToken);

        var permissoes = await ListarPermissoesSelectAsync(permissoesDiretas, permissoesRole, cancellationToken);

        return new AdminAcessosViewModel
        {
            UsuarioId = usuario.Id,
            NomeUsuario = usuario.NomeExibicao,
            EmailUsuario = usuario.Email ?? "-",
            TipoAcesso = usuario.TipoAcesso,
            RoleSelecionadaId = roleSelecionadaId ?? 0,
            RolesDisponiveis = roles,
            PermissoesDisponiveis = permissoes
        };
    }

    public async Task<AdminRolesIndexViewModel> ListarRolesAsync(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = tamanhoPagina is 10 or 20 or 30 ? tamanhoPagina : 20;

        var query = dbContext.RolesSistema.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(x =>
                x.Nome.Contains(termo) ||
                x.Codigo.Contains(termo) ||
                (x.Descricao != null && x.Descricao.Contains(termo)));
        }

        if (tipo.HasValue)
        {
            query = query.Where(x => x.TipoAcesso == tipo.Value);
        }

        if (ativo.HasValue)
        {
            query = query.Where(x => x.Ativo == ativo.Value);
        }

        var totalRegistros = await query.CountAsync(cancellationToken);

        var rolesBase = await query
            .OrderByDescending(x => x.IsSistema)
            .ThenBy(x => x.TipoAcesso)
            .ThenBy(x => x.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new
            {
                x.Id,
                x.Codigo,
                x.Nome,
                x.Descricao,
                x.TipoAcesso,
                x.Ativo,
                x.IsSistema,
                x.DataCriacaoUtc
            })
            .ToListAsync(cancellationToken);

        var roleIds = rolesBase.Select(x => x.Id).ToList();

        var usuariosPorRole = await dbContext.UsuariosRoles
            .AsNoTracking()
            .Where(x => roleIds.Contains(x.RoleId))
            .GroupBy(x => x.RoleId)
            .Select(x => new
            {
                RoleId = x.Key,
                TotalUsuarios = x.Select(y => y.UsuarioId).Distinct().Count()
            })
            .ToListAsync(cancellationToken);

        var permisssoesPorRole = await dbContext.RolesPermissoes
            .AsNoTracking()
            .Where(x => roleIds.Contains(x.RoleId))
            .GroupBy(x => x.RoleId)
            .Select(x => new
            {
                RoleId = x.Key,
                TotalPermissoes = x.Count()
            })
            .ToListAsync(cancellationToken);

        var totalUsuariosByRole = usuariosPorRole.ToDictionary(x => x.RoleId, x => x.TotalUsuarios);
        var totalPermissoesByRole = permisssoesPorRole.ToDictionary(x => x.RoleId, x => x.TotalPermissoes);

        var roles = rolesBase
            .Select(x => new AdminRoleListItemViewModel
            {
                Id = x.Id,
                Codigo = x.Codigo,
                Nome = x.Nome,
                Descricao = x.Descricao,
                TipoAcesso = x.TipoAcesso,
                Ativo = x.Ativo,
                IsSistema = x.IsSistema,
                TotalUsuarios = totalUsuariosByRole.TryGetValue(x.Id, out var totalUsuarios) ? totalUsuarios : 0,
                TotalPermissoes = totalPermissoesByRole.TryGetValue(x.Id, out var totalPermissoes) ? totalPermissoes : 0,
                DataCriacaoUtc = x.DataCriacaoUtc
            })
            .ToList();

        return new AdminRolesIndexViewModel
        {
            Busca = busca,
            Tipo = tipo,
            Ativo = ativo,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            Roles = roles
        };
    }

    public async Task<AdminRoleFormViewModel> ObterRoleParaCriacaoAsync(CancellationToken cancellationToken = default)
    {
        return new AdminRoleFormViewModel
        {
            Ativo = true,
            TipoAcesso = TipoAcessoEnum.Funcionario,
            PermissoesDisponiveis = await ListarPermissoesSelectAsync([], [], cancellationToken)
        };
    }

    public async Task<AdminRoleFormViewModel?> ObterRoleParaEdicaoAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await dbContext.RolesSistema
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (role is null)
        {
            return null;
        }

        var isAdminRole = IsAdminRoleCode(role.Codigo);
        var permissoesSelecionadas = isAdminRole
            ? await ObterTodasPermissoesAtivasIdsAsync(cancellationToken)
            : await dbContext.RolesPermissoes
                .Where(x => x.RoleId == role.Id)
                .Select(x => x.PermissaoId)
                .ToListAsync(cancellationToken);

        return new AdminRoleFormViewModel
        {
            Id = role.Id,
            Nome = role.Nome,
            Codigo = role.Codigo,
            Descricao = role.Descricao,
            TipoAcesso = role.TipoAcesso,
            Ativo = role.Ativo,
            IsSistema = role.IsSistema,
            PermissoesSelecionadas = permissoesSelecionadas,
            PermissoesDisponiveis = await ListarPermissoesSelectAsync(permissoesSelecionadas, [], cancellationToken)
        };
    }

    public async Task<AdminLogsIndexViewModel> ListarLogsAsync(
        string? busca,
        int? usuarioResponsavelId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        pagina = Math.Max(1, pagina);
        tamanhoPagina = tamanhoPagina is 10 or 20 or 30 ? tamanhoPagina : 20;

        var query = dbContext.AuditoriaLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim();
            query = query.Where(x =>
                x.Acao.Contains(termo) ||
                x.Entidade.Contains(termo) ||
                (x.Descricao != null && x.Descricao.Contains(termo)));
        }

        if (usuarioResponsavelId.HasValue)
        {
            query = query.Where(x => x.UsuarioResponsavelId == usuarioResponsavelId.Value);
        }

        var totalRegistros = await query.CountAsync(cancellationToken);
        var logs = await query
            .OrderByDescending(x => x.DataEventoUtc)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .Select(x => new AdminLogListItemViewModel
            {
                Id = x.Id,
                DataEventoUtc = x.DataEventoUtc,
                Acao = x.Acao,
                Entidade = x.Entidade,
                EntidadeId = x.EntidadeId,
                Descricao = x.Descricao,
                ResponsavelNome = x.UsuarioResponsavel != null ? x.UsuarioResponsavel.NomeExibicao : null,
                AfetadoNome = x.UsuarioAfetado != null ? x.UsuarioAfetado.NomeExibicao : null
            })
            .ToListAsync(cancellationToken);

        var responsaveis = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => !x.Excluido)
            .OrderBy(x => x.NomeExibicao)
            .Select(x => new AdminFiltroUsuarioViewModel
            {
                Id = x.Id,
                Nome = x.NomeExibicao
            })
            .ToListAsync(cancellationToken);

        return new AdminLogsIndexViewModel
        {
            Busca = busca,
            UsuarioResponsavelId = usuarioResponsavelId,
            PaginaAtual = pagina,
            TamanhoPagina = tamanhoPagina,
            TotalRegistros = totalRegistros,
            Logs = logs,
            Responsaveis = responsaveis
        };
    }

    private async Task<List<AdminPermissaoSelectItemViewModel>> ListarPermissoesSelectAsync(
        IReadOnlyCollection<int> permisssoesConcedidas,
        IReadOnlyCollection<int> permisssoesHerdadas,
        CancellationToken cancellationToken)
    {
        var moduloByCodigo = AppPermissions.Definicoes
            .GroupBy(x => x.Codigo, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.First().Modulo, StringComparer.OrdinalIgnoreCase);

        var permissoes = await dbContext.PermissoesSistema
            .AsNoTracking()
            .Where(x => x.Ativo)
            .OrderBy(x => x.Nome)
            .ToListAsync(cancellationToken);

        return permissoes
            .Select(x => new AdminPermissaoSelectItemViewModel
            {
                Id = x.Id,
                Codigo = x.Codigo,
                Nome = x.Nome,
                Descricao = x.Descricao,
                Modulo = moduloByCodigo.TryGetValue(x.Codigo, out var modulo) ? modulo : "Outros",
                Concedida = permisssoesConcedidas.Contains(x.Id),
                HerdadaDaRole = permisssoesHerdadas.Contains(x.Id)
            })
            .OrderBy(x => x.Modulo)
            .ThenBy(x => x.Nome)
            .ToList();
    }

    private async Task<int?> ObterRoleAtualUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
    {
        return await dbContext.UsuariosRoles
            .Where(x => x.UsuarioId == usuarioId)
            .OrderByDescending(x => x.DataVinculoUtc)
            .Select(x => (int?)x.RoleId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<int?> ObterRolePadraoIdPorTipoAsync(TipoAcessoEnum tipoAcesso, CancellationToken cancellationToken)
    {
        var codigoPadrao = AppRoles.FromTipoAcesso(tipoAcesso);
        return await dbContext.RolesSistema
            .Where(x => x.Codigo == codigoPadrao && x.Ativo)
            .Select(x => (int?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<List<int>> ObterTodasPermissoesAtivasIdsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.PermissoesSistema
            .Where(x => x.Ativo)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    private static bool IsAdminRoleCode(string? roleCode)
    {
        return !string.IsNullOrWhiteSpace(roleCode)
            && roleCode.Equals(AppRoles.Admin, StringComparison.OrdinalIgnoreCase);
    }
}
