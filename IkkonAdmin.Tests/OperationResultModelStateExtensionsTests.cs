using IkkonAdmin.Web.Infrastructure.Operations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IkkonAdmin.Tests;

public class OperationResultModelStateExtensionsTests
{
    [Fact]
    public void AddToModelState_AdicionaErroNoCampoInformado()
    {
        var modelState = new ModelStateDictionary();
        var result = OperationResult.Fail("Nome já está em uso.", "Nome");

        result.AddToModelState(modelState);

        Assert.False(modelState.IsValid);
        var error = Assert.Single(modelState["Nome"]!.Errors);
        Assert.Equal("Nome já está em uso.", error.ErrorMessage);
    }

    [Fact]
    public void AddToModelState_UsaMensagemGeralQuandoNaoHaErroDetalhado()
    {
        var modelState = new ModelStateDictionary();
        var result = OperationResult<int>.Fail(
            "Não foi possível salvar.",
            Array.Empty<OperationError>());

        result.AddToModelState(modelState);

        Assert.False(modelState.IsValid);
        var error = Assert.Single(modelState[string.Empty]!.Errors);
        Assert.Equal("Não foi possível salvar.", error.ErrorMessage);
    }
}
