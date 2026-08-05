using IkkonAdmin.Web.Data;
using IkkonAdmin.Web.Enums;
using IkkonAdmin.Web.Infrastructure.Operations;
using IkkonAdmin.Web.Infrastructure.Time;
using IkkonAdmin.Web.Models.Entities;
using IkkonAdmin.Web.Models.ViewModels;
using IkkonAdmin.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace IkkonAdmin.Tests;

public class InventarioServiceTests
{
    [Fact]
    public async Task CriarAsync_CadastraItemERetornaId()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.CriarAsync(CriarForm("Nagado principal", "TAIKO-001"), usuarioId: 7);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        var itemId = Assert.IsType<int>(result.Value);
        var item = await dbContext.InventarioItens.FindAsync(itemId);
        Assert.NotNull(item);
        Assert.Equal("Nagado principal", item.Nome);
        Assert.Equal("TAIKO-001", item.CodigoInterno);
        Assert.Equal(7, item.CriadoPorUsuarioId);
        Assert.Equal(new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc), item.CriadoEmUtc);
    }

    [Fact]
    public async Task CriarAsync_ComCodigoDuplicado_RetornaValidationErrorNoCampo()
    {
        await using var dbContext = CriarDbContext();
        dbContext.InventarioItens.Add(CriarItem("Nagado", "TAIKO-001"));
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.CriarAsync(CriarForm("Outro nagado", " TAIKO-001 "), usuarioId: null);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.ValidationError, result.Status);
        var erro = Assert.Single(result.Errors);
        Assert.Equal(nameof(InventarioFormViewModel.CodigoInterno), erro.Field);
    }

    [Fact]
    public async Task AtualizarAsync_ComItemInexistente_RetornaNotFound()
    {
        await using var dbContext = CriarDbContext();
        var service = CriarService(dbContext);

        var result = await service.AtualizarAsync(999, CriarForm("Shime", "SHIME-001"), usuarioId: 3);

        Assert.False(result.Success);
        Assert.Equal(OperationResultStatus.NotFound, result.Status);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task InativarAsync_AtualizaStatusAuditoriaERetornaId()
    {
        await using var dbContext = CriarDbContext();
        var item = CriarItem("Bachi reserva", "BACHI-001");
        dbContext.InventarioItens.Add(item);
        await dbContext.SaveChangesAsync();

        var service = CriarService(dbContext);

        var result = await service.InativarAsync(item.Id, usuarioId: 11);

        Assert.True(result.Success);
        Assert.Equal(OperationResultStatus.Success, result.Status);
        Assert.Equal(item.Id, result.Value);
        var atualizado = await dbContext.InventarioItens.FindAsync(item.Id);
        Assert.NotNull(atualizado);
        Assert.False(atualizado.Ativo);
        Assert.Equal(InventarioStatusEnum.Baixado, atualizado.Status);
        Assert.Equal(11, atualizado.AtualizadoPorUsuarioId);
        Assert.Equal(new DateTime(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc), atualizado.AtualizadoEmUtc);
    }

    private static ApplicationDbContext CriarDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static InventarioService CriarService(ApplicationDbContext dbContext)
    {
        return new InventarioService(
            dbContext,
            new TestClock());
    }

    private static InventarioFormViewModel CriarForm(string nome, string codigo)
    {
        return new InventarioFormViewModel
        {
            Nome = nome,
            CodigoInterno = codigo,
            Categoria = InventarioCategoriaEnum.Taiko,
            Tipo = "Nagado",
            Quantidade = 1,
            Status = InventarioStatusEnum.Disponivel,
            EstadoConservacao = InventarioEstadoConservacaoEnum.Bom,
            DisponivelParaAula = true,
            DisponivelParaEvento = true
        };
    }

    private static InventarioItem CriarItem(string nome, string codigo)
    {
        return new InventarioItem
        {
            Nome = nome,
            CodigoInterno = codigo,
            Categoria = InventarioCategoriaEnum.Taiko,
            Tipo = "Nagado",
            Quantidade = 1,
            Status = InventarioStatusEnum.Disponivel,
            EstadoConservacao = InventarioEstadoConservacaoEnum.Bom,
            DisponivelParaAula = true,
            DisponivelParaEvento = true,
            CriadoEmUtc = new DateTime(2026, 7, 1, 12, 0, 0),
            Ativo = true
        };
    }

    private sealed class TestClock : IClock
    {
        public DateTime UtcNow { get; } = new(2026, 7, 13, 12, 0, 0, DateTimeKind.Utc);
        public DateTime Now { get; } = new(2026, 7, 13, 9, 0, 0, DateTimeKind.Local);
        public DateTime Today => Now.Date;
        public DateOnly TodayDate => DateOnly.FromDateTime(Today);
    }
}
