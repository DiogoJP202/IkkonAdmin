namespace IkkonAdmin.Web.Infrastructure.Operations;

public sealed record OperationResult(
    bool Success,
    string Message,
    IReadOnlyCollection<OperationError> Errors,
    OperationResultStatus Status)
{
    public static OperationResult Ok(string message)
    {
        return new OperationResult(true, message, Array.Empty<OperationError>(), OperationResultStatus.Success);
    }

    public static OperationResult Fail(string message, string? field = null)
    {
        return new OperationResult(false, message, [new OperationError(field, message)], OperationResultStatus.ValidationError);
    }

    public static OperationResult Fail(string message, IReadOnlyCollection<OperationError> errors)
    {
        return new OperationResult(false, message, errors, OperationResultStatus.ValidationError);
    }

    public static OperationResult NotFound(string message)
    {
        return new OperationResult(false, message, Array.Empty<OperationError>(), OperationResultStatus.NotFound);
    }
}

public sealed record OperationResult<T>(
    bool Success,
    string Message,
    T? Value,
    IReadOnlyCollection<OperationError> Errors,
    OperationResultStatus Status)
{
    public static OperationResult<T> Ok(T value, string message)
    {
        return new OperationResult<T>(true, message, value, Array.Empty<OperationError>(), OperationResultStatus.Success);
    }

    public static OperationResult<T> Fail(string message, string? field = null)
    {
        return new OperationResult<T>(false, message, default, [new OperationError(field, message)], OperationResultStatus.ValidationError);
    }

    public static OperationResult<T> Fail(string message, IReadOnlyCollection<OperationError> errors)
    {
        return new OperationResult<T>(false, message, default, errors, OperationResultStatus.ValidationError);
    }

    public static OperationResult<T> NotFound(string message)
    {
        return new OperationResult<T>(false, message, default, Array.Empty<OperationError>(), OperationResultStatus.NotFound);
    }
}
