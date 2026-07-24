namespace IkkonAdmin.Web.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
    public DateTime Today => DateTime.Today;
    public DateOnly TodayDate => DateOnly.FromDateTime(DateTime.Today);
}
