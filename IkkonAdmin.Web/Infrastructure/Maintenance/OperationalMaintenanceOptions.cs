namespace IkkonAdmin.Web.Infrastructure.Maintenance;

public sealed class OperationalMaintenanceOptions
{
    public const string SectionName = "OperationalMaintenance";

    public bool Enabled { get; set; } = true;
    public bool RunOnStartup { get; set; } = true;
    public int HourLocal { get; set; } = 3;
    public int MinuteLocal { get; set; }
}
