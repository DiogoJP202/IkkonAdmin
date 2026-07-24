using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IkkonAdmin.Web.Infrastructure.Operations;

public static class OperationResultModelStateExtensions
{
    public static void AddToModelState(this OperationResult result, ModelStateDictionary modelState)
    {
        AddErrorsToModelState(result.Errors, result.Message, modelState);
    }

    public static void AddToModelState<T>(this OperationResult<T> result, ModelStateDictionary modelState)
    {
        AddErrorsToModelState(result.Errors, result.Message, modelState);
    }

    private static void AddErrorsToModelState(
        IReadOnlyCollection<OperationError> errors,
        string fallbackMessage,
        ModelStateDictionary modelState)
    {
        var errorsToAdd = errors.Count > 0
            ? errors
            : [new OperationError(null, fallbackMessage)];

        foreach (var error in errorsToAdd)
        {
            modelState.AddModelError(error.Field ?? string.Empty, error.Message);
        }
    }
}
