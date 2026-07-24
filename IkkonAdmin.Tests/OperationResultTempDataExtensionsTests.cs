using IkkonAdmin.Web.Infrastructure.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace IkkonAdmin.Tests;

public class OperationResultTempDataExtensionsTests
{
    [Fact]
    public void AddToTempData_GravaMensagemNaChaveSuccess()
    {
        var tempData = CriarTempData();
        var result = OperationResult.Ok("Operação concluída.");

        result.AddToTempData(tempData);

        Assert.Equal("Operação concluída.", tempData["Success"]);
        Assert.False(tempData.ContainsKey("Error"));
    }

    [Fact]
    public void AddToTempData_GravaMensagemNaChaveErrorQuandoFalha()
    {
        var tempData = CriarTempData();
        var result = OperationResult<int>.Fail("Revise os dados.");

        result.AddToTempData(tempData);

        Assert.Equal("Revise os dados.", tempData["Error"]);
        Assert.False(tempData.ContainsKey("Success"));
    }

    [Fact]
    public void AddToTempData_PermiteMensagemCustomizadaDeSucesso()
    {
        var tempData = CriarTempData();
        var result = OperationResult.Ok("Mensagem padrão.");

        result.AddToTempData(tempData, successMessage: "Mensagem customizada.");

        Assert.Equal("Mensagem customizada.", tempData["Success"]);
        Assert.False(tempData.ContainsKey("Error"));
    }

    private static TempDataDictionary CriarTempData()
    {
        return new TempDataDictionary(
            new DefaultHttpContext(),
            new TestTempDataProvider());
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context)
        {
            return new Dictionary<string, object>();
        }

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
