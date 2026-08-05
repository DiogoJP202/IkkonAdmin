using IkkonAdmin.Web.Infrastructure.Operations;
using Microsoft.AspNetCore.Http;

namespace IkkonAdmin.Web.Infrastructure.Files;

public interface IDocumentFileValidator
{
    Task<OperationResult<DocumentFileValidationResult>> ValidateAsync(
        IFormFile file,
        CancellationToken cancellationToken = default);
}

public sealed record DocumentFileValidationResult(
    string Extension,
    string ContentType,
    string SafeOriginalFileName);
