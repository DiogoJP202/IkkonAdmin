using System.Text.Json;

namespace IkkonAdmin.Web.Infrastructure.Auditing;

public static class AuditJson
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, Options);
    }
}
