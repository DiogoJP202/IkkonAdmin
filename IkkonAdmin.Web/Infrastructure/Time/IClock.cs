namespace IkkonAdmin.Web.Infrastructure.Time;

public interface IClock
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
    DateTime Today { get; }
    DateOnly TodayDate { get; }
}
