using Microsoft.AspNetCore.Mvc;

namespace IkkonAdmin.Web.Infrastructure.Operations;

public static class OperationResultMvcExtensions
{
    public static IActionResult? ToFailureActionResult(
        this OperationResult result,
        ControllerBase controller)
    {
        return result.Success ? null : MapStatus(result.Status, result.Message, controller);
    }

    public static IActionResult? ToFailureActionResult<T>(
        this OperationResult<T> result,
        ControllerBase controller)
    {
        return result.Success ? null : MapStatus(result.Status, result.Message, controller);
    }

    private static IActionResult? MapStatus(
        OperationResultStatus status,
        string message,
        ControllerBase controller)
    {
        return status switch
        {
            OperationResultStatus.NotFound => controller.NotFound(),
            OperationResultStatus.Forbidden => controller.Forbid(),
            OperationResultStatus.Conflict => controller.Conflict(new { message }),
            _ => null
        };
    }
}
