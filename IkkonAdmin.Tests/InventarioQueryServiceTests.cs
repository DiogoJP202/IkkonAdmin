using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class InventarioQueryServiceTests
{
    [Fact]
    public async Task ListarAsync_AplicaFiltrosEPreservaIndicadoresGlobais()
    {
        await using var dbContext = CriarDbContext();
        dbContext.InventarioItens.AddRange(
            CriarItem("Nagado principal", "TAIKO-NAGADO-001", InventarioCategoriaEnum.Taiko, "Nagado", "Dojo", InventarioStatusEnum.Disponivel),
            CriarItem("Shime reserva", "TAIKO-SHIME-002", InventarioCategoriaEnum.Taiko, "Shime", "Manutencao", InventarioStatusEnum.Manutencao),
            CriarItem("Bachi aula", "BACHI-001", InventarioCategoriaEnum.Bachi, "Bachi", "Armario", InventarioStatusEnum.Indisponivel),
            CriarItem("Odaiko baixado", "TAIKO-ODAIKO-003", InventarioCategoriaEnum.Taiko, "Odaiko", "Deposito", InventarioStatusEnum.Baixado, ativo: false));
        await dbContext.SaveChangesAsync();

        var service = new InventarioQueryService(dbContext);

        var resultado = await service.ListarAsync(new InventarioFiltroViewModel
        {
            Busca = "TAIKO",
            Categoria = InventarioCategoriaEnum.Taiko,
            Status = InventarioStatusEnum.Disponivel,
            TamanhoPagina = 999
        });

        var item = Assert.Single(resultado.Itens);
        Assert.Equal("Nagado principal", item.Nome);
        Assert.Equal(1, resultado.TotalRegistros);
        Assert.Equal(3, resultado.TotalItens);
        Assert.Equal(1, resultado.ItensDisponiveis);
        Assert.Equal(1, resultado.ItensManutencao);
        Assert.Equal(1, resultado.ItensIndisponiveis);
        Assert.Equal(20, resultado.TamanhoPagina);
        Assert.Contains("Nagado", resultado.TiposDisponiveis);
        Assert.Contains("Dojo", resultado.LocalizacoesDisponiveis);
        Assert.DoesNotContain("Odaiko", resultado.TiposDisponiveis);
    }

    [Fact]
    public async Task ObterDetalhesAsync_RetornaAuditoriaEMovimentacoesRecentes()
    {
        await using var dbContext = CriarDbContext();
        var responsavel = new UsuarioSistema
        {
            Login = "admin",
            LoginNormalizado = "ADMIN",
            NomeExibicao = "Administrador Ikkon",
            SenhaHash = "hash",
            TipoAcesso = TipoAcessoEnum.Admin,
            Ativo = true
        };
        var item = CriarItem(
            "Shime em manutencao",
            "TAIKO-SHIME-001",
            InventarioCategoriaEnum.Taiko,
            "Shime",
            "Manutencao",
            InventarioStatusEnum.Manutencao);
        item.CriadoPorUsuario = responsavel;
        item.Movimentacoes.Add(new InventarioMovimentacao
        {
            TipoMovimentacao = InventarioTipoMovimentacaoEnum.Reserva,
            Quantidade = 1,
            DataInicioUtc = new DateTime(2026, 7, 1, 12, 0, 0),
            ResponsavelUsuario = responsavel,
            Observacoes = "Reserva antiga"
        });
        item.Movimentacoes.Add(new InventarioMovimentacao
        {
            TipoMovimentacao = InventarioTipoMovimentacaoEnum.Manutencao,
            Quantidade = 1,
            DataInicioUtc = new DateTime(2026, 7, 10, 12, 0, 0),
            ResponsavelUsuario = responsavel,
            Observacoes = "Troca de corda"
        });

        dbContext.AddRange(responsavel, item);
        await dbContext.SaveChangesAsync();

        var service = new InventarioQueryService(dbContext);

        var detalhes = await service.ObterDetalhesAsync(item.Id);

        Assert.NotNull(detalhes);
        Assert.Equal("Shime em manutencao", detalhes.Nome);
        Assert.Equal("Administrador Ikkon", detalhes.CriadoPorNome);
        Assert.Equal(2, detalhes.MovimentacoesRecentes.Count);
        Assert.Equal(InventarioTipoMovimentacaoEnum.Manutencao, detalhes.MovimentacoesRecentes[0].TipoMovimentacao);
        Assert.Equal("Administrador Ikkon", detalhes.MovimentacoesRecentes[0].ResponsavelNome);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static InventarioItem CriarItem(
        string nome,
        string codigo,
        InventarioCategoriaEnum categoria,
        string tipo,
        string localizacao,
        InventarioStatusEnum status,
        bool ativo = true)
    {
        return new InventarioItem
        {
            Nome = nome,
            CodigoInterno = codigo,
            Categoria = categoria,
            Tipo = tipo,
            Localizacao = localizacao,
            Quantidade = 1,
            Status = status,
            EstadoConservacao = InventarioEstadoConservacaoEnum.Bom,
            DisponivelParaAula = true,
            DisponivelParaEvento = true,
            CriadoEmUtc = new DateTime(2026, 7, 1, 12, 0, 0),
            Ativo = ativo
        };
    }
}
