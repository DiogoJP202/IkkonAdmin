using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace IkkonAdmin.Web.Infrastructure.Operations;

public static class OperationResultTempDataExtensions
{
    private const string SuccessKey = "Success";
    private const string ErrorKey = "Error";

    public static void AddToTempData(
        this OperationResult result,
        ITempDataDictionary tempData,
        string? successMessage = null,
        string? errorMessage = null)
    {
        tempData[result.Success ? SuccessKey : ErrorKey] = result.Success
            ? successMessage ?? result.Message
            : errorMessage ?? result.Message;
    }

    public static void AddToTempData<T>(
        this OperationResult<T> result,
        ITempDataDictionary tempData,
        string? successMessage = null,
        string? errorMessage = null)
    {
        tempData[result.Success ? SuccessKey : ErrorKey] = result.Success
            ? successMessage ?? result.Message
            : errorMessage ?? result.Message;
    }
}
