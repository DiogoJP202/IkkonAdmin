using IkkonAdmin.Web.Infrastructure.Operations;
using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Tests;

public class OperationResultTests
{
    [Fact]
    public void Factories_RepresentamTodosOsEstadosSuportados()
    {
        Assert.Equal(OperationResultStatus.Success, OperationResult.Ok("ok").Status);
        Assert.Equal(OperationResultStatus.ValidationError, OperationResult.Fail("inválido").Status);
        Assert.Equal(OperationResultStatus.NotFound, OperationResult.NotFound("ausente").Status);
        Assert.Equal(OperationResultStatus.Forbidden, OperationResult.Forbidden("negado").Status);
        Assert.Equal(OperationResultStatus.Conflict, OperationResult.Conflict("conflito").Status);

        Assert.Equal(OperationResultStatus.Forbidden, OperationResult<int>.Forbidden("negado").Status);
        Assert.Equal(OperationResultStatus.Conflict, OperationResult<int>.Conflict("conflito").Status);
    }

    [Fact]
    public void ToFailureActionResult_ConverteStatusHttpEstruturais()
    {
        var controller = new TestController();

        Assert.IsType<NotFoundResult>(OperationResult.NotFound("ausente").ToFailureActionResult(controller));
        Assert.IsType<ForbidResult>(OperationResult.Forbidden("negado").ToFailureActionResult(controller));
        Assert.IsType<ConflictObjectResult>(OperationResult.Conflict("duplicado").ToFailureActionResult(controller));
        Assert.Null(OperationResult.Fail("formulário").ToFailureActionResult(controller));
        Assert.Null(OperationResult.Ok("ok").ToFailureActionResult(controller));
    }

    [Fact]
    public void ResultadoGenerico_PreservaValorDaOperacao()
    {
        var result = OperationResult<int>.Ok(42, "criado");

        Assert.True(result.Success);
        Assert.Equal(42, result.Value);
    }

    private sealed class TestController : ControllerBase;
}
