using IkkonAdmin.Web.Enums;

namespace IkkonAdmin.Web.Services;

public sealed record BlogWorkflowValidation(
    bool Success,
    string Message,
    BlogPostStatusEnum Status,
    DateTime? PublishedAtUtc,
    DateTime? ScheduledAtUtc,
    DateTime? ArchivedAtUtc)
{
    public static BlogWorkflowValidation Ok(
        BlogPostStatusEnum status,
        DateTime? publishedAtUtc,
        DateTime? scheduledAtUtc,
        DateTime? archivedAtUtc)
        => new(true, string.Empty, status, publishedAtUtc, scheduledAtUtc, archivedAtUtc);

    public static BlogWorkflowValidation Fail(string message)
        => new(false, message, BlogPostStatusEnum.Draft, null, null, null);
}
