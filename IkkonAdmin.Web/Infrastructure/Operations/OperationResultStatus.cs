namespace IkkonAdmin.Web.Infrastructure.Operations;

public enum OperationResultStatus
{
    Success = 1,
    ValidationError = 2,
    NotFound = 3,
    Forbidden = 4,
    Conflict = 5
}
