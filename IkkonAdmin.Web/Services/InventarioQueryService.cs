using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Web.Services;

public class InventarioQueryService(ApplicationDbContext dbContext) : IInventarioQueryService
{
    public async Task<InventarioIndexViewModel> ListarAsync(
        InventarioFiltroViewModel filtro,
        CancellationToken cancellationToken = default)
    {
        filtro.PaginaAtual = Math.Max(1, filtro.PaginaAtual);
        filtro.TamanhoPagina = filtro.TamanhoPagina is 10 or 20 or 30 ? filtro.TamanhoPagina : 20;

        var baseAtivos = dbContext.InventarioItens
            .AsNoTracking()
            .Where(x => x.Ativo);

        var totalItens = await baseAtivos.CountAsync(cancellationToken);
        var itensDisponiveis = await baseAtivos.CountAsync(x => x.Status == InventarioStatusEnum.Disponivel, cancellationToken);
        var itensManutencao = await baseAtivos.CountAsync(x => x.Status == InventarioStatusEnum.Manutencao, cancellationToken);
        var itensIndisponiveis = await baseAtivos.CountAsync(x => x.Status == InventarioStatusEnum.Indisponivel, cancellationToken);

        var query = baseAtivos;

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = filtro.Busca.Trim();
            query = query.Where(x =>
                x.Nome.Contains(termo) ||
                (x.CodigoInterno != null && x.CodigoInterno.Contains(termo)) ||
                (x.Tipo != null && x.Tipo.Contains(termo)) ||
                (x.Descricao != null && x.Descricao.Contains(termo)));
        }

        if (filtro.Categoria.HasValue)
        {
            query = query.Where(x => x.Categoria == filtro.Categoria.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Tipo))
        {
            var tipo = filtro.Tipo.Trim();
            query = query.Where(x => x.Tipo == tipo);
        }

        if (filtro.Status.HasValue)
        {
            query = query.Where(x => x.Status == filtro.Status.Value);
        }

        if (filtro.EstadoConservacao.HasValue)
        {
            query = query.Where(x => x.EstadoConservacao == filtro.EstadoConservacao.Value);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Localizacao))
        {
            var localizacao = filtro.Localizacao.Trim();
            query = query.Where(x => x.Localizacao == localizacao);
        }

        var totalRegistros = await query.CountAsync(cancellationToken);

        var itens = await query
            .OrderBy(x => x.Categoria)
            .ThenBy(x => x.Nome)
            .Skip((filtro.PaginaAtual - 1) * filtro.TamanhoPagina)
            .Take(filtro.TamanhoPagina)
            .Select(x => new InventarioItemViewModel
            {
                Id = x.Id,
                Nome = x.Nome,
                CodigoInterno = x.CodigoInterno,
                Categoria = x.Categoria,
                Tipo = x.Tipo,
                Quantidade = x.Quantidade,
                Status = x.Status,
                EstadoConservacao = x.EstadoConservacao,
                Localizacao = x.Localizacao,
                DisponivelParaAula = x.DisponivelParaAula,
                DisponivelParaEvento = x.DisponivelParaEvento,
                Ativo = x.Ativo
            })
            .ToListAsync(cancellationToken);

        return new InventarioIndexViewModel
        {
            Filtro = filtro,
            TotalRegistros = totalRegistros,
            TotalItens = totalItens,
            ItensDisponiveis = itensDisponiveis,
            ItensManutencao = itensManutencao,
            ItensIndisponiveis = itensIndisponiveis,
            Itens = itens,
            TiposDisponiveis = await ListarTiposDisponiveisAsync(cancellationToken),
            LocalizacoesDisponiveis = await ListarLocalizacoesDisponiveisAsync(cancellationToken)
        };
    }

    public async Task<InventarioDetalhesViewModel?> ObterDetalhesAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.InventarioItens
            .AsNoTracking()
            .Where(x => x.Id == id && x.Ativo)
            .Select(x => new InventarioDetalhesViewModel
            {
                Id = x.Id,
                Nome = x.Nome,
                CodigoInterno = x.CodigoInterno,
                Categoria = x.Categoria,
                Tipo = x.Tipo,
                Descricao = x.Descricao,
                Quantidade = x.Quantidade,
                Status = x.Status,
                EstadoConservacao = x.EstadoConservacao,
                Localizacao = x.Localizacao,
                DisponivelParaAula = x.DisponivelParaAula,
                DisponivelParaEvento = x.DisponivelParaEvento,
                DataAquisicao = x.DataAquisicao,
                ValorEstimado = x.ValorEstimado,
                Observacoes = x.Observacoes,
                CriadoEmUtc = x.CriadoEmUtc,
                AtualizadoEmUtc = x.AtualizadoEmUtc,
                CriadoPorNome = x.CriadoPorUsuario != null ? x.CriadoPorUsuario.NomeExibicao : null,
                AtualizadoPorNome = x.AtualizadoPorUsuario != null ? x.AtualizadoPorUsuario.NomeExibicao : null,
                Ativo = x.Ativo,
                MovimentacoesRecentes = x.Movimentacoes
                    .OrderByDescending(m => m.DataInicioUtc)
                    .Take(8)
                    .Select(m => new InventarioMovimentacaoResumoViewModel
                    {
                        Id = m.Id,
                        TipoMovimentacao = m.TipoMovimentacao,
                        Quantidade = m.Quantidade,
                        DataInicioUtc = m.DataInicioUtc,
                        DataFimUtc = m.DataFimUtc,
                        ResponsavelNome = m.ResponsavelUsuario != null ? m.ResponsavelUsuario.NomeExibicao : null,
                        GoogleEventId = m.GoogleEventId,
                        Observacoes = m.Observacoes
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<InventarioFormViewModel> ObterFormCriacaoAsync(CancellationToken cancellationToken = default)
    {
        return new InventarioFormViewModel
        {
            TiposSugeridos = await ListarTiposSugeridosAsync(cancellationToken)
        };
    }

    public async Task<InventarioFormViewModel?> ObterFormEdicaoAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var item = await dbContext.InventarioItens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.Ativo, cancellationToken);

        if (item is null)
        {
            return null;
        }

        return new InventarioFormViewModel
        {
            Id = item.Id,
            Nome = item.Nome,
            CodigoInterno = item.CodigoInterno,
            Categoria = item.Categoria,
            Tipo = item.Tipo,
            Descricao = item.Descricao,
            Quantidade = item.Quantidade,
            Status = item.Status,
            EstadoConservacao = item.EstadoConservacao,
            Localizacao = item.Localizacao,
            DisponivelParaAula = item.DisponivelParaAula,
            DisponivelParaEvento = item.DisponivelParaEvento,
            DataAquisicao = item.DataAquisicao,
            ValorEstimado = item.ValorEstimado,
            Observacoes = item.Observacoes,
            TiposSugeridos = await ListarTiposSugeridosAsync(cancellationToken)
        };
    }

    private async Task<List<string>> ListarTiposDisponiveisAsync(CancellationToken cancellationToken)
    {
        return await dbContext.InventarioItens
            .AsNoTracking()
            .Where(x => x.Ativo && x.Tipo != null && x.Tipo != string.Empty)
            .Select(x => x.Tipo!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> ListarLocalizacoesDisponiveisAsync(CancellationToken cancellationToken)
    {
        return await dbContext.InventarioItens
            .AsNoTracking()
            .Where(x => x.Ativo && x.Localizacao != null && x.Localizacao != string.Empty)
            .Select(x => x.Localizacao!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<string>> ListarTiposSugeridosAsync(CancellationToken cancellationToken)
    {
        var tiposBanco = await ListarTiposDisponiveisAsync(cancellationToken);
        var tiposPadrao = Enum.GetValues<InventarioTipoTaikoEnum>()
            .Select(FormatarTipoTaiko)
            .Concat(new[] { "Bachi", "Outro" });

        return tiposPadrao
            .Concat(tiposBanco)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }

    private static string FormatarTipoTaiko(InventarioTipoTaikoEnum tipo)
    {
        return tipo switch
        {
            InventarioTipoTaikoEnum.KatsugiOke => "Katsugi Oke",
            _ => tipo.ToString()
        };
    }
}
