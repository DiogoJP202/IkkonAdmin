using System.Text.Json;
using System.Text.RegularExpressions;
using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Auditing;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class AdminPainelService(
    ApplicationDbContext dbContext,
    IPasswordHasher<UsuarioSistema> passwordHasher,
    IClock clock,
    IAuditLogger auditLogger,
    IAdminPainelQueryService queryService) : IAdminPainelService
{
    public Task<AdminPainelViewModel> ObterPainelAsync(CancellationToken cancellationToken = default)
    {
        return queryService.ObterPainelAsync(cancellationToken);
    }

    public Task<AdminUsuariosIndexViewModel> ListarUsuariosAsync(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        bool incluirExcluidos,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        return queryService.ListarUsuariosAsync(busca, tipo, ativo, incluirExcluidos, pagina, tamanhoPagina, cancellationToken);
    }

    public Task<List<AdminRoleSelectItemViewModel>> ListarRolesAtivasAsync(
        TipoAcessoEnum? tipoAcesso,
        CancellationToken cancellationToken = default)
    {
        return queryService.ListarRolesAtivasAsync(tipoAcesso, cancellationToken);
    }

    public Task<AdminUsuarioFormViewModel?> ObterUsuarioParaEdicaoAsync(int id, CancellationToken cancellationToken = default)
    {
        return queryService.ObterUsuarioParaEdicaoAsync(id, cancellationToken);
    }

    public async Task<OperationResult> CriarUsuarioAsync(
        AdminUsuarioFormViewModel model,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.SenhaInicial))
        {
            return OperationResult.Fail("Informe uma senha inicial para o usuário.");
        }

        if (!IsStrongPassword(model.SenhaInicial))
        {
            return OperationResult.Fail("A senha inicial deve ter 8+ caracteres, com letra maiuscula, minuscula, numero e simbolo.");
        }

        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == model.RoleId && x.Ativo, cancellationToken);

        if (role is null)
        {
            return OperationResult.Fail("Cargo inválido para o novo usuário.");
        }

        var loginNormalizado = Normalize(model.Login);
        var emailNormalizado = Normalize(model.Email);

        var loginEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.LoginNormalizado == loginNormalizado, cancellationToken);
        if (loginEmUso)
        {
            return OperationResult.Conflict("Este login já está em uso.", nameof(model.Login));
        }

        var emailEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.EmailNormalizado == emailNormalizado, cancellationToken);
        if (emailEmUso)
        {
            return OperationResult.Conflict("Este e-mail já está em uso.", nameof(model.Email));
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
            DataCriacaoUtc = clock.UtcNow
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

        return OperationResult.Ok("Usuário criado com sucesso.");
    }

    public async Task<OperationResult> AtualizarUsuarioAsync(
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
            return OperationResult.NotFound("Usuário não encontrado.");
        }

        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == model.RoleId && x.Ativo, cancellationToken);

        if (role is null)
        {
            return OperationResult.Fail("Cargo inválido para o usuário.");
        }

        var loginNormalizado = Normalize(model.Login);
        var emailNormalizado = Normalize(model.Email);

        var loginEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.LoginNormalizado == loginNormalizado, cancellationToken);
        if (loginEmUso)
        {
            return OperationResult.Conflict("Este login já está em uso.", nameof(model.Login));
        }

        var emailEmUso = await dbContext.UsuariosSistema
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Id != id && x.EmailNormalizado == emailNormalizado, cancellationToken);
        if (emailEmUso)
        {
            return OperationResult.Conflict("Este e-mail já está em uso.", nameof(model.Email));
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

        return OperationResult.Ok("Usuário atualizado com sucesso.");
    }

    public async Task<OperationResult> AlterarStatusUsuarioAsync(
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
            return OperationResult.NotFound("Usuário não encontrado.");
        }

        if (usuario.Id == usuarioResponsavelId && !ativo)
        {
            return OperationResult.Forbidden("Não é permitido desativar o próprio usuário.");
        }

        if (usuario.Ativo == ativo)
        {
            return OperationResult.Ok("Status já estava atualizado.");
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

        return OperationResult.Ok("Status atualizado com sucesso.");
    }

    public async Task<OperationResult> ExcluirUsuarioAsync(
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
            return OperationResult.NotFound("Usuário não encontrado.");
        }

        if (usuario.Excluido)
        {
            return OperationResult.Ok("Usuário já estava excluído.");
        }

        if (usuario.Id == usuarioResponsavelId)
        {
            return OperationResult.Forbidden("Não é permitido excluir o próprio usuário.");
        }

        var antes = SnapshotUsuario(usuario);

        usuario.Excluido = true;
        usuario.Ativo = false;
        usuario.DataExclusaoUtc = clock.UtcNow;
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

        return OperationResult.Ok("Usuário excluído com sucesso.");
    }

    public Task<AdminAcessosViewModel?> ObterAcessosAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        return queryService.ObterAcessosAsync(usuarioId, cancellationToken);
    }

    public async Task<OperationResult> AtualizarAcessosAsync(
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
            return OperationResult.NotFound("Usuário não encontrado.");
        }

        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == request.RoleId && x.Ativo, cancellationToken);

        if (role is null)
        {
            return OperationResult.Fail("Cargo inválido.");
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
                DataConcessaoUtc = clock.UtcNow
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

        return OperationResult.Ok("Acessos atualizados com sucesso.");
    }

    public Task<AdminRolesIndexViewModel> ListarRolesAsync(
        string? busca,
        TipoAcessoEnum? tipo,
        bool? ativo,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        return queryService.ListarRolesAsync(busca, tipo, ativo, pagina, tamanhoPagina, cancellationToken);
    }

    public Task<AdminRoleFormViewModel> ObterRoleParaCriacaoAsync(CancellationToken cancellationToken = default)
    {
        return queryService.ObterRoleParaCriacaoAsync(cancellationToken);
    }

    public Task<AdminRoleFormViewModel?> ObterRoleParaEdicaoAsync(int id, CancellationToken cancellationToken = default)
    {
        return queryService.ObterRoleParaEdicaoAsync(id, cancellationToken);
    }

    public async Task<OperationResult> CriarRoleAsync(
        AdminRoleFormViewModel model,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var nome = model.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            return OperationResult.Fail("Informe um nome valido para o cargo.");
        }

        var codigo = NormalizarCodigoRole(model.Codigo, nome);
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return OperationResult.Fail("Não foi possível gerar um código válido para o cargo.");
        }

        if (codigo.Length > 60)
        {
            return OperationResult.Fail("Codigo do cargo muito longo. Use um nome/codigo menor.");
        }

        var codigoEmUso = await dbContext.RolesSistema
            .AnyAsync(x => x.Codigo == codigo, cancellationToken);

        if (codigoEmUso)
        {
            return OperationResult.Conflict("Já existe um cargo com este código.", nameof(model.Codigo));
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
            DataCriacaoUtc = clock.UtcNow
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

        return OperationResult.Ok("Cargo criado com sucesso.");
    }

    public async Task<OperationResult> AtualizarRoleAsync(
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
            return OperationResult.NotFound("Cargo não encontrado.");
        }

        var nome = model.Nome.Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            return OperationResult.Fail("Informe um nome valido para o cargo.");
        }

        var codigo = role.IsSistema
            ? role.Codigo
            : NormalizarCodigoRole(model.Codigo, nome);

        if (!role.IsSistema)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return OperationResult.Fail("Não foi possível gerar um código válido para o cargo.");
            }

            if (codigo.Length > 60)
            {
                return OperationResult.Fail("Codigo do cargo muito longo. Use um nome/codigo menor.");
            }

            var codigoEmUso = await dbContext.RolesSistema
                .AnyAsync(x => x.Id != role.Id && x.Codigo == codigo, cancellationToken);

            if (codigoEmUso)
            {
                return OperationResult.Conflict("Já existe um cargo com este código.", nameof(model.Codigo));
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

        return OperationResult.Ok("Cargo atualizado com sucesso.");
    }

    public async Task<OperationResult> AlterarStatusRoleAsync(
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
            return OperationResult.NotFound("Cargo não encontrado.");
        }

        if (role.IsSistema)
        {
            return OperationResult.Forbidden("Não é permitido alterar status de cargos do sistema.");
        }

        if (role.Ativo == ativo)
        {
            return OperationResult.Ok("Status do cargo já estava atualizado.");
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

        return OperationResult.Ok("Status do cargo atualizado com sucesso.");
    }

    public async Task<OperationResult> ExcluirRoleAsync(
        int id,
        int usuarioResponsavelId,
        string? enderecoIp,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.RolesSistema
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (role is null)
        {
            return OperationResult.NotFound("Cargo não encontrado.");
        }

        if (role.IsSistema)
        {
            return OperationResult.Forbidden("Não é permitido remover cargos do sistema.");
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

        return OperationResult.Ok("Cargo removido com sucesso.");
    }

    public Task<AdminLogsIndexViewModel> ListarLogsAsync(
        string? busca,
        int? usuarioResponsavelId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        return queryService.ListarLogsAsync(busca, usuarioResponsavelId, pagina, tamanhoPagina, cancellationToken);
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
            DataVinculoUtc = clock.UtcNow
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
                DataVinculoUtc = clock.UtcNow
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
        await auditLogger.LogAsync(new AuditLogEntry
        {
            UsuarioResponsavelId = usuarioResponsavelId,
            UsuarioAfetadoId = usuarioAfetadoId,
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Descricao = descricao,
            DadosAntesJson = dadosAntesJson,
            DadosDepoisJson = dadosDepoisJson,
            EnderecoIp = enderecoIp
        }, cancellationToken);
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
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

    private string MontarCodigoRemovido(string codigoAtual, int roleId)
    {
        var sufixo = clock.UtcNow.ToString("yyyyMMddHHmmss");
        var codigoNovo = $"REMOVED_{roleId}_{sufixo}";

        if (codigoNovo.Length <= 60)
        {
            return codigoNovo;
        }

        return $"REMOVED_{roleId}_{clock.UtcNow:yyyyMMdd}";
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
