namespace IkkonAdmin.Web.Services;

public class GoogleAgendaOptions
{
    public string ApplicationName { get; set; } = "IkkonAdmin";
    public string? CalendarId { get; set; }
    public string? CredentialsPath { get; set; }
    public string? OAuthClientSecretsPath { get; set; }
    public string? RedirectUri { get; set; }
    public string TimeZone { get; set; } = "America/Sao_Paulo";
}
