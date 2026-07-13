using System.Text.Json;
using System.Text.RegularExpressions;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AdminPainelService(
    ApplicationDbContext dbContext,
    IPasswordHasher<UsuarioSistema> passwordHasher) : IAdminPainelService
{
    public async Task<AdminPainelViewModel> ObterPainelAsync(CancellationToken cancellationToken = default)
    {
        var agoraUtc = DateTime.UtcNow;
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

    public async Task<AdminOperationResult> CriarUsuarioAsync(
        AdminUsuarioFormViewModel model,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.SenhaInicial))
        {
            return AdminOperationResult.Fail("Informe uma senha inicial para o usuário.");
        }

        if (!IsStrongPassword(model.SenhaInicial))
        {
            return AdminOperationResult.Fail("A senha inicial deve ter 8+ caracteres, com letra maiuscula, minuscula, numero e simbolo.");
        }

        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == model.RoleId && x.Ativo, cancellationToken);

        if (role is null)
        {
            return AdminOperationResult.Fail("Cargo inválido para o novo usuário.");
        }

        var loginNormalizado = Normalize(model.Login);
        var emailNormalizado = Normalize(model.Email);

        var loginEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.LoginNormalizado == loginNormalizado, cancellationToken);
        if (loginEmUso)
        {
            return AdminOperationResult.Fail("Este login já está em uso.");
        }

        var emailEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);
        if (emailEmUso)
        {
            return AdminOperationResult.Fail("Este e-mail já está em uso.");
        }

        var usuario = new UsuarioSistema
        {
            NomeExibicao = model.NomeExibicao.Trim(),
            Login = model.Login.Trim(),
            LoginNormalizado = loginNormalizado,
            Email = model.Email.Trim(),
            EmailNormalizado = emailNormalizado,
            Telefone = LimparTextoOpcional(model.Telefone),
            TipoAcesso = role.TipoAcesso,
            Ativo = model.Ativo,
            Excluido = false,
            DataCriacaoUtc = DateTime.UtcNow
        };

        usuario.SenhaHash = passwordHasher.HashPassword(usuario, model.SenhaInicial);

        await dbContext.UsuariosSistema.AddAsync(usuario, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await SincronizarRoleUsuarioAsync(usuario.Id, role.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            usuario.Id,
            "CRIAR_USUARIO",
            nameof(UsuarioSistema),
            usuario.Id.ToString(),
            $"Usuário {usuario.NomeExibicao} criado.",
            null,
            SnapshotUsuario(usuario),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Usuário criado com sucesso.");
    }

    public async Task<AdminOperationResult> AtualizarUsuarioAsync(
        int id,
        AdminUsuarioFormViewModel model,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (usuario is null || usuario.Excluido)
        {
            return AdminOperationResult.Fail("Usuário não encontrado.");
        }

        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == model.RoleId && x.Ativo, cancellationToken);

        if (role is null)
        {
            return AdminOperationResult.Fail("Cargo inválido para o usuário.");
        }

        var loginNormalizado = Normalize(model.Login);
        var emailNormalizado = Normalize(model.Email);

        var loginEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.LoginNormalizado == loginNormalizado, cancellationToken);
        if (loginEmUso)
        {
            return AdminOperationResult.Fail("Este login já está em uso.");
        }

        var emailEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.EmailNormalizado == emailNormalizado, cancellationToken);
        if (emailEmUso)
        {
            return AdminOperationResult.Fail("Este e-mail já está em uso.");
        }

        var antes = SnapshotUsuario(usuario);

        usuario.NomeExibicao = model.NomeExibicao.Trim();
        usuario.Login = model.Login.Trim();
        usuario.LoginNormalizado = loginNormalizado;
        usuario.Email = model.Email.Trim();
        usuario.EmailNormalizado = emailNormalizado;
        usuario.Telefone = LimparTextoOpcional(model.Telefone);
        usuario.TipoAcesso = role.TipoAcesso;
        usuario.Ativo = model.Ativo;

        await SincronizarRoleUsuarioAsync(usuario.Id, role.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            usuario.Id,
            "EDITAR_USUARIO",
            nameof(UsuarioSistema),
            usuario.Id.ToString(),
            $"Usuário {usuario.NomeExibicao} atualizado.",
            antes,
            SnapshotUsuario(usuario),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Usuário atualizado com sucesso.");
    }

    public async Task<AdminOperationResult> AlterarStatusUsuarioAsync(
        int id,
        bool ativo,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (usuario is null || usuario.Excluido)
        {
            return AdminOperationResult.Fail("Usuário não encontrado.");
        }

        if (usuario.Id == usuarioResponsavelId && !ativo)
        {
            return AdminOperationResult.Fail("Não é permitido desativar o próprio usuário.");
        }

        if (usuario.Ativo == ativo)
        {
            return AdminOperationResult.Ok("Status já estava atualizado.");
        }

        var antes = SnapshotUsuario(usuario);
        usuario.Ativo = ativo;
        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            usuario.Id,
            ativo ? "ATIVAR_USUARIO" : "DESATIVAR_USUARIO",
            nameof(UsuarioSistema),
            usuario.Id.ToString(),
            $"Usuário {(ativo ? "ativado" : "desativado")}.",
            antes,
            SnapshotUsuario(usuario),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Status atualizado com sucesso.");
    }

    public async Task<AdminOperationResult> ExcluirUsuarioAsync(
        int id,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (usuario is null)
        {
            return AdminOperationResult.Fail("Usuário não encontrado.");
        }

        if (usuario.Excluido)
        {
            return AdminOperationResult.Ok("Usuário já estava excluído.");
        }

        if (usuario.Id == usuarioResponsavelId)
        {
            return AdminOperationResult.Fail("Não é permitido excluir o próprio usuário.");
        }

        var antes = SnapshotUsuario(usuario);

        usuario.Excluido = true;
        usuario.Ativo = false;
        usuario.DataExclusaoUtc = DateTime.UtcNow;
        usuario.ExcluidoPorUsuarioId = usuarioResponsavelId;

        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            usuario.Id,
            "EXCLUIR_USUARIO",
            nameof(UsuarioSistema),
            usuario.Id.ToString(),
            "Usuário removido (soft delete).",
            antes,
            SnapshotUsuario(usuario),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Usuário excluído com sucesso.");
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

    public async Task<AdminOperationResult> AtualizarAcessosAsync(
        AdminAcessosUpdateRequest request,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var usuario = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == request.UsuarioId, cancellationToken);

        if (usuario is null || usuario.Excluido)
        {
            return AdminOperationResult.Fail("Usuário não encontrado.");
        }

        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == request.RoleId && x.Ativo, cancellationToken);

        if (role is null)
        {
            return AdminOperationResult.Fail("Cargo inválido.");
        }

        var permissoesValidas = await dbContext.PermissoesSistema
            .Where(x => request.PermissoesDiretas.Contains(x.Id) && x.Ativo)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var antes = await SnapshotAcessosAsync(usuario.Id, cancellationToken);

        await SincronizarRoleUsuarioAsync(usuario.Id, role.Id, cancellationToken);

        var userPerms = dbContext.UsuariosPermissoes.Where(x => x.UsuarioId == usuario.Id);
        dbContext.UsuariosPermissoes.RemoveRange(userPerms);

        foreach (var permissaoId in permissoesValidas.Distinct())
        {
            await dbContext.UsuariosPermissoes.AddAsync(new UsuarioPermissao
            {
                UsuarioId = usuario.Id,
                PermissaoId = permissaoId,
                DataConcessaoUtc = DateTime.UtcNow
            }, cancellationToken);
        }

        usuario.TipoAcesso = role.TipoAcesso;
        await dbContext.SaveChangesAsync(cancellationToken);

        var depois = await SnapshotAcessosAsync(usuario.Id, cancellationToken);
        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            usuario.Id,
            "EDITAR_ACESSOS",
            nameof(UsuarioSistema),
            usuario.Id.ToString(),
            "Permissões e cargo do usuário atualizados.",
            antes,
            depois,
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Acessos atualizados com sucesso.");
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

    public async Task<AdminOperationResult> CriarRoleAsync(
        AdminRoleFormViewModel model,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var nome = model.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            return AdminOperationResult.Fail("Informe um nome valido para o cargo.");
        }

        var codigo = NormalizarCodigoRole(model.Codigo, nome);
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return AdminOperationResult.Fail("Não foi possível gerar um código válido para o cargo.");
        }

        if (codigo.Length > 60)
        {
            return AdminOperationResult.Fail("Codigo do cargo muito longo. Use um nome/codigo menor.");
        }

        var codigoEmUso = await dbContext.RolesSistema
            .AnyAsync(x => x.Codigo == codigo, cancellationToken);

        if (codigoEmUso)
        {
            return AdminOperationResult.Fail("Já existe um cargo com este código.");
        }

        var permissoesIds = await ValidarPermissoesSelecionadasAsync(model.PermissoesSelecionadas, cancellationToken);

        var role = new RoleSistema
        {
            Codigo = codigo,
            Nome = nome,
            Descricao = LimparTextoOpcional(model.Descricao),
            TipoAcesso = model.TipoAcesso,
            Ativo = model.Ativo,
            IsSistema = false,
            DataCriacaoUtc = DateTime.UtcNow
        };

        await dbContext.RolesSistema.AddAsync(role, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await SincronizarPermissoesRoleAsync(role.Id, permissoesIds, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            null,
            "CRIAR_ROLE",
            nameof(RoleSistema),
            role.Id.ToString(),
            $"Cargo {role.Nome} criado.",
            null,
            SnapshotRole(role),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Cargo criado com sucesso.");
    }

    public async Task<AdminOperationResult> AtualizarRoleAsync(
        int id,
        AdminRoleFormViewModel model,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (role is null)
        {
            return AdminOperationResult.Fail("Cargo não encontrado.");
        }

        var nome = model.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            return AdminOperationResult.Fail("Informe um nome valido para o cargo.");
        }

        var codigo = role.IsSistema
            ? role.Codigo
            : NormalizarCodigoRole(model.Codigo, nome);

        if (!role.IsSistema)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return AdminOperationResult.Fail("Não foi possível gerar um código válido para o cargo.");
            }

            if (codigo.Length > 60)
            {
                return AdminOperationResult.Fail("Codigo do cargo muito longo. Use um nome/codigo menor.");
            }

            var codigoEmUso = await dbContext.RolesSistema
                .AnyAsync(x => x.Id != role.Id && x.Codigo == codigo, cancellationToken);

            if (codigoEmUso)
            {
                return AdminOperationResult.Fail("Já existe um cargo com este código.");
            }
        }

        var antes = SnapshotRole(role);
        var isAdminRole = IsAdminRoleCode(role.Codigo);
        var permissoesIds = isAdminRole
            ? await ObterTodasPermissoesAtivasIdsAsync(cancellationToken)
            : await ValidarPermissoesSelecionadasAsync(model.PermissoesSelecionadas, cancellationToken);

        role.Nome = nome;
        role.Descricao = LimparTextoOpcional(model.Descricao);

        if (role.IsSistema)
        {
            role.Ativo = true;
            if (isAdminRole)
            {
                role.TipoAcesso = TipoAcessoEnum.Admin;
            }
        }
        else
        {
            role.Codigo = codigo;
            role.TipoAcesso = model.TipoAcesso;
            role.Ativo = model.Ativo;

            var usuariosIds = await dbContext.UsuariosRoles
                .Where(x => x.RoleId == role.Id)
                .Select(x => x.UsuarioId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (usuariosIds.Count > 0)
            {
                var usuarios = await dbContext.UsuariosSistema
                    .IgnoreQueryFilters()
                    .Where(x => usuariosIds.Contains(x.Id) && !x.Excluido)
                    .ToListAsync(cancellationToken);

                foreach (var usuario in usuarios)
                {
                    usuario.TipoAcesso = role.TipoAcesso;
                }
            }
        }

        await SincronizarPermissoesRoleAsync(role.Id, permissoesIds, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            null,
            "EDITAR_ROLE",
            nameof(RoleSistema),
            role.Id.ToString(),
            $"Cargo {role.Nome} atualizado.",
            antes,
            SnapshotRole(role),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Cargo atualizado com sucesso.");
    }

    public async Task<AdminOperationResult> AlterarStatusRoleAsync(
        int id,
        bool ativo,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (role is null)
        {
            return AdminOperationResult.Fail("Cargo não encontrado.");
        }

        if (role.IsSistema)
        {
            return AdminOperationResult.Fail("Não é permitido alterar status de cargos do sistema.");
        }

        if (role.Ativo == ativo)
        {
            return AdminOperationResult.Ok("Status do cargo já estava atualizado.");
        }

        var antes = SnapshotRole(role);
        role.Ativo = ativo;

        if (!ativo)
        {
            await ReatribuirUsuariosDoCargoParaPadraoAsync(role, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            null,
            ativo ? "ATIVAR_ROLE" : "DESATIVAR_ROLE",
            nameof(RoleSistema),
            role.Id.ToString(),
            $"Cargo {(ativo ? "ativado" : "desativado")}: {role.Nome}.",
            antes,
            SnapshotRole(role),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Status do cargo atualizado com sucesso.");
    }

    public async Task<AdminOperationResult> ExcluirRoleAsync(
        int id,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (role is null)
        {
            return AdminOperationResult.Fail("Cargo não encontrado.");
        }

        if (role.IsSistema)
        {
            return AdminOperationResult.Fail("Não é permitido remover cargos do sistema.");
        }

        var antes = SnapshotRole(role);

        role.Ativo = false;
        role.Nome = role.Nome.StartsWith("[REMOVIDO]", StringComparison.OrdinalIgnoreCase)
            ? role.Nome
            : $"[REMOVIDO] {role.Nome}";
        role.Codigo = MontarCodigoRemovido(role.Codigo, role.Id);

        await ReatribuirUsuariosDoCargoParaPadraoAsync(role, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await RegistrarAuditoriaAsync(
            usuarioResponsavelId,
            null,
            "EXCLUIR_ROLE",
            nameof(RoleSistema),
            role.Id.ToString(),
            "Cargo removido (inativado).",
            antes,
            SnapshotRole(role),
            enderecoIp,
            cancellationToken);

        return AdminOperationResult.Ok("Cargo removido com sucesso.");
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

    private async Task<List<int>> ValidarPermissoesSelecionadasAsync(
        IEnumerable<int>? permissoesSelecionadas,
        CancellationToken cancellationToken)
    {
        var ids = (permissoesSelecionadas ?? [])
            .Distinct()
            .ToArray();

        if (ids.Length == 0)
        {
            return [];
        }

        return await dbContext.PermissoesSistema
            .Where(x => ids.Contains(x.Id) && x.Ativo)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
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

    private async Task SincronizarRoleUsuarioAsync(int usuarioId, int roleId, CancellationToken cancellationToken)
    {
        var userRoles = dbContext.UsuariosRoles.Where(x => x.UsuarioId == usuarioId);
        dbContext.UsuariosRoles.RemoveRange(userRoles);

        await dbContext.UsuariosRoles.AddAsync(new UsuarioRole
        {
            UsuarioId = usuarioId,
            RoleId = roleId,
            DataVinculoUtc = DateTime.UtcNow
        }, cancellationToken);
    }

    private async Task SincronizarPermissoesRoleAsync(int roleId, IReadOnlyCollection<int> permisssoesIds, CancellationToken cancellationToken)
    {
        var roleCode = await dbContext.RolesSistema
            .Where(x => x.Id == roleId)
            .Select(x => x.Codigo)
            .FirstOrDefaultAsync(cancellationToken);

        if (IsAdminRoleCode(roleCode))
        {
            permisssoesIds = await ObterTodasPermissoesAtivasIdsAsync(cancellationToken);
        }

        var existentes = dbContext.RolesPermissoes.Where(x => x.RoleId == roleId);
        dbContext.RolesPermissoes.RemoveRange(existentes);

        foreach (var permissaoId in permisssoesIds.Distinct())
        {
            await dbContext.RolesPermissoes.AddAsync(new RolePermissao
            {
                RoleId = roleId,
                PermissaoId = permissaoId,
                DataVinculoUtc = DateTime.UtcNow
            }, cancellationToken);
        }
    }

    private async Task ReatribuirUsuariosDoCargoParaPadraoAsync(RoleSistema role, CancellationToken cancellationToken)
    {
        var rolePadraoId = await ObterRolePadraoIdPorTipoAsync(role.TipoAcesso, cancellationToken);
        if (!rolePadraoId.HasValue)
        {
            return;
        }

        var usuariosIds = await dbContext.UsuariosRoles
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.UsuarioId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var usuarioId in usuariosIds)
        {
            await SincronizarRoleUsuarioAsync(usuarioId, rolePadraoId.Value, cancellationToken);
        }
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

    private static string SnapshotUsuario(UsuarioSistema usuario)
    {
        var payload = new
        {
            usuario.Id,
            usuario.NomeExibicao,
            usuario.Login,
            usuario.Email,
            usuario.Telefone,
            usuario.TipoAcesso,
            usuario.Ativo,
            usuario.Excluido
        };

        return JsonSerializer.Serialize(payload);
    }

    private static string SnapshotRole(RoleSistema role)
    {
        var payload = new
        {
            role.Id,
            role.Codigo,
            role.Nome,
            role.Descricao,
            role.TipoAcesso,
            role.Ativo,
            role.IsSistema
        };

        return JsonSerializer.Serialize(payload);
    }

    private async Task<string> SnapshotAcessosAsync(int usuarioId, CancellationToken cancellationToken)
    {
        var roles = await dbContext.UsuariosRoles
            .Where(x => x.UsuarioId == usuarioId)
            .Select(x => x.Role != null ? x.Role.Codigo : string.Empty)
            .Where(x => x != string.Empty)
            .ToListAsync(cancellationToken);

        var permissoes = await dbContext.UsuariosPermissoes
            .Where(x => x.UsuarioId == usuarioId)
            .Select(x => x.Permissao != null ? x.Permissao.Codigo : string.Empty)
            .Where(x => x != string.Empty)
            .ToListAsync(cancellationToken);

        return JsonSerializer.Serialize(new
        {
            UsuarioId = usuarioId,
            Roles = roles,
            PermissoesDiretas = permissoes
        });
    }

    private async Task RegistrarAuditoriaAsync(
        int? usuarioResponsavelId,
        int? usuarioAfetadoId,
        string acao,
        string entidade,
        string? entidadeId,
        string? descricao,
        string? dadosAntesJson,
        string? dadosDepoisJson,
        string? enderecoIp,
        CancellationToken cancellationToken)
    {
        await dbContext.AuditoriaLogs.AddAsync(new AuditoriaLog
        {
            UsuarioResponsavelId = usuarioResponsavelId,
            UsuarioAfetadoId = usuarioAfetadoId,
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Descricao = descricao,
            DadosAntesJson = dadosAntesJson,
            DadosDepoisJson = dadosDepoisJson,
            EnderecoIp = LimparIp(enderecoIp),
            DataEventoUtc = DateTime.UtcNow
        }, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? LimparIp(string? ip)
    {
        var valor = ip?.Trim();
        return string.IsNullOrWhiteSpace(valor) ? null : valor;
    }

    private static string? LimparTextoOpcional(string? value)
    {
        var texto = value?.Trim();
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static string NormalizarCodigoRole(string? codigo, string nomeFallback)
    {
        var baseCodigo = string.IsNullOrWhiteSpace(codigo) ? nomeFallback : codigo;

        var normalizado = baseCodigo.Trim().ToUpperInvariant();
        normalizado = normalizado.Replace(" ", "_");
        normalizado = Regex.Replace(normalizado, "[^A-Z0-9_]", string.Empty);
        normalizado = Regex.Replace(normalizado, "_{2,}", "_").Trim('_');

        if (string.IsNullOrWhiteSpace(normalizado))
        {
            return string.Empty;
        }

        if (!normalizado.StartsWith("ROLE_", StringComparison.Ordinal))
        {
            normalizado = $"ROLE_{normalizado}";
        }

        return normalizado;
    }

    private static string MontarCodigoRemovido(string codigoAtual, int roleId)
    {
        var sufixo = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var codigoNovo = $"REMOVED_{roleId}_{sufixo}";

        if (codigoNovo.Length <= 60)
        {
            return codigoNovo;
        }

        return $"REMOVED_{roleId}_{DateTime.UtcNow:yyyyMMdd}";
    }

    private static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            return false;
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSymbol;
    }
}
